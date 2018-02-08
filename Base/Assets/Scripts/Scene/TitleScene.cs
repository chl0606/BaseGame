using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TitleScene : BaseScene
{
    private GameObject _goTitleWindow;

    public override IEnumerator Initialize()
    {
        Debug.Log(string.Format("[{0}] Initialize Start", this.GetType().Name));
        yield return null;

        string prefabPath = "UI/Title/TitleWindow";

        /*
        yield return AssetLoader.Load<GameObject>(GameSystem.Instance.UseBundleUI, prefabPath);
        GameObject prefab = ABDownloader.Instance.GetLoadedAsset<GameObject>("TitleWindow");
        if (null != prefab)
            AssetLoader.Instantiate(prefab);
        */

        //yield return AssetLoader.LoadUI(prefabPath, GameSystem.Instance.tfPanelMain, LoadCompleteListener);
        //AssetLoader.Instance.LoadUIAsync(prefabPath, GameSystem.Instance.tfPanelMain, LoadCompleteListener);

        List<string> pathList = new List<string>();
        pathList.Add(prefabPath);
        yield return AssetLoader.LoadList(GameSystem.Instance.UseBundleUI, pathList, LoadAssetListCompleteListener);

        Debug.Log(string.Format("[{0}] Initialize End", this.GetType().Name));
    }

    public override IEnumerator Terminate()
	{
        GameObject.Destroy(_goTitleWindow);
        yield return null;
	}

    public override void OrientationChanged()
    {
    }

    //void LoadCompleteListener(GameObject go)
    //{
    //    Debug.Log(go.name);
    //    goTitleWindow = go;
    //}

    void LoadAssetListCompleteListener(Dictionary<string, Object> assetDic)
    {
        Debug.Log(assetDic.Count);
        string prefabPath = "UI/Title/TitleWindow";
        //assetDic[prefabPath]
        _goTitleWindow = AssetLoader.Instantiate(assetDic[prefabPath], GameSystem.Instance.tfPanelMain);
        _goTitleWindow.transform.Find("ScreenTouch").gameObject.SetActive(false);
        StartCoroutine("CrtActiveScreenTouch");
    }

    IEnumerator CrtActiveScreenTouch()
    {
        yield return TableManager.Instance.LoadTableAll();
        yield return DataProvider.Instance.LoadPlayerInfo();

        GameObject goScreenTouch = _goTitleWindow.transform.Find("ScreenTouch").gameObject;
        goScreenTouch.SetActive(true);
        goScreenTouch.AddComponent<ButtonListener>().OnClicked = (ButtonListener btn) =>
        {
            GameSystem.Instance.ChangeScene(eSceneType.Town);
        };
    }
}
