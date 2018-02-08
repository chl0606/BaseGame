using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ItemStringData
{
    public string Name;
    public string Desc;
}

public class StringTable
{
    private delegate void ParseTable(TextAsset ta);

    private string TABLE_PATH_GAMEUI = "Localize/GameUI";
    private string TABLE_PATH_ITEMSTRING = "Localize/ItemString";

    public Dictionary<string, string> dicGameUI;
    public Dictionary<string, ItemStringData> dicItemString;

    public IEnumerator LoadTableAsync()
    {
        string language = GameSystem.language.ToString();
        yield return AssetLoader.LoadTable(string.Format("{0}_{1}", TABLE_PATH_GAMEUI, language), ParseGameUI);
        yield return AssetLoader.LoadTable(string.Format("{0}_{1}", TABLE_PATH_ITEMSTRING, language), ParseItemString);
    }

    #region GameUI
    public void ParseGameUI(TextAsset ta)
    {
        ByteReader br = new ByteReader(ta);

        string tableName = br.ReadLine();
        StringBuilder tableEnd = new StringBuilder(tableName);
        tableEnd.Append(" END");
        string str = "";

        TableManager.Instance.GetDataColumnIndices<GeneralSettingDataManifest>(GeneralSettingDataManifest.DATA_NAME);

        dicGameUI = new Dictionary<string, string>();
        do
        {
            str = br.ReadLine();
            string[] data = str.Split('\t');

            if (string.Compare(data[0], tableEnd.ToString()) == 0)
            {
                break;
            }

            int idx = 0;
            string ID = data[idx++];
            string Value = data[idx++];

            if (!dicGameUI.ContainsKey(ID))
            {
                dicGameUI.Add(ID, Value);
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError(string.Format("GameUI has duplicated ID : {0}", ID));
            }
#endif
        } while (br.canRead);
    }

    public string GetGameUI(string stringId)
    {
        if (dicGameUI.ContainsKey(stringId))
            return dicGameUI[stringId];

        return stringId;
    }
    #endregion

    #region ItemString
    public void ParseItemString(TextAsset ta)
    {
        ByteReader br = new ByteReader(ta);

        string tableName = br.ReadLine();
        StringBuilder tableEnd = new StringBuilder(tableName);
        tableEnd.Append(" END");
        string str = "";

        TableManager.Instance.GetDataColumnIndices<GeneralSettingDataManifest>(GeneralSettingDataManifest.DATA_NAME);

        dicItemString = new Dictionary<string, ItemStringData>();
        do
        {
            str = br.ReadLine();
            string[] data = str.Split('\t');

            if (string.Compare(data[0], tableEnd.ToString()) == 0)
            {
                break;
            }

            int idx = 0;
            string ID = data[idx++];
            ItemStringData info = new ItemStringData();
            info.Name = data[idx++];
            info.Desc = data[idx++];

            if (!dicItemString.ContainsKey(ID))
            {
                dicItemString.Add(ID, info);
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError(string.Format("ItemString has duplicated ID : {0}", ID));
            }
#endif
        } while (br.canRead);
    }

    public ItemStringData GetItemString(string stringId)
    {
        if (dicItemString.ContainsKey(stringId))
            return dicItemString[stringId];

        return new ItemStringData();
    }
    #endregion
}
