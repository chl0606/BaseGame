using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class TableManager
{
    private static TableManager s_instance = new TableManager();
    public static TableManager Instance
    {
        get
        {
            return s_instance;
        }
    }

    #region DataManifest
    public const string PATH_DATA_MANIFEST = "Tables/DataManifest";
    public Dictionary<string, Dictionary<string, int>> m_DataManifestDict = new Dictionary<string, Dictionary<string, int>>();
    public IEnumerator LoadManifest()
    {
        m_DataManifestDict.Clear();

        TextAsset ta = null;
        if (GameSystem.Instance.UseBundleTable)
        {
            string[] splitPath = PATH_DATA_MANIFEST.Split('/');
            AssetBundleLoadAssetOperation operation = ABDownloader.Instance.LoadAssetAsync<TextAsset>(splitPath[splitPath.Length - 1]);
            yield return operation;

            ta = operation.GetAsset<TextAsset>();

        }
        else
        {
            var resourceRequest = Resources.LoadAsync(PATH_DATA_MANIFEST);
            yield return resourceRequest;

            ta = resourceRequest.asset as TextAsset;
        }

        //if (ta == null)
        //{
        //    ta = Resources.Load<TextAsset>(PATH_DATA_MANIFEST);
        //}


        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(ta.text);

        XmlElement nodes = xmlDoc.LastChild as XmlElement;

        for (int i = 0; i < nodes.ChildNodes.Count; ++i)
        {
            XmlElement dataNode = nodes.ChildNodes.Item(i) as XmlElement;
            string dataName = dataNode.GetAttribute("name");

            Dictionary<string, int> dataDict = new Dictionary<string, int>();
            if (!m_DataManifestDict.ContainsKey(dataName))
            {
                m_DataManifestDict.Add(dataName, dataDict);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("error data: " + dataName);
#endif
            }

            for (int j = 0; j < dataNode.ChildNodes.Count; ++j)
            {
                XmlElement colNode = dataNode.ChildNodes.Item(j) as XmlElement;
                string colName = colNode.GetAttribute("name");
                int colIndex = int.Parse(colNode.GetAttribute("index"));

                if (!dataDict.ContainsKey(colName))
                    dataDict.Add(colName, colIndex);
                else
                {
//#if UNITY_EDITOR
//                    // 에러로그는 중복 컬럼 이름이 문제인지 확인을 하기 위한 목적임, 중복 이름 무시해도 되는 경우에는 여기서 무시 처리 한다!
//                    if (dataName != "JewelOptionData" && dataName != "EquipGradeTypeData")
//                        Debug.LogError(string.Format("ERROR Data LoadManifest: duplicated column = {0} in data = {1}", colName, dataName));
//#endif
                }
            }
        }
    }

    public void ClearManifest()
    {
        m_DataManifestDict.Clear();
    }


    // 컬럼 정보 가져오기
    public void GetDataColumnIndices<T>(string dataName) where T : class
    {
        System.Type typeT = typeof(T);

        Dictionary<string, int> dict = null;

        if (m_DataManifestDict.TryGetValue(dataName, out dict))
        {
            foreach (KeyValuePair<string, int> pair in dict)
            {
                string columnName = pair.Key;
                int columnIndex = pair.Value;

                string fieldName = columnName;

                char fieldFirstC = fieldName.ToCharArray(0, 1)[0];
                if (char.IsDigit(fieldFirstC))
                    fieldName = "_" + fieldName;

                System.Reflection.FieldInfo fieldinfo = typeT.GetField(fieldName);
                if (fieldinfo != null)
                {
                    // static 변수일테니 instance 없이 setvalue 할 수 있다.
                    fieldinfo.SetValue(null, columnIndex);
                }
            }
        }
    }
    #endregion

    public StringTable stringTable;

    public GeneralSettingTable generalSettingTable;


    public IEnumerator LoadTableAll()
    {
        yield return LoadManifest();

        stringTable = new StringTable();
        yield return stringTable.LoadTableAsync();
        generalSettingTable = new GeneralSettingTable();
        yield return generalSettingTable.LoadTableAsync();
        
        Debug.Log(generalSettingTable.GetData("title_name").Value);
    }
}
