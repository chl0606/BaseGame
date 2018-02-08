using UnityEngine;
using System.Collections;

public class ExampleLoadVariant : ExampleLoad
{
    bool isLoaded = false;

    IEnumerator Start()
    {
        yield return ABDownloader.Instance.Initialize();
        text = "LoadComplete Manifest";
    }

    protected override void OnGUI()
    {
        base.OnGUI();

        if (!isLoaded)
        {
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            if (GUILayout.Button("Load Cube Variant SD"))
            {
                ABDownloader.Instance.ActiveVariant("sd");
                isLoaded = true;
                LoadAssetUseListener("cube_variant");
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Load Cube Variant HD"))
            {
                ABDownloader.Instance.ActiveVariant("hd");
                isLoaded = true;
                LoadAssetUseListener("cube_variant");
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    private void LoadAssetUseListener(string assetName)
    {
        float startTime = Time.realtimeSinceStartup;

        //No.2-2 use AssetName
        ABDownloader.Instance.LoadAssetAsync<GameObject>(assetName, delegate (GameObject obj)
        {
            GameObject.Instantiate(obj);
        }, false);

        // Calculate and display the elapsed time.
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        text = "LoadAssetUserListener\n";
        text += assetName + " [variant is " + ABDownloader.Instance.GetActivatedVariants()[0] + "] loaded successfully in " + elapsedTime + " seconds";
    }
}
