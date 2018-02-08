using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TownScene : BaseScene
{
    private GameObject _goTownWindow;

    public override IEnumerator Initialize()
    {
        Debug.Log( string.Format("[{0}] Initialize Start", this.GetType().Name));
        yield return null;

        string prefabPath = "UI/Town/TownWindow";

        List<string> pathList = new List<string>();
        pathList.Add(prefabPath);
        yield return AssetLoader.LoadList(GameSystem.Instance.UseBundleUI, pathList, LoadAssetListCompleteListener);

        Debug.Log(string.Format("[{0}] Initialize End", this.GetType().Name));
    }

    public override IEnumerator Terminate()
	{
        yield return null;
	}

    public override void OrientationChanged()
    {
        _goTownWindow.transform.Find("Player").localPosition = RotationManager.Instance.CalcPosScreenRatio(-360, 640);
    }

    void LoadAssetListCompleteListener(Dictionary<string, Object> assetDic)
    {
        Debug.Log(assetDic.Count);
        string prefabPath = "UI/Town/TownWindow";
        //assetDic[prefabPath]
        _goTownWindow = AssetLoader.Instantiate(assetDic[prefabPath], GameSystem.Instance.tfPanelMain);

        _goTownWindow.transform.Find("Player/lblPlayerName").GetComponent<UILabel>().text = DataProvider.Instance.playerInfo.name;

        OrientationChanged();
    }
}
