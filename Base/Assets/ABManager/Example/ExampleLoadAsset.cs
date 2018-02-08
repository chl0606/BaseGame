using UnityEngine;
using System.Collections;

public class ExampleLoadAsset : ExampleLoad
{
    IEnumerator Start()
    {
        yield return ABDownloader.Instance.Initialize();
        text = "LoadComplete Manifest";

        //No.1 Coroutine
        yield return StartCoroutine(LoadAsset("cube_unitylogo", "cube_unitylogo"));

        //No.2 Listener
        //LoadAssetUseListener("cube_unitylogo", "cube_unitylogo");
    }

    IEnumerator LoadAsset(string assetBundleName, string assetName)
    {
        float startTime = Time.realtimeSinceStartup;

        //No.1-1 use AssetBundleName and AssetName
        AssetBundleLoadAssetOperation operation = ABDownloader.Instance.LoadAssetAsync<GameObject>(assetBundleName, assetName);
        while (!operation.IsDone())
        {
            yield return null;
        }

        //No.1-2 use AssetName
        //AssetBundleLoadAssetOperation operation = ABDownloader.Instance.LoadAssetAsync<GameObject>(assetName);
        //yield return operation;

        GameObject prefab = operation.GetAsset<GameObject>();
        if (prefab != null)
            GameObject.Instantiate(prefab);

        // Calculate and display the elapsed time.
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        text = "LoadAsset\n";
        text += assetName + " loaded successfully in " + elapsedTime + " seconds";
    }

    private void LoadAssetUseListener(string assetBundleName, string assetName)
    {
        float startTime = Time.realtimeSinceStartup;

        //No.2-1 use AssetBundleName and AssetName
        //ABDownloader.Instance.LoadAssetAsync<GameObject>(assetBundleName, assetName, delegate (GameObject obj)
        //{
        //    GameObject.Instantiate(obj);
        //}, false);

        //No.2-2 use AssetName
        ABDownloader.Instance.LoadAssetAsync<GameObject>(assetName, delegate (GameObject obj)
        {
            GameObject.Instantiate(obj);
        }, false);

        // Calculate and display the elapsed time.
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        text = "LoadAssetUserListener\n";
        text += assetName + " loaded successfully in " + elapsedTime + " seconds";
    }
}
