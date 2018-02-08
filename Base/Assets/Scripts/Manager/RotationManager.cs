using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIRelativeScale : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR
        RotationManager.Instance.RegisterUIRelativeScale(this);
#endif
        ChangeScale();
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        RotationManager.Instance.RegisterUIRelativeScale(this);
#endif
        ChangeScale();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        RotationManager.Instance.UnregisterUIRelativeScale(this);
#endif
    }

    public void ChangeScale()
    {
        transform.localScale = new Vector3(RotationManager.Instance.relativeScale, RotationManager.Instance.relativeScale, 1.0f);

        UIStretch[] uiStretchArr = transform.GetComponentsInChildren<UIStretch>();

        for (int i = 0; i < uiStretchArr.Length; i++)
        {
            uiStretchArr[i].relativeSize = new Vector2(1.0f / transform.localScale.x, 1.0f / transform.localScale.y);
        }
    }
}

public class RotationManager : MonoBehaviour
{
#if UNITY_EDITOR
    //에디터에서 orientation에 따라 자동으로 해상도 변경 수행여부
    private bool RESOLUTION_AUTOSET = false;
#endif

    public const string KEY_LOCK_ROTATION = "lock_rotation"; //0 : Unlocked, 1 : Lock Portrait, 2 : Lock Landscape
    private const string KEY_CONTROL_HAND = "control_hand"; //0 : left, 1 : right

    public const int STANDARD_WIDTH = 1280;
    public const int STANDARD_HEIGHT = 720;

    private static RotationManager s_instance;
    static public RotationManager Instance
    {
        get
        {
            if (null == s_instance)
            {
                Debug.LogWarning("Failed To Find RotationManager Instance");
            }
            return s_instance;
        }
    }

    /// <summary>
    /// 현재 Orientation 상태. ScreenOrientation enum의 Landscape, Portrait 두개만 사용한다.
    /// </summary>
    public ScreenOrientation currentGameOrientation { get; private set; }
    private DeviceOrientation lastDeviceOrientation;

    public bool isLandscape { get { return ScreenOrientation.Portrait != currentGameOrientation; } }
    public bool isLockRotation { get; private set; }
    public int uirootWidth { get; private set; }
    public int uirootHeight { get; private set; }
    public float widthRatio { get; private set; }
    public float heightRatio { get; private set; }

    public float relativeScale { get; private set; } // 16:9보다 가로비가 더 클 경우 scale변경 값

    public delegate void OrientationChangedListener();
    public OrientationChangedListener OnOrientationChanged;

    //OrientationChanged수행에 수 프레임이 소요되므로, 수행중에 재호출될 시에 오동작을 방지하기 위해 순차적으로 수행하도록 함
    private bool _processingOrientationChanged = false;
    private int _callCountOrientationChanged = 0;

    private int m_resolutionX, m_resolutionY;
    private int m_graphicLevel = 1;

    private bool m_bSavedBackup;
    private string m_backupTag;
    private bool m_backupIsLockRotation;
    private bool[] m_backupAutoRotateState = new bool[4];

    private float m_deviceWidth = 0.0f;
    private float m_deviceHeight = 0.0f;

    public float PIXEL_GAIN { get { return m_pixelGain; } }
    private float m_pixelGain = 1.0f;	//ratio of Screen resolution and Game resolution

    void Awake()
    {
        s_instance = FindObjectOfType(typeof(RotationManager)) as RotationManager;

        LoadPrefLockRotation();
        currentGameOrientation = ScreenOrientation.Landscape;
        switch (Screen.orientation)
        {
            case ScreenOrientation.LandscapeRight:
                lastDeviceOrientation = DeviceOrientation.LandscapeRight;
                break;
            default:
                lastDeviceOrientation = DeviceOrientation.LandscapeLeft;
                break;
        }
        relativeScale = 1.0f;
    }

    void Update()
    {
        // 튜토리얼 회전 막기 코드. 버그 라서 삭제 예정
        //if ((GameManager.instance != null && GameManager.instance.IsTutorialWidgetActive()) || Overlay.instance.IsActiveWebView())
        //	return;		==> 이걸 하면 디바이스에서 돌아갔는데 아래 부분 처리를 못해서 화면이 이상해진다.

#if UNITY_EDITOR
        //if (CheatManager.Instance != null && CheatManager.Instance.CanHandleCheatKeyInput())
        //{
        //if (!isLockRotation)
        //{
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyUp(KeyCode.P))
            {
                TestRotate(false);
            }
            else if (Input.GetKeyUp(KeyCode.L))
            {
                TestRotate(true);
            }
        }
        //}

        if (Input.GetKeyUp(KeyCode.O))
        {
            OrientationChanged();
        }
        //}

        CheckAutoRotate();
#else
        CheckAutoRotate();
#endif
    }

#if !UNITY_EDITOR
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            OrientationChanged();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
            OrientationChanged();
    }
#endif

#if UNITY_EDITOR
    public static UnityEditor.EditorWindow GetMainGameView()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetMainGameView.Invoke(null, null);
        return (UnityEditor.EditorWindow)Res;
    }

    public void TestRotate(bool isLandscape)
    {
        //currentGameOrientation = isLandscape ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;

        UnityEditor.EditorWindow mainView = GetMainGameView();

        Rect rect = mainView.position;
        rect.position = new Vector2(100, 25);
        rect.width = isLandscape ? 1024 : 576;
        rect.height = (isLandscape ? 576 : 1024) + 17;

        //rect.width = isLandscape ? 1024 : 512;
        //rect.height = (isLandscape ? 512 : 1024) + 17;

        //rect.width = isLandscape ? 1024 : 498;
        //rect.height = (isLandscape ? 498 : 1024) + 17;

        mainView.position = rect;
        mainView.Repaint();

        /* 높이를 유지하고 같은 비율이 되도록 가로 크기만 조절 하는 코드

        if (m_viewAspect == 0)
            m_viewAspect = rect.width / rect.height;

        if (isLandscape)
        {
            float width = (float)rect.height * m_viewAspect;
            rect.width = (int)width;
        }
        else
        {
            float width = (float)rect.height / m_viewAspect;
            rect.width = (int)width;
        }
        mainView.position = rect;
        mainView.Repaint();
        */

        //yield return new WaitForEndOfFrame();
    }

    private List<UIRelativeScale> uiRelativeScaleList = new List<UIRelativeScale>();
    public void RegisterUIRelativeScale(UIRelativeScale uiRelativeScale)
    {
        uiRelativeScaleList.Add(uiRelativeScale);
    }

    public void UnregisterUIRelativeScale(UIRelativeScale uiRelativeScale)
    {
        uiRelativeScaleList.Remove(uiRelativeScale);
    }

#endif

#if UNITY_STANDALONE && !UNITY_EDITOR

    public void TestRotate(bool isLandscape)
    {    
        StartCoroutine(CrtTestRotate(isLandscape));
    }
    IEnumerator CrtTestRotate(bool isLandscape)
    {
        currentGameOrientation = isLandscape ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
        Screen.SetResolution(isLandscape ? 1024 : 576, isLandscape ? 576 : 1024, false);
        yield return null;
        yield return null;

        OrientationChanged();
        yield return null;
    }

#endif

    private string DevOrientationToString(DeviceOrientation devOrient)		// ==> ToString 이 동작 하지 않아서 코딩 했음
    {
        switch (devOrient)
        {
            case DeviceOrientation.Portrait:
                return "Portrait";
            case DeviceOrientation.PortraitUpsideDown:
                return "PortraitUpsideDown";
            case DeviceOrientation.LandscapeLeft:
                return "LandscapeLeft";
            case DeviceOrientation.LandscapeRight:
                return "LandscapeRight";
            case DeviceOrientation.FaceUp:
                return "FaceUp";
            case DeviceOrientation.FaceDown:
                return "FaceDown";
        }

        return "Unknown";
    }

    //ScreenOrientation을 고정한다.
    public void SetLockRotation(bool lockLandscape, bool savePrefs = false)
    {
        Debug.Log("[SetLockRotation] " + lockLandscape);
        isLockRotation = true;

        Screen.autorotateToPortrait = !lockLandscape;
        Screen.autorotateToPortraitUpsideDown = !lockLandscape;
        Screen.autorotateToLandscapeLeft = lockLandscape;
        Screen.autorotateToLandscapeRight = lockLandscape;

        ScreenOrientation prevOrientation = currentGameOrientation;
        if (lockLandscape)
        {
            currentGameOrientation = ScreenOrientation.Landscape;
            //m_text = DevOrientationToString(Input.deviceOrientation) + " / SetLockRotation";
            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.LandscapeLeft:
                    Screen.orientation = ScreenOrientation.LandscapeLeft;
                    break;
                case DeviceOrientation.LandscapeRight:
                    Screen.orientation = ScreenOrientation.LandscapeRight;
                    break;
                default:
                    switch (lastDeviceOrientation)
                    {
                        case DeviceOrientation.LandscapeRight:
                            Screen.orientation = ScreenOrientation.LandscapeRight;
                            break;
                        default:
                            Screen.orientation = ScreenOrientation.LandscapeLeft;
                            break;
                    }
                    break;
            }
        }
        else
        {
            currentGameOrientation = ScreenOrientation.Portrait;
            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.PortraitUpsideDown:
                    Screen.orientation = ScreenOrientation.PortraitUpsideDown;
                    break;
                default:
                    switch (lastDeviceOrientation)
                    {
                        case DeviceOrientation.PortraitUpsideDown:
                            Screen.orientation = ScreenOrientation.PortraitUpsideDown;
                            break;
                        default:
                            Screen.orientation = ScreenOrientation.Portrait;
                            break;
                    }
                    break;
            }
        }

#if UNITY_EDITOR
        if (RESOLUTION_AUTOSET)
        {
            TestRotate(ScreenOrientation.Landscape == currentGameOrientation ? true : false);
        }
        else
        {
            if (prevOrientation != currentGameOrientation)
            {
                OrientationChanged();
            }
        }
#elif UNITY_ANDROID
        if (prevOrientation != currentGameOrientation)
        {
            OrientationChanged();
        }
        SetScreenOrientationAuto();
#elif UNITY_IPHONE
        if (prevOrientation != currentGameOrientation)
        {
            OrientationChanged();
        }
        Invoke("SetScreenOrientationAuto", 0.3f);
#endif

        if (savePrefs)
        {
            SavePrefLockRotation();
        }
    }

    // 현재 방향 대로 잠근다. pref에 저장
    public void LockCurrentRotationSavePref()
    {
        SetLockRotation(isLandscape, true);
    }

    // 현재 방향 대로 잠근다. pref에 저장 안하도록 (임시 잠금 목적임, RestoreLockRotationByPref 로 쉽게 복구 할 수 있다.)
    public void LockCurrentRotationInstant()
    {
        SetLockRotation(isLandscape, false);
    }

    // Pref에 저장된 값에 따라 로테이션을 세팅한다.
    public void RestoreLockRotationByPref()
    {
        switch (GetPrefLockRotation())
        {
            case 0: //Auto
                SetAutoRotation();
                break;
            case 1: //Landscape
                SetLockRotation(true);
                break;
            case 2: //Portrait
                SetLockRotation(false);
                break;
        }
    }

    private void SetScreenOrientationAuto()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
    }

    //ScreenOrientation을 AutoRotation으로 변경
    public void SetAutoRotation(bool savePrefs = false)
    {
        //m_text = Input.deviceOrientation.ToString() + "SetAutoRotation";

        isLockRotation = false;

        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

#if !UNITY_EDITOR
        ScreenOrientation prevOrientation = currentGameOrientation;

        switch (Input.deviceOrientation)
        {
            case DeviceOrientation.Portrait:
                Screen.orientation = ScreenOrientation.Portrait;
                currentGameOrientation = ScreenOrientation.Portrait;
                break;
            case DeviceOrientation.PortraitUpsideDown:
                Screen.orientation = ScreenOrientation.PortraitUpsideDown;
                currentGameOrientation = ScreenOrientation.Portrait;
                break;
            case DeviceOrientation.LandscapeLeft:
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                currentGameOrientation = ScreenOrientation.Landscape;
                break;
            case DeviceOrientation.LandscapeRight:
                Screen.orientation = ScreenOrientation.LandscapeRight;
                currentGameOrientation = ScreenOrientation.Landscape;
                break;
        }

        if (prevOrientation != currentGameOrientation)
        {
            OrientationChanged();
        }
#endif

        SetScreenOrientationAuto();

        if (savePrefs)
        {
            SavePrefLockRotation();
        }
    }

    private void CheckAutoRotate()
    {
        switch (Input.deviceOrientation)
        {
            case DeviceOrientation.Portrait:
            case DeviceOrientation.PortraitUpsideDown:
            case DeviceOrientation.LandscapeLeft:
            case DeviceOrientation.LandscapeRight:
                lastDeviceOrientation = Input.deviceOrientation;
                break;
        }

        if (isLockRotation) return;

        ScreenOrientation prevOrientation = currentGameOrientation;

        switch (prevOrientation)
        {
            case ScreenOrientation.Landscape:
            case ScreenOrientation.LandscapeRight:
                if (Screen.width < Screen.height)
                {
                    currentGameOrientation = ScreenOrientation.Portrait;
                }
#if UNITY_IPHONE
                else
                {
                    switch (Input.deviceOrientation)
                    {
                        case DeviceOrientation.Portrait:
                        case DeviceOrientation.PortraitUpsideDown:
                            currentGameOrientation = ScreenOrientation.Portrait;
                            break;
                    }
                }
#endif
                break;
            case ScreenOrientation.Portrait:
            case ScreenOrientation.PortraitUpsideDown:
                if (Screen.width > Screen.height)
                {
                    currentGameOrientation = ScreenOrientation.Landscape;
                }
#if UNITY_IPHONE
                else
                {
                    switch (Input.deviceOrientation)
                    {
                        case DeviceOrientation.LandscapeLeft:
                        case DeviceOrientation.LandscapeRight:
                            currentGameOrientation = ScreenOrientation.Landscape;
                            break;
                    }
                }
#endif
                break;
        }

        //m_text = currentGameOrientation + " " + Screen.width + " " + Screen.height;

        if (prevOrientation != currentGameOrientation)
        {
            OrientationChanged();
        }
        //#endif
    }

    public void OrientationChanged()
    {
        if (_processingOrientationChanged)
        {
            _callCountOrientationChanged++;
        }
        else
        {
            _processingOrientationChanged = true;
            if (0 < _callCountOrientationChanged)
                _callCountOrientationChanged--;

            Debug.Log("[RotationManager] OrientationChanged CallCount : " + _callCountOrientationChanged);

            StartCoroutine(CrtOrientationChanged());
        }
    }

    IEnumerator CrtOrientationChanged()
    {
        if (0.0f == m_deviceWidth)
        {
            m_deviceWidth = Screen.width > Screen.height ? Screen.width : Screen.height;
            m_deviceHeight = Screen.width > Screen.height ? Screen.height : Screen.width;
        }

        /*
        switch (m_graphicLevel)
        {
            case 0: //high
                if (PlatformManager.Instance.GetTotalMemoryLevel() < 2)
                {
                    m_resolutionX = m_deviceWidth < 960 ? (int)m_deviceWidth : 960;
                    m_resolutionY = (int)((m_deviceHeight / m_deviceWidth) * m_resolutionX);

                    //m_resolutionX = m_deviceWidth < STANDARD_WIDTH ? (int)m_deviceWidth : STANDARD_WIDTH;
                    //m_resolutionY = (int)((m_deviceHeight / m_deviceWidth) * m_resolutionX);
                }
                else
                {
                    m_resolutionX = (int)m_deviceWidth;
                    m_resolutionY = (int)m_deviceHeight;
                }
                break;
            case 1: //medium
            case 2: //low
            default:
                if (PlatformManager.Instance.GetTotalMemoryLevel() < 2)
                {
                    m_resolutionX = m_deviceWidth < 960 ? (int)m_deviceWidth : 960;
                    m_resolutionY = (int)((m_deviceHeight / m_deviceWidth) * m_resolutionX);
                }
                else
                {
                    m_resolutionX = m_deviceWidth < STANDARD_WIDTH ? (int)m_deviceWidth : STANDARD_WIDTH;
                    m_resolutionY = (int)((m_deviceHeight / m_deviceWidth) * m_resolutionX);
                }
                break;
        }
        */

        switch (m_graphicLevel)
        {
            case 0: //high
                m_resolutionX = (int)m_deviceWidth;
                m_resolutionY = (int)m_deviceHeight;
                break;
            case 1: //medium
            case 2: //low
            default:
                m_resolutionX = m_deviceWidth < STANDARD_WIDTH ? (int)m_deviceWidth : STANDARD_WIDTH;
                m_resolutionY = (int)((m_deviceHeight / m_deviceWidth) * m_resolutionX);
                break;
        }

        bool isFullScreen = Screen.fullScreen;
        Screen.SetResolution(isLandscape ? m_resolutionX : m_resolutionY, isLandscape ? m_resolutionY : m_resolutionX, isFullScreen);

        GameObject goCamera = GameObject.Find("Camera2D");
        Camera cam = null;
        if (null != goCamera)
        {
            cam = goCamera.transform.GetComponent<Camera>();
            cam.enabled = false;
        }

        yield return null;

        SetUIRootResolution();

        yield return null;
        Debug.Log(m_graphicLevel + " " + m_resolutionX + "|" + m_resolutionY + ", " + uirootWidth + "|" + uirootHeight + ", " + relativeScale);

        if (null != cam)
        {
            cam.enabled = true;
            cam.useOcclusionCulling = false;
        }

#if UNITY_EDITOR
        for (int i = uiRelativeScaleList.Count - 1; i >= 0; i--)
        {
            if (null == uiRelativeScaleList[i])
            {
                uiRelativeScaleList.RemoveAt(i);
            }
            else
            {
                uiRelativeScaleList[i].ChangeScale();
            }
        }
#endif

        if (null != OnOrientationChanged)
        {
            OnOrientationChanged();
        }

        _processingOrientationChanged = false;
        if (0 < _callCountOrientationChanged)
            OrientationChanged();
    }

    public void SetUIRootResolution()
    {
        // ====> UIRoot 에 대해서 세팅 해주기
        GameObject screen2d = GameObject.Find("Screen2D");
        if (null != screen2d)
        {
            SetUIRootProperties(screen2d.GetComponent<UIRoot>());
        }

        //if (Overlay.instance != null)
        //    SetUIRootProperties(Overlay.instance.GetComponent<UIRoot>());

        //if (UITutorial.Instance != null)
        //{
        //    SetUIRootProperties(UITutorial.Instance.transform.root.GetComponent<UIRoot>());
        //}

        //if (UILoadingBase.Instance != null)
        //    UILoadingBase.Instance.OnOrientationChanged();

        //if (null != Overlay.instance)
        //{
        //    bool isOverayActive = Overlay.instance.gameObject.activeSelf;
        //    Overlay.instance.gameObject.SetActive(true);
        //    Overlay.instance.SetUIRootResolution();
        //    LetterBox.instance.Refresh();
        //    Overlay.instance.gameObject.SetActive(isOverayActive);
        //}
        //else
        //{
        //    LetterBox.instance.Refresh();
        //}

        //if (null != ScreenTouchEffect.Instance)
        //{
        //    ScreenTouchEffect.Instance.SetResolutionFactor();
        //}
    }

    public void SetUIRootProperties(UIRoot uiRoot)
    {
        if (isLandscape)
        {
            uiRoot.manualWidth = STANDARD_WIDTH;
            uiRoot.manualHeight = (int)(((float)Screen.height / (float)Screen.width) * (float)uiRoot.manualWidth);
            uiRoot.fitWidth = true;
            uiRoot.fitHeight = false;
            relativeScale = (float)uiRoot.manualHeight / (float)STANDARD_HEIGHT;
            relativeScale = 1f < relativeScale ? 1f : relativeScale;
        }
        else
        {
            uiRoot.manualHeight = STANDARD_WIDTH;
            uiRoot.manualWidth = (int)(((float)Screen.width / (float)Screen.height) * (float)uiRoot.manualHeight);
            uiRoot.fitWidth = false;
            uiRoot.fitHeight = true;
            relativeScale = (float)uiRoot.manualWidth / (float)STANDARD_HEIGHT;
            relativeScale = 1f < relativeScale ? 1f : relativeScale;
        }
        Debug.Log(uiRoot.manualWidth + " " + uiRoot.manualHeight);
        uirootWidth = uiRoot.manualWidth;
        uirootHeight = uiRoot.manualHeight;
        widthRatio = uirootWidth / (float)Screen.width;
        heightRatio = uirootHeight / (float)Screen.height;
    }

    public void SetUIRootPropertiesLandscape(UIRoot uiRoot)
    {
        uiRoot.manualWidth = STANDARD_WIDTH;
        uiRoot.manualHeight = (int)(((float)Screen.height / (float)Screen.width) * (float)uiRoot.manualWidth);
        uiRoot.fitWidth = true;
        uiRoot.fitHeight = false;
        relativeScale = (float)uiRoot.manualHeight / (float)STANDARD_HEIGHT;
        relativeScale = 1f < relativeScale ? 1f : relativeScale;

        uirootWidth = uiRoot.manualWidth;
        uirootHeight = uiRoot.manualHeight;
        widthRatio = uirootWidth / (float)Screen.width;
        heightRatio = uirootHeight / (float)Screen.height;
    }

    /// <summary>
    /// SetGraphicLevel Grade (0 : high, 1 : medium, 2 : low)
    /// </summary>
    /// <param name="level">0 : high, 1 : medium, 2 : low</param>
    public void SetGraphicLevel(int graphicLevel)
    {
        m_graphicLevel = graphicLevel;

        OrientationChanged();
    }

    public Vector3 CalcPosScreenRatio(float x, float y)
    {
        Vector3 pos = Vector3.zero;
        if (isLandscape)
        {
            pos.x = x;

            if (0 == y)
                pos.y = 0;
            else if (0 < y)
                pos.y = (uirootHeight - (STANDARD_HEIGHT - (y * 2))) * 0.5f;
            else
                pos.y = -(uirootHeight - (STANDARD_HEIGHT + (y * 2))) * 0.5f;
        }
        else
        {
            if (0 == x)
                pos.x = 0;
            else if (0 < x)
                pos.x = (uirootWidth - (STANDARD_HEIGHT - (x * 2))) * 0.5f;
            else
                pos.x = -(uirootWidth - (STANDARD_HEIGHT + (x * 2))) * 0.5f;

            pos.y = y;
        }

        return pos;
    }

    public void BackupAutoRotateState(string tag)
    {
        if (m_bSavedBackup)
        {
            Debug.LogError("ERROR BackupAutoRotateState: already saved! " + m_backupTag);
        }

        m_backupIsLockRotation = isLockRotation;
        m_backupAutoRotateState[0] = Screen.autorotateToLandscapeLeft;
        m_backupAutoRotateState[1] = Screen.autorotateToLandscapeRight;
        m_backupAutoRotateState[2] = Screen.autorotateToPortrait;
        m_backupAutoRotateState[3] = Screen.autorotateToPortraitUpsideDown;
        m_backupTag = tag;
        m_bSavedBackup = true;
    }


    public void RestoreAutoRotateState()
    {
        if (m_bSavedBackup)
        {
            Screen.autorotateToLandscapeLeft = m_backupAutoRotateState[0];
            Screen.autorotateToLandscapeRight = m_backupAutoRotateState[1];
            Screen.autorotateToPortrait = m_backupAutoRotateState[2];
            Screen.autorotateToPortraitUpsideDown = m_backupAutoRotateState[3];

            if (m_backupIsLockRotation)
                SetLockRotation(isLandscape);
            else
                SetAutoRotation();

            m_backupTag = string.Empty;
            m_bSavedBackup = false;
        }
        else
        {
            Debug.LogError("ERROR RestoreAutoRotateState: not saved yet!");
        }
    }

    public void ClearBackupAutoRotateState()
    {
        m_bSavedBackup = false;
        m_backupTag = string.Empty;
        m_bSavedBackup = false;
    }



    #region PlayerPrefs

    public void SavePrefLockRotation()
    {
        PlayerPrefs.SetInt(KEY_LOCK_ROTATION, isLockRotation ? (isLandscape ? 1 : 2) : 0);
    }

    public int GetPrefLockRotation()
    {
        return PlayerPrefs.GetInt(KEY_LOCK_ROTATION, 0);
    }

    public void LoadPrefLockRotation()
    {
        isLockRotation = 0 == PlayerPrefs.GetInt(KEY_LOCK_ROTATION, 0) ? false : true;
    }

    public void SavePrefControlHand(int controlHand)
    {
        PlayerPrefs.SetInt(KEY_CONTROL_HAND, controlHand);
    }

    public int GetPrefControlHand()
    {
        return PlayerPrefs.GetInt(KEY_CONTROL_HAND, 1);
    }

    #endregion

    /*
    #region GUI

    private string m_text = string.Empty;
    private GUIStyle m_style;

    void OnGUI()
    {
        if (null == m_style)
        {
            m_style = new GUIStyle();
            m_style.alignment = TextAnchor.UpperRight;
            m_style.normal.textColor = Color.white;
            m_style.fontSize = 30;
            m_style.richText = true;
        }

        string temp = isLandscape + " " + Screen.width + " " + Screen.height + "\n"; // Screen.orientation + " " + m_resolutionX + " " + m_resolutionY;
        temp += m_text;

        DrawOutline(new Rect(0, 0, Screen.width, Screen.height), temp, m_style, Color.red);

        //if (GUI.Button(new Rect(0, 0, 100, 100), isLockRotation ? "Locked" : "Unlocked"))
        //{
        //    if (isLockRotation)
        //    {
        //        SetAutoRotation();
        //    }
        //    else
        //    {
        //        SetLockRotation(true);
        //    }
        //}
    }

    public void DrawOutline(Rect position, string text, GUIStyle style, Color outColor)
    {
        GUIStyle backupStyle = style;
        Color oldColor = style.normal.textColor;
        style.normal.textColor = outColor;
        position.x--;
        GUI.Label(position, text, style);
        position.x += 2;
        GUI.Label(position, text, style);
        position.x--;
        position.y--;
        GUI.Label(position, text, style);
        position.y += 2;
        GUI.Label(position, text, style);
        position.y--;
        style.normal.textColor = oldColor;
        GUI.Label(position, text, style);
        style = backupStyle;
    }

    #endregion
     */
}
