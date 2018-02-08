using UnityEngine;
using System.Collections;

public class ExampleLoadBundle : ExampleLoad
{
    IEnumerator Start()
    {
        yield return ABDownloader.Instance.Initialize();
        text = "LoadComplete Manifest";

        //No.1 Coroutine
        yield return StartCoroutine(LoadBundleFirst("cube_unitylogo", "cube_unitylogo"));

        //No.2 Listener
        //LoadBundleFirstUseListener("cube_unitylogo", "cube_unitylogo");
    }

    IEnumerator LoadBundleFirst(string assetBundleName, string assetName)
    {
        float startTime = Time.realtimeSinceStartup;

        yield return ABDownloader.Instance.LoadBundleAsync(assetBundleName, true);

        GameObject prefab = ABDownloader.Instance.GetLoadedAsset<GameObject>(assetName);
        if (prefab != null)
            GameObject.Instantiate(prefab);

        // Calculate and display the elapsed time.
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        text = "LoadBundleFirst\n";
        text += assetName + " loaded successfully in " + elapsedTime + " seconds";
    }

    private void LoadBundleFirstUseListener(string assetBundleName, string assetName)
    {
        float startTime = Time.realtimeSinceStartup;

        ABDownloader.Instance.LoadBundleAsync(assetBundleName, delegate(LoadedAssetBundle bundle)
        {
            GameObject prefab = ABDownloader.Instance.GetLoadedAsset<GameObject>(assetName);
            if (prefab != null)
                GameObject.Instantiate(prefab);

            // Calculate and display the elapsed time.
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            text = "LoadBundleFirstUseListener\n";
            text += assetName + " loaded successfully in " + elapsedTime + " seconds";
        }, false);
    }
}
