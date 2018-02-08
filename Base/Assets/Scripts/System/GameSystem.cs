using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class GameSystem : MonoBehaviour
{
    private static GameSystem s_instance;
    static public GameSystem Instance
    {
        get
        {
            if (null == s_instance)
            {
                s_instance = FindObjectOfType(typeof(GameSystem)) as GameSystem;
                if (null == s_instance)
                {
                    Debug.LogError("Failed To Find GameSystem Instance");
                }
            }
            return s_instance;
        }
    }

    public static eLanguage language = eLanguage.KOR;

    private bool UseServerBundle = false;//false; //Server에 저장된 AssetBundle을 사용할 지 여부

    public bool UseBundleTable { get { return _useBundleTable; } }
    private bool _useBundleTable = false; //Table, Txt, Json, Lua의 AssetBundle사용여부

    public bool UseBundleUI { get { return _useBundleUI; } }
    private bool _useBundleUI = false; //Table, Txt, Json, Lua의 AssetBundle사용여부

    public float deltaTime { get { return _deltaTime; } }
    private float _deltaTime;

    public Transform screen2D { get { return _screen2D; } }
    private Transform _screen2D;
    public Transform screen3D { get { return _screen3D; } }
    private Transform _screen3D;

    public Camera camera2D { get { return _camera2D; } }
    private Camera _camera2D;
    public Camera camera3D { get { return _camera3D; } }
    private Camera _camera3D;

    public PanelMain panelMain { get { return _panelMain; } }
    private PanelMain _panelMain;
    public Transform tfPanelMain { get { return _tfPanelMain; } }
    private Transform _tfPanelMain;

    private BaseScene _currentScene;
    private eSceneType _currentSceneType;

    private GameObject _blocker;
    private bool _showBlocker = false;
    private float _blockFadeTime, _bloclFadeFullTime;

    private Action _actOnSceneChanged = null;



    void Awake()
    {
        //_useBundleTable = true;
        //_useBundleUI = true;

        gameObject.AddComponent<ABDownloader>();
        ABDownloader.Instance.rootUrl
            = Path.Combine(UseServerBundle ? "http://yourUrl" : "file://" + Application.dataPath.Replace("Assets", ABUtil.PATH_OUTPUT), ABUtil.GetPlatformName());
        Debug.Log("rootUrl : " + ABDownloader.Instance.rootUrl);

        CachingPreference();

        gameObject.AddComponent<AssetLoader>();

        DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<RotationManager>();
        gameObject.AddComponent<SoundManager>();
        gameObject.AddComponent<DataProvider>();

        _screen2D = GameObject.Find("Screen2D").transform;
        DontDestroyOnLoad(_screen2D.gameObject);
        _camera2D = _screen2D.transform.Find("Camera2D").GetComponent<Camera>();

        _screen3D = GameObject.Find("Screen3D").transform;
        DontDestroyOnLoad(_screen3D.gameObject);
        _camera3D = _screen3D.transform.Find("Camera3D").GetComponent<Camera>();

        _panelMain = GameObject.Find("PanelMain").AddComponent<PanelMain>();
        _panelMain.ClearAll();
        _tfPanelMain = _panelMain.transform;
    }

    void Update()
    {
        _deltaTime = Time.deltaTime;
        UpdateBlocker();
    }

    private void CachingPreference()
    {
        Caching.compressionEnabled = true;

#if UNITY_EDITOR
        //Caching space 4GB
        Caching.maximumAvailableDiskSpace = 1073700000 * (long)4;
#else
        //Caching space 1GB
        Caching.maximumAvailableDiskSpace = 1073700000;
#endif

        Debug.Log("Caching.maximumAvailableDiskSpace : " + Caching.maximumAvailableDiskSpace);
        Debug.Log("Caching.spaceOccupied : " + Caching.spaceOccupied);
        Debug.Log("Caching.spaceFree : " + Caching.spaceFree);
    }

    IEnumerator Start()
    {
        RotationManager.Instance.OrientationChanged();
        RotationManager.Instance.SetLockRotation(false);

        yield return ABDownloader.Instance.Initialize();

        yield return LoadBlocker();

        ShowBlocker(0.0f, eBlockerType.Black);

        ChangeScene(eSceneType.Title);
    }

    #region Scene
    // 씬 전환이 필요할때 쓰는 함수.
    public void ChangeScene(eSceneType _eScene, Action actOnSceneChanged = null)
    {
        Debug.Log(name + " ChangeScene : " + _eScene);
        if (_currentScene != null)
        {
            if (_currentSceneType == _eScene)
            {
                Debug.LogWarning(name + " try to Change same for CurrentScene : " + _currentSceneType);
                return;
            }
        }
        _currentSceneType = _eScene;
        _actOnSceneChanged = actOnSceneChanged;

        StopCoroutine(CrtChangeScene());
        StartCoroutine(CrtChangeScene());
    }

    IEnumerator CrtChangeScene()
    {
        float blockerShowTime = 0.3f;
        ShowBlocker(blockerShowTime, eBlockerType.Black);
        yield return new WaitForSeconds(blockerShowTime);

        // 이전 씬의 오브젝트를 모두 지운다.
        if (_currentScene != null)
        {
            yield return _currentScene.Terminate();
            Destroy(_currentScene);
        }

        switch (_currentSceneType)
        {
            case eSceneType.Title:
                _currentScene = gameObject.AddComponent<TitleScene>();
                break;
            case eSceneType.Town:
                _currentScene = gameObject.AddComponent<TownScene>();
                break;
        }

        if (null == _currentScene)
            Debug.LogError("Current Scene is null. \nScene Type: " + _currentSceneType.ToString());
        else
        {
            yield return _currentScene.Initialize();

            if (_actOnSceneChanged != null)
                _actOnSceneChanged();

            _actOnSceneChanged = null;
            HideBlocker(0.3f);
        }
    }
    #endregion

    #region Blocker
    IEnumerator LoadBlocker()
    {
        string prefabPath = "UI/Blocker/Blocker";
        yield return AssetLoader.LoadUI(prefabPath, camera2D.transform, null);
        _blocker = camera2D.transform.Find("Blocker").gameObject;
        _blocker.GetComponent<UIPanel>().depth = UIDepthUtil.GetDepth(eUIDepth.Blocker);
        _blocker.SetActive(false);
    }

    private void UpdateBlocker()
    {
        if (0.0f >= _blockFadeTime) return;

        _blockFadeTime -= deltaTime;
        if (0.0f >= _blockFadeTime)
        {
            if (_showBlocker)
            {
                _blocker.GetComponent<UIPanel>().alpha = 1.0f;
            }
            else
            {
                _blocker.SetActive(false);
            }
            return;
        }

        if (_showBlocker)
        {
            _blocker.GetComponent<UIPanel>().alpha = (_bloclFadeFullTime - _blockFadeTime) / _bloclFadeFullTime;
        }
        else
        {
            _blocker.GetComponent<UIPanel>().alpha = _blockFadeTime / _bloclFadeFullTime;
        }
    }

    public void ShowBlocker(float fadeTime, eBlockerType blockerType)
    {
        if (_showBlocker) return;

        _showBlocker = true;
        if (!_blocker.activeSelf)
        {
            _blocker.SetActive(true);
            _blocker.GetComponent<UIPanel>().alpha = 1.0f;
        }

        switch (blockerType)
        {
            case eBlockerType.Black:
                _blocker.transform.Find("Black").gameObject.SetActive(true);
                _blocker.transform.Find("Circle").gameObject.SetActive(false);
                break;
            case eBlockerType.Circle:
                _blocker.transform.Find("Black").gameObject.SetActive(false);
                _blocker.transform.Find("Circle").gameObject.SetActive(true);
                break;
        }

        _blocker.GetComponent<BoxCollider>().enabled = _showBlocker;
        _bloclFadeFullTime = _blockFadeTime = fadeTime;
        //_blocker.GetComponent<UIPanel>().alpha = _showBlocker ? 0.1f : 1.0f;
    }

    public void HideBlocker(float fadeTime)
    {
        if (!_showBlocker) return;

        _showBlocker = false;
        _blocker.GetComponent<BoxCollider>().enabled = _showBlocker;
        _bloclFadeFullTime = _blockFadeTime = fadeTime;
        //_blocker.GetComponent<UIPanel>().alpha = _showBlocker ? 0.1f : 1.0f;
    }
    #endregion
}
