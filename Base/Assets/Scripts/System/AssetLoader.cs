using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssetLoader : MonoBehaviour
{
    private static AssetLoader s_instance;
    public static AssetLoader Instance
    {
        get
        {
            if (null == s_instance)
            {
                Debug.LogWarning("Failed To Find AssetLoader Instance");
            }
            return s_instance;
        }
    }

    void Awake()
    {
        s_instance = FindObjectOfType(typeof(AssetLoader)) as AssetLoader;
    }

    public delegate void AssetLoadedListener<T>(T obj) where T : UnityEngine.Object;
    private static IEnumerator Load<T>(bool useBundle, string assetPath, AssetLoadedListener<T> assetLoadedListener = null) where T : UnityEngine.Object
    {
        if (useBundle)
        {
            string[] splitPath = assetPath.Split('/');
            AssetBundleLoadAssetOperation operation = ABDownloader.Instance.LoadAssetAsync<T>(splitPath[splitPath.Length - 1]);
            yield return operation;

            if (null != assetLoadedListener)
                assetLoadedListener(operation.GetAsset<T>());
        }
        else
        {
            var resourceRequest = Resources.LoadAsync(assetPath);
            yield return resourceRequest;

            if (null != assetLoadedListener)
                assetLoadedListener(resourceRequest.asset as T);
        }
    }

    public static IEnumerator LoadTable(string assetPath, AssetLoadedListener<TextAsset> assetLoadedListener)
    {
        yield return Load<TextAsset>(GameSystem.Instance.UseBundleTable, assetPath, assetLoadedListener);
    }
    public void LoadTableAsync(string assetPath, AssetLoadedListener<TextAsset> assetLoadedListener)
    {
        StartCoroutine(LoadTable(assetPath, assetLoadedListener));
    }

    public delegate void LoadUICompleteListener(GameObject go);
    public static IEnumerator LoadUI(string assetPath, Transform parent, LoadUICompleteListener loadComplete)
    {
        AssetLoadedListener<GameObject> listener = (GameObject go) =>
        {
            go = Instantiate(go, parent);
            if (null != loadComplete)
                loadComplete(go);
        };
        yield return Load<GameObject>(GameSystem.Instance.UseBundleUI, assetPath, listener);
    }
    public void LoadUIAsync(string assetPath, Transform parent, LoadUICompleteListener loadComplete)
    {
        StartCoroutine(LoadUI(assetPath, parent, loadComplete));
    }

    public delegate void LoadListCompleteListener(Dictionary<string, Object> assetDic);
    public static IEnumerator LoadList(bool useBundle, List<string> assetPathList, LoadListCompleteListener loadComplete)
    {
        Dictionary<string, Object> assetDic = new Dictionary<string, Object>();
        if (useBundle)
        {
            for (int i = 0; i < assetPathList.Count; i++)
            {
                string[] splitPath = assetPathList[i].Split('/');
                AssetBundleLoadAssetOperation operation = ABDownloader.Instance.LoadAssetAsync<Object>(splitPath[splitPath.Length - 1]);
                yield return operation;

                assetDic.Add(assetPathList[i], operation.GetAsset<Object>());
            }
        }
        else
        {
            for (int i = 0; i < assetPathList.Count; i++)
            {
                var resourceRequest = Resources.LoadAsync(assetPathList[i]);
                yield return resourceRequest;

                assetDic.Add(assetPathList[i], resourceRequest.asset);
            }
        }

        if (null != loadComplete)
            loadComplete(assetDic);
    }
    public void LoadListAsync(bool useBundle, List<string> assetPathList, LoadListCompleteListener loadComplete)
    {
        StartCoroutine(LoadList(useBundle, assetPathList, loadComplete));
    }

    //public static GameObject Instantiate(string _resPath, Transform _parent = null)
    //{
    //    Object res = Resources.Load(_resPath);
    //    if (null == res)
    //    {
    //        Debug.LogError("Erorr Path: " + _resPath);
    //        return null;
    //    }

    //    GameObject obj = Instantiate(res, _parent);

    //    return obj;
    //}
    public static GameObject Instantiate(Object _res, Transform _parent)
    {
        if (null == _res) return null;

        GameObject obj = GameObject.Instantiate(_res) as GameObject;

        Vector3 localPos = obj.transform.localPosition;
        Quaternion localRotation = obj.transform.localRotation;
        Vector3 localScale = obj.transform.localScale;

        obj.transform.parent = _parent;

        obj.transform.localPosition = localPos;
        obj.transform.localRotation = localRotation;
        obj.transform.localScale = localScale;

        obj.name = _res.name;

        return obj;
    }

    // GetComponent
    //public static T Instantiate<T>(string _resPath, Transform _parent) where T : Component
    //{
    //    return Instantiate(_resPath, _parent).GetComponent<T>();
    //}
    static public T Instantiate<T>(Object _res, Transform _parent) where T : Component
    {
        return Instantiate(_res, _parent).GetComponent<T>();
    }

    static public GameObject FindRootInTag(Transform tfChild)
    {
        if (tfChild == null) return null;

        Transform tfParent = tfChild.transform.parent;
        if (tfParent != null && tfChild.gameObject.layer == tfParent.gameObject.layer)
        {
            return FindRootInTag(tfParent);
        }

        return tfChild.gameObject;
    }
}
