using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class LoadedAssetBundle
{
    public AssetBundle assetBundle { get; private set; }
    public int referencedCount;

    public LoadedAssetBundle(AssetBundle assetBundle)
    {
        this.assetBundle = assetBundle;
        referencedCount = 1;
    }
}

public class ABDownloader : MonoBehaviour
{
    static private ABDownloader s_instance;
    static public ABDownloader Instance
    {
        get
        {
            if (null == s_instance)
            {
                //Debug.LogError("Failed To Find ABDownloader Instance");
            }
            return s_instance;
        }
    }

    public string rootUrl { get; set; }  //file server url

    public AssetBundleManifest manifest { get; private set; }
    private Dictionary<string, string> dicABName; //key is AssetName, value is AssetBundleName List
    private Dictionary<string, List<string>> dicABDuplicated;   //duplicated name Asset dictionary

    public Dictionary<string, LoadedAssetBundle> getDicLoadedAssetBundles { get { return dicLoadedAssetBundles; } }
    private Dictionary<string, LoadedAssetBundle> dicLoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

    private Dictionary<string, WWW> dicDownloadingWWWs = new Dictionary<string, WWW>();

    private Dictionary<string, string> dicDownloadingErrors = new Dictionary<string, string>();

    private List<AssetBundleLoadOperation> dicInProgressOperations = new List<AssetBundleLoadOperation>();

    public Dictionary<string, string[]> getDicDependencies { get { return dicDependencies; } }
    private Dictionary<string, string[]> dicDependencies = new Dictionary<string, string[]>();

    private List<string> allVariantList = new List<string>();
    private List<string> activatedVariantList = new List<string>();

    private List<string> unloadableList = new List<string>();

    public delegate void LoadAssetCompleteListener<T>(T obj) where T : UnityEngine.Object;
    public delegate void LoadBundleCompleteListener(LoadedAssetBundle bundle);

    #region MonoBehaviour

    void Awake()
    {
        s_instance = FindObjectOfType(typeof(ABDownloader)) as ABDownloader;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Collect all the finished WWWs.
        List<string> keysToRemove = new List<string>();
        Dictionary<string, WWW>.Enumerator enumerator = dicDownloadingWWWs.GetEnumerator();
        while (enumerator.MoveNext())
        {
            KeyValuePair<string, WWW> keyValue = enumerator.Current;
            WWW download = keyValue.Value;

            // If downloading fails.
            if (null != download.error)
            {
                dicDownloadingErrors.Add(keyValue.Key, string.Format("Failed downloading bundle {0} from {1}: {2}", keyValue.Key, download.url, download.error));
                keysToRemove.Add(keyValue.Key);
                continue;
            }

            // If downloading succeeds.
            if (download.isDone)
            {
                AssetBundle bundle = download.assetBundle;
                if (null == bundle)
                {
                    dicDownloadingErrors.Add(keyValue.Key, string.Format("{0} is not a valid asset bundle.", keyValue.Key));
                    keysToRemove.Add(keyValue.Key);
                    continue;
                }

                //Debug.Log(string.Format("[Bundle Downloading] {0} is done at frame {1}", keyValue.Key, Time.frameCount));
                dicLoadedAssetBundles.Add(keyValue.Key, new LoadedAssetBundle(download.assetBundle));
                keysToRemove.Add(keyValue.Key);
            }
        }

        // Remove the finished WWWs.
        for (int i = 0; i < keysToRemove.Count; i++)
        {
            string key = keysToRemove[i];
            WWW download = dicDownloadingWWWs[key];
            dicDownloadingWWWs.Remove(key);
            download.Dispose();
        }

        // Update all in progress operations
        for (int i = 0; i < dicInProgressOperations.Count;)
        {
            if (!dicInProgressOperations[i].Update())
            {
                dicInProgressOperations.RemoveAt(i);
            }
            else
                i++;
        }
    }

    #endregion

    #region Manifest

    public Coroutine Initialize()
    {
        return StartCoroutine(LoadManifest());
    }

    private IEnumerator LoadManifest()
    {
        string platformName = ABUtil.GetPlatformName();
        string fileurl = Path.Combine(rootUrl, platformName);
        Debug.Log(fileurl);
        WWW www = new WWW(fileurl);
        yield return www;

        if (www.isDone && null == www.error)
        {
            manifest = (AssetBundleManifest)www.assetBundle.LoadAsset("AssetBundleManifest", typeof(AssetBundleManifest));
            allVariantList = new List<string>(manifest.GetAllAssetBundlesWithVariant());
        }
        else
        {
            Debug.LogWarning("Fail to load AssetBundleManifest");
            yield break;
        }

        //string[] names = manifest.GetAllAssetBundles();
        //foreach (string name in names)
        //{
        //    Debug.Log(string.Format("Name : {0}, Hash : {1}", name, manifest.GetAssetBundleHash(name));

        //    string[] dependencies = manifest.GetAllDependencies(name);
        //    foreach (string dpdc in dependencies)
        //    {
        //        Debug.Log(string.Format("{0} : {1}", name, dpdc));
        //        Debug.Log(name + "   " + dpdc);
        //    }

        //    dependencies = manifest.GetDirectDependencies(name);
        //    foreach (string dpdc in dependencies)
        //    {
        //        Debug.Log(string.Format("dependencies {0} : {1}", name, dpdc));
        //    }
        //}

        //ABData.json
        #region Json Download Direct
        //WWW wwwJson = new WWW(Path.Combine(rootUrl, VALUE_JSONNAME.ToLower()));
        //yield return wwwJson;

        //if (wwwJson.isDone && null == wwwJson.error)
        //{
        //    TextAsset t = (TextAsset)wwwJson.assetBundle.LoadAsset(VALUE_JSONNAME, typeof(TextAsset));

        //    ABData[] dataList = JsonMapper.ToObject<ABData[]>(t.text);
        //    dicABData = new Dictionary<string, string>();
        //    ABData data;
        //    for (int i = 0; i < dataList.Length; i++)
        //    {
        //        data = dataList[i];
        //        Debug.Log(string.Format("{0} : {1}", data.name, data.assets.Length));
        //        for (int j = 0; j < data.assets.Length; j++)
        //        {
        //            Debug.Log(string.Format("{0} : {1}", data.name, data.assets[j]));
        //            dicABData.Add(data.assets[j], data.name);
        //        }
        //    }
        //}
        #endregion

        #region Json LoadFromCacheOrDownload
        AssetBundleLoadAssetOperation operation = LoadAssetAsync<TextAsset>(ABUtil.KEY_JSONNAME.ToLower(), ABUtil.KEY_JSONNAME, true);
        yield return operation;

        TextAsset t = operation.GetAsset<TextAsset>();

        List<ABData> dataList = JsonMapper.ToObject<List<ABData>>(t.text);
        dicABName = new Dictionary<string, string>();
        dicABDuplicated = new Dictionary<string, List<string>>();
        ABData data;
        StringBuilder sb;
        for (int i = 0; i < dataList.Count; i++)
        {
            data = dataList[i];
            //Debug.Log(string.Format("{0} : {1}", data.name, data.assets.Length));
            for (int j = 0; j < data.assets.Length; j++)
            {
                sb = new StringBuilder();
                sb.Append(data.assets[j]);
                if (!string.IsNullOrEmpty(data.variant))
                    sb.Append(".").Append(data.variant);

                string assetName = sb.ToString();

                if (dicABDuplicated.ContainsKey(assetName))
                {
                    dicABDuplicated[assetName].Add(data.name);
                }
                else
                {
                    if (dicABName.ContainsKey(assetName))
                    {
                        List<string> bundleNameList = new List<string>();
                        bundleNameList.Add(dicABName[assetName]);
                        bundleNameList.Add(data.name);
                        dicABDuplicated.Add(assetName, bundleNameList);

                        dicABName.Remove(assetName);
                    }
                    else
                    {
                        dicABName.Add(assetName, data.name);
                    }
                }
            }
        }

#if UNITY_EDITOR
        //Log Duplicated
        foreach (string key in dicABDuplicated.Keys)
        {
            string str = "D : " + key;
            foreach (string item in dicABDuplicated[key])
            {
                str += "   " + item;
            }
            Debug.Log(str);
        }
#endif
        #endregion
    }

    #endregion

    #region Asynchronous Load AssetBundle

    // Load AssetBundle.
    public AssetBundleLoadBundleOperation LoadBundleAsync(string assetBundleName, bool unloadable = false)
    {
        assetBundleName = RemapAssetBundleName(assetBundleName);
#if UNITY_EDITOR
        Debug.Log(string.Format("[LoadBundleAsync] Loading [{0}]", assetBundleName));
#endif
        if (unloadable && null != assetBundleName && !unloadableList.Contains(assetBundleName))
        {
            unloadableList.Add(assetBundleName);
        }

        LoadAssetBundle(assetBundleName);

        AssetBundleLoadBundleOperation operation = new AssetBundleLoadBundleOperation(assetBundleName);
        dicInProgressOperations.Add(operation);
        StartCoroutine(operation);

        return operation;
    }

    public void LoadBundleAsync(string assetBundleName, LoadBundleCompleteListener listener, bool unloadable = false)
    {
        StartCoroutine(CrtLoadBundleAsync(assetBundleName, listener, unloadable));
    }

    IEnumerator CrtLoadBundleAsync(string assetBundleName, LoadBundleCompleteListener listener, bool unloadable)
    {
        yield return LoadBundleAsync(assetBundleName, unloadable);

        if (null != listener)
        {
            string error;
            listener(GetLoadedAssetBundle(assetBundleName, out error));
        }
    }

    #endregion

    #region Asynchronous Load Asset direct

    // Load asset from the given assetBundle.
    public AssetBundleLoadAssetOperation LoadAssetAsync<T>(string assetName, bool unloadable = false) where T : UnityEngine.Object
    {
        string assetBundleName = FindAssetBundleName(assetName);

        if (string.IsNullOrEmpty(assetBundleName))
        {
            Debug.LogWarning(string.Format("assetBundleName is NULL or EMPTY finded by {0}", assetName));
            return null;
        }
        else if (unloadable && !unloadableList.Contains(assetBundleName))
        {
            unloadableList.Add(assetBundleName);
        }
#if UNITY_EDITOR
        Debug.Log(string.Format("[LoadAssetAsync] Loading [{0}] from [{1}]", assetName, assetBundleName));
#endif

        LoadAssetBundle(assetBundleName);

        AssetBundleLoadAssetOperation operation = new AssetBundleLoadAssetOperation(assetBundleName, assetName, typeof(T));
        dicInProgressOperations.Add(operation);
        StartCoroutine(operation);

        return operation;
    }

    public void LoadAssetAsync<T>(string assetName, LoadAssetCompleteListener<T> listener, bool unloadable = false) where T : UnityEngine.Object
    {
        StartCoroutine(CrtLoadAssetAsync(assetName, listener, unloadable));
    }

    IEnumerator CrtLoadAssetAsync<T>(string assetName, LoadAssetCompleteListener<T> listener, bool unloadable) where T : UnityEngine.Object
    {
        AssetBundleLoadAssetOperation operation = LoadAssetAsync<T>(assetName, unloadable);
        yield return operation;

        if (null != listener)
        {
            listener(operation.GetAsset<T>());
        }
    }

    public AssetBundleLoadAssetOperation LoadAssetAsync<T>(string assetBundleName, string assetName, bool unloadable = false) where T : UnityEngine.Object
    {
        assetBundleName = RemapAssetBundleName(assetBundleName);
#if UNITY_EDITOR
        Debug.Log(string.Format("[LoadAssetAsync] Loading [{0}] from [{1}]", assetName, assetBundleName));
#endif
        if (unloadable && null != assetBundleName && !unloadableList.Contains(assetBundleName))
        {
            unloadableList.Add(assetBundleName);
        }

        LoadAssetBundle(assetBundleName);

        AssetBundleLoadAssetOperation operation = new AssetBundleLoadAssetOperation(assetBundleName, assetName, typeof(T));
        dicInProgressOperations.Add(operation);
        StartCoroutine(operation);

        return operation;
    }

    public void LoadAssetAsync<T>(string assetBundleName, string assetName, LoadAssetCompleteListener<T> listener, bool unloadable = false) where T : UnityEngine.Object
    {
        StartCoroutine(CrtLoadAssetAsync(assetBundleName, assetName, listener, unloadable));
    }

    IEnumerator CrtLoadAssetAsync<T>(string assetBundleName, string assetName, LoadAssetCompleteListener<T> listener, bool unloadable) where T : UnityEngine.Object
    {
        AssetBundleLoadAssetOperation operation = LoadAssetAsync<T>(assetBundleName, assetName, unloadable);
        yield return operation;

        if (null != listener)
        {
            listener(operation.GetAsset<T>());
        }
    }

    #endregion

    #region Asynchronous Load Level(Scene)

    // Load level from the given assetBundle.
    public AssetBundleLoadOperation LoadLevelAsync(string levelName, LoadSceneMode loadSceneMode, bool unloadable = false)
    {
        string assetBundleName;
        dicABName.TryGetValue(levelName, out assetBundleName);
#if UNITY_EDITOR
        Debug.Log(string.Format("[LoadLevelAsync] Loading [{0}] from [{1}]", levelName, assetBundleName));
#endif
        if (unloadable && null != assetBundleName && !unloadableList.Contains(assetBundleName))
        {
            unloadableList.Add(assetBundleName);
        }

        LoadAssetBundle(assetBundleName);

        AssetBundleLoadOperation operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, loadSceneMode);
        dicInProgressOperations.Add(operation);
        StartCoroutine(operation);

        return operation;
    }

    public AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, LoadSceneMode loadSceneMode, bool unloadable = false)
    {
        assetBundleName = RemapAssetBundleName(assetBundleName);
#if UNITY_EDITOR
        Debug.Log(string.Format("[LoadLevelAsync] Loading [{0}] from [{1}]", levelName, assetBundleName));
#endif
        if (unloadable && null != assetBundleName && !unloadableList.Contains(assetBundleName))
        {
            unloadableList.Add(assetBundleName);
        }

        LoadAssetBundle(assetBundleName);

        AssetBundleLoadOperation operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, loadSceneMode);
        dicInProgressOperations.Add(operation);
        StartCoroutine(operation);

        return operation;
    }

    #endregion

    #region get loaded AssetBundle or Asset

    public T GetLoadedAsset<T>(string assetName) where T : UnityEngine.Object
    {
        string assetBundleName = FindAssetBundleName(assetName);

        if (string.IsNullOrEmpty(assetBundleName))
        {
            Debug.LogWarning(string.Format("assetBundleName is NULL or EMPTY finded by {0}", assetName));
            return null;
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log(string.Format("[GetLoadedAsset] Loading [{0}] from [{1}]", assetName, assetBundleName));
#endif
        }

        string outString;
        LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out outString);

        if (null != bundle)
            return bundle.assetBundle.LoadAsset<T>(assetName);
        else
            return null;
    }

    public LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
    {
        assetBundleName = RemapAssetBundleName(assetBundleName);

        if (dicDownloadingErrors.TryGetValue(assetBundleName, out error))
            return null;

        LoadedAssetBundle bundle = null;
        dicLoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
        if (bundle == null)
            return null;

        // No dependencies are recorded, only the bundle itself is required.
        string[] dependencies = null;
        if (!dicDependencies.TryGetValue(assetBundleName, out dependencies))
            return bundle;

        // Make sure all dependencies are loaded
        for (int i = 0; i < dependencies.Length; i++)
        {
            if (dicDownloadingErrors.TryGetValue(assetBundleName, out error))
                return bundle;

            // Wait all the dependent assetBundles being loaded.
            LoadedAssetBundle dependentBundle;
            dicLoadedAssetBundles.TryGetValue(dependencies[i], out dependentBundle);
            if (dependentBundle == null)
                return null;
        }

        return bundle;
    }

    #endregion

    #region variants

    public List<string> GetActivatedVariants()
    {
        return activatedVariantList;
    }

    public void ActiveVariant(string variant)
    {
        activatedVariantList.Add(variant);
    }

    public void DeactiveVariant(string variant)
    {
        activatedVariantList.Remove(variant);
    }

    public void DeactiveAllVariant(string variant)
    {
        activatedVariantList.Clear();
    }

    #endregion

    #region internal use

    private string FindAssetBundleName(string assetName)
    {
        if (0 < activatedVariantList.Count)
        {
            for (int i = 0; i < activatedVariantList.Count; i++)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(assetName).Append(".").Append(activatedVariantList[i]);

                if (dicABName.ContainsKey(sb.ToString()))
                {
                    return dicABName[sb.ToString()];
                }
                else if (dicABDuplicated.ContainsKey(sb.ToString()))
                {
                    Debug.Log(assetName + " is duplicated AssetName. Use AssetBundleName and AssetNAme both to load.");
                    return dicABDuplicated[sb.ToString()][0];
                }
            }
        }
        else
        {
            if (dicABName.ContainsKey(assetName))
            {
                return dicABName[assetName];
            }
            else if (dicABDuplicated.ContainsKey(assetName))
            {
                Debug.Log(assetName + " is duplicated AssetName. Use AssetBundleName and AssetNAme both to load.");
                return dicABDuplicated[assetName][0];
            }
        }

        return string.Empty;
    }

    private string RemapAssetBundleName(string assetBundleName)
    {
        if (0 < activatedVariantList.Count)
        {
            for (int i = 0; i < activatedVariantList.Count; i++)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(assetBundleName).Append(".").Append(activatedVariantList[i]);

                if (allVariantList.Contains(sb.ToString()))
                {
                    assetBundleName = sb.ToString();
                }
            }
        }

        return assetBundleName;
    }

    // Load AssetBundle and its dependencies.
    private void LoadAssetBundle(string assetBundleName)
    {
        //Debug.Log("Loading Asset Bundle : " + assetBundleName);

        // Check if the assetBundle has already been processed.
        bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName);

        // Load dependencies.
        if (!isAlreadyProcessed)
            LoadDependencies(assetBundleName);
    }

    // Where we actuall call WWW to download the assetBundle.
    private bool LoadAssetBundleInternal(string assetBundleName)
    {
        // Already loaded.
        LoadedAssetBundle bundle = null;
        dicLoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
        if (bundle != null)
        {
            bundle.referencedCount++;
            return true;
        }

        // @TODO: Do we need to consider the referenced count of WWWs?
        // In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
        // But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.
        if (dicDownloadingWWWs.ContainsKey(assetBundleName))
            return true;

        WWW download = null;
        string url = Path.Combine(rootUrl, assetBundleName);

        // For manifest assetbundle, always download it as we don't have hash for it.
        Hash128 bundleHash = manifest.GetAssetBundleHash(assetBundleName);
        download = WWW.LoadFromCacheOrDownload(url, bundleHash);
        Caching.MarkAsUsed(download.url, bundleHash);

        dicDownloadingWWWs.Add(assetBundleName, download);

        return false;
    }

    // Where we get all the dependencies and load them all.
    private void LoadDependencies(string assetBundleName)
    {
        if (manifest == null)
        {
            Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
            return;
        }

        // Get dependecies from the AssetBundleManifest object..
        string[] dependencies = manifest.GetAllDependencies(assetBundleName);
        if (dependencies.Length == 0)
            return;

        // Record and load all dependencies.
        dicDependencies.Add(assetBundleName, dependencies);
        for (int i = 0; i < dependencies.Length; i++)
            LoadAssetBundleInternal(dependencies[i]);
    }

    #endregion

    #region Unload

    public void UnloadAllAssetBundle()
    {
        //Debug.Log("[UnloadAllAssetBundle]");

        List<string> keyList = new List<string>(dicLoadedAssetBundles.Keys);
        for (int i = 0; i < keyList.Count; i++)
        {
            string key = keyList[i];
            if (!unloadableList.Contains(key))
            {
                dicLoadedAssetBundles[key].assetBundle.Unload(false);
                dicLoadedAssetBundles.Remove(key);

                if (dicDependencies.ContainsKey(key))
                    dicDependencies.Remove(key);
            }
        }
    }

    /// <summary>
    /// Find and Unload assetbundle and its dependencies. Ignore unloadble list
    /// </summary>
    /// <param name="assetName"></param>
    public void FindUnloadAssetBundle(string assetName)
    {
        string assetBundleName = FindAssetBundleName(assetName);

        if (!string.IsNullOrEmpty(assetBundleName))
            UnloadAssetBundle(assetBundleName);
    }

    /// <summary>
    /// Unload assetbundle and its dependencies.
    /// </summary>
    /// <param name="assetBundleName"></param>
    public void UnloadAssetBundle(string assetBundleName)
    {
        assetBundleName = RemapAssetBundleName(assetBundleName);
        //Debug.Log(string.Format("{0} assetbundle(s) in memory before unloading [{1}]", dicLoadedAssetBundles.Count, assetBundleName));

        UnloadAssetBundleInternal(assetBundleName);
        UnloadDependencies(assetBundleName);

        //Debug.Log(string.Format("{0} assetbundle(s) in memory after unloading [{1}]", dicLoadedAssetBundles.Count, assetBundleName));
    }

    private void UnloadDependencies(string assetBundleName)
    {
        string[] dependencies = null;
        if (!dicDependencies.TryGetValue(assetBundleName, out dependencies))
            return;

        // Loop dependencies.
        for (int i = 0; i < dependencies.Length; i++)
        {
            UnloadAssetBundleInternal(dependencies[i]);
        }

        dicDependencies.Remove(assetBundleName);
    }

    private void UnloadAssetBundleInternal(string assetBundleName)
    {
        string error;
        LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out error);
        if (bundle == null)
            return;

        if (--bundle.referencedCount == 0)
        {
            bundle.assetBundle.Unload(false);
            dicLoadedAssetBundles.Remove(assetBundleName);
            //Debug.Log(string.Format("{0} has been unloaded successfully", assetBundleName));
        }
    }

    #endregion
}