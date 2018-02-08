using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public abstract class AssetBundleLoadOperation : IEnumerator
{
    public object Current
    {
        get
        {
            return null;
        }
    }
    public bool MoveNext()
    {
        return !IsDone();
    }

    public void Reset()
    {
    }

    abstract public bool Update();

    abstract public bool IsDone();
}

public class AssetBundleLoadBundleOperation : AssetBundleLoadOperation
{
    protected string m_assetBundleName;
    protected string m_downloadingError;
    protected bool m_isDone;

    public AssetBundleLoadBundleOperation(string assetbundleName)
    {
        m_assetBundleName = assetbundleName;
        m_isDone = false;
    }

    public override bool Update()
    {
        if (m_isDone)
            return false;

        LoadedAssetBundle bundle = ABDownloader.Instance.GetLoadedAssetBundle(m_assetBundleName, out m_downloadingError);
        if (null != bundle)
        {
            m_isDone = true;
            return false;
        }
        else
            return true;
    }

    public override bool IsDone()
    {
        // Return if meeting downloading error.
        // m_DownloadingError might come from the dependency downloading.
        if (!m_isDone && null != m_downloadingError)
        {
            Debug.LogError(m_downloadingError);
            return true;
        }

        return m_isDone;
    }
}

public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
{
    protected string m_assetBundleName;
    protected string m_levelName;
    protected LoadSceneMode m_loadSceneMode;
    protected string m_downloadingError;
    protected AsyncOperation m_request;

    public AssetBundleLoadLevelOperation(string assetbundleName, string levelName, LoadSceneMode loadSceneMode)
    {
        m_assetBundleName = assetbundleName;
        m_levelName = levelName;
        m_loadSceneMode = loadSceneMode;
    }

    public override bool Update()
    {
        if (null != m_request)
            return false;

        LoadedAssetBundle bundle = ABDownloader.Instance.GetLoadedAssetBundle(m_assetBundleName, out m_downloadingError);
        if (null != bundle)
        {
            m_request = SceneManager.LoadSceneAsync(m_levelName, m_loadSceneMode);
            return false;
        }
        else
            return true;
    }

    public override bool IsDone()
    {
        // Return if meeting downloading error.
        // m_DownloadingError might come from the dependency downloading.
        if (null == m_request && null != m_downloadingError)
        {
            Debug.LogError(m_downloadingError);
            return true;
        }

        return null != m_request && m_request.isDone;
    }
}

public class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
{
    protected string m_assetBundleName;
    protected string m_assetName;
    protected string m_downloadingError;
    protected System.Type m_type;
    protected AssetBundleRequest m_request;

    public AssetBundleLoadAssetOperation(string bundleName, string assetName, System.Type type)
    {
        m_assetBundleName = bundleName;
        m_assetName = assetName;
        m_type = type;
    }

    public T GetAsset<T>() where T : UnityEngine.Object
    {
        if (null != m_request && m_request.isDone)
        {
            T asset = m_request.asset as T;

            #region shader setting
            //AssetBundle은 custom shader를 포함하지 않으므로 불러올 시점에 shader를 재설정해준다.
            if (typeof(GameObject) == typeof(T))
            {
                GameObject go = asset as GameObject;
                RefreshShader(go.GetComponents<Renderer>());
                RefreshShader(go.GetComponentsInChildren<Renderer>(true));
            }
            else if (typeof(Material) == typeof(T))
            {
                RefreshShader(asset as Material);
            }
            #endregion

            if (null == asset)
                Debug.LogError(string.Format("[{0}] asset is NULL from [{1}]. Type is [{2}]. T is [{3}]", m_assetName, m_assetBundleName, m_type.ToString(), typeof(T).ToString()));

            return asset;
        }
        else
        {
            Debug.LogError(string.Format("[{0}] asset is NULL from [{1}]", m_assetName, m_assetBundleName));
            return null;
        }
    }

    // Returns true if more Update calls are required.
    public override bool Update()
    {
        if (null != m_request)
            return false;

        LoadedAssetBundle bundle = ABDownloader.Instance.GetLoadedAssetBundle(m_assetBundleName, out m_downloadingError);
        if (null != bundle && null != bundle.assetBundle)
        {
            ///@TODO: When asset bundle download fails this throws an exception...
            m_request = bundle.assetBundle.LoadAssetAsync(m_assetName, m_type);
            return false;
        }
        else
        {
            return true;
        }
    }

    public override bool IsDone()
    {
        // Return if meeting downloading error.
        // m_DownloadingError might come from the dependency downloading.
        if (null == m_request && null != m_downloadingError)
        {
            Debug.LogError(m_downloadingError);
            return true;
        }

        return null != m_request && m_request.isDone;
    }

    static protected void RefreshShader(Renderer[] renderers)
    {
        if (null == renderers) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;
            for (int j = 0; j < materials.Length; j++)
            {
                RefreshShader(materials[j]);
            }
        }
    }

    static protected void RefreshShader(Material material)
    {
        Shader shader = Shader.Find(material.shader.name);
        if (shader != null)
        {
            material.shader = shader;
        }
        else
        {
            Debug.LogWarning(string.Format("unable to refresh shader: [{0}] in material [{1}]", material.shader.name, material.name));
        }
    }
}
