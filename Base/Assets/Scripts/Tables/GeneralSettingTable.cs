using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GeneralSettingData
{
    public string ID;
    public string Value;
}

public class GeneralSettingTable
{
    private string TABLE_PATH = "Tables/GeneralSetting";

    public Dictionary<string, GeneralSettingData> dicGeneralSetting;

    public IEnumerator LoadTableAsync()
    {
        yield return AssetLoader.LoadTable(TABLE_PATH, Parse);
    }

    public void Parse(TextAsset ta)
    {
        ByteReader br = new ByteReader(ta);

        string tableName = br.ReadLine();
        StringBuilder tableEnd = new StringBuilder(tableName);
        tableEnd.Append(" END");
        string str = "";

        TableManager.Instance.GetDataColumnIndices<GeneralSettingDataManifest>(GeneralSettingDataManifest.DATA_NAME);

        dicGeneralSetting = new Dictionary<string, GeneralSettingData>();
        do
        {
            str = br.ReadLine();
            string[] data = str.Split('\t');

            if (string.Compare(data[0], tableEnd.ToString()) == 0)
            {
                break;
            }

            GeneralSettingData info = new GeneralSettingData();
            info.ID = data[GeneralSettingDataManifest.ID];
            info.Value = data[GeneralSettingDataManifest.Value];

            if (!dicGeneralSetting.ContainsKey(info.ID))
            {
                dicGeneralSetting.Add(info.ID, info);
            }
        } while (br.canRead);
    }

    public GeneralSettingData GetData(string id)
    {
        if (dicGeneralSetting.ContainsKey(id))
            return dicGeneralSetting[id];

        return null;
    }
}
