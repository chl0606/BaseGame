using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExampleLoadScene : ExampleLoad
{
    IEnumerator Start()
    {
        yield return ABDownloader.Instance.Initialize();
        text = "LoadComplete Manifest";

        yield return StartCoroutine(LoadScene("figurescene", "FigureScene", LoadSceneMode.Single));
    }

    IEnumerator LoadScene(string assetBundleName, string levelName, LoadSceneMode loadSceneMode)
    {
        float startTime = Time.realtimeSinceStartup;

        //No.1 Use AssetBundleName and LevelName
        yield return ABDownloader.Instance.LoadLevelAsync(assetBundleName, levelName, loadSceneMode);

        //No.2 Use only LevelName
        yield return ABDownloader.Instance.LoadLevelAsync(levelName, loadSceneMode);

        // Calculate and display the elapsed time.
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        text = "LoadScene\n";
        text += "Finished loading " + levelName + " in " + elapsedTime + " seconds";
    }
}
