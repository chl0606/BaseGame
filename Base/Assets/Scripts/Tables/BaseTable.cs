using UnityEngine;
using System.Collections;


public class BaseTable
{
    protected delegate void ParseTable(TextAsset ta);

    protected IEnumerator LoadTableAsync(string tablePath, ParseTable parseCallback)
    {
        if (GameSystem.Instance.UseBundleTable)
        {
            string[] splitPath = tablePath.Split('/');
            AssetBundleLoadAssetOperation operation = ABDownloader.Instance.LoadAssetAsync<TextAsset>(splitPath[splitPath.Length - 1]);
            yield return operation;

            if (null != parseCallback)
                parseCallback(operation.GetAsset<TextAsset>());
        }
        else
        {
            var resourceRequest = Resources.LoadAsync(tablePath);
            yield return resourceRequest;

            if (null != parseCallback)
                parseCallback(resourceRequest.asset as TextAsset);
        }
    }


    //public struct DataFieldDesc
    //{
    //    public string m_FieldName;
    //    public int m_ListCount;

    //    public DataFieldDesc(string fieldName)
    //    {
    //        m_FieldName = fieldName;
    //        m_ListCount = 0;
    //    }

    //    public DataFieldDesc(string fieldName, int count)
    //    {
    //        m_FieldName = fieldName;
    //        m_ListCount = count;
    //    }
    //}

    //protected bool ParseLine(string[] data, object data_instance, params object[] args)
    //{
    //    if (data.Length < args.Length)
    //    {
    //        Debug.LogError(string.Format("ParseLine error. data={0}, num fields={1}, found {2} items in the line", data_instance.GetType(), args.Length, data.Length));
    //        return false;
    //    }

    //    int j = 0;
    //    for (int i = 0; i < args.Length; ++i)
    //    {
    //        DataFieldDesc desc = (DataFieldDesc)args[i];
    //        string fieldName = desc.m_FieldName;

    //        System.Reflection.FieldInfo fieldinfo = data_instance.GetType().GetField(fieldName);
    //        if (fieldinfo != null)
    //        {
    //            string cur = data[j];
    //            if (fieldinfo.FieldType == typeof(int))
    //            {
    //                int IntValue = int.Parse(cur);
    //                fieldinfo.SetValue(data_instance, IntValue);
    //            }
    //            if (fieldinfo.FieldType == typeof(long))
    //            {
    //                long LongValue = long.Parse(cur);
    //                fieldinfo.SetValue(data_instance, LongValue);
    //            }
    //            if (fieldinfo.FieldType == typeof(float))
    //            {
    //                float FloatValue = float.Parse(cur);
    //                fieldinfo.SetValue(data_instance, FloatValue);
    //            }
    //            else if (fieldinfo.FieldType == typeof(bool))
    //            {
    //                bool BoolValue = bool.Parse(cur);
    //                fieldinfo.SetValue(data_instance, BoolValue);
    //            }
    //            else if (fieldinfo.FieldType == typeof(string))
    //            {
    //                string StrValue = cur;
    //                fieldinfo.SetValue(data_instance, StrValue);
    //            }
    //            else if (fieldinfo.FieldType == typeof(List<int>))
    //            {
    //                List<int> list = fieldinfo.GetValue(data_instance) as List<int>;

    //                int n = desc.m_ListCount;
    //                if (n <= 0)
    //                    n = 1;
    //                for (int x = 0; x < n; ++x)
    //                {
    //                    cur = data[j + x];
    //                    int IntValue = int.Parse(cur);
    //                    list.Add(IntValue);
    //                }
    //                j += n;
    //                continue;
    //            }
    //            else if (fieldinfo.FieldType.BaseType == typeof(Enum))
    //            {
    //                fieldinfo.SetValue(data_instance, Enum.Parse(fieldinfo.FieldType, cur, true));
    //            }

    //            ++j;
    //        }
    //    }

    //    return true;
    //}

    //protected delegate void FunctionParseLineAndAddTableData(string[] data);


    //protected virtual void ParseLineAndAddTableData(string[] data)
    //{
    //    // default로 이 함수가 매 line을 파싱 할때 호출 된다. override 해서 내용을 작성하면 된다.
    //}

    //protected void ParseTableNew(TextAsset ta, FunctionParseLineAndAddTableData parseCallback = null)
    //{
    //    ByteReader br = new ByteReader(ta);

    //    string tableName = br.ReadLine();
    //    string tableEnd = tableName + " END";
    //    string str = "";

    //    do
    //    {
    //        str = br.ReadLine();
    //        string[] data = str.Split('\t');

    //        if (data[0] == tableEnd)
    //            break;

    //        if (parseCallback != null)
    //            parseCallback(data);
    //        else
    //            ParseLineAndAddTableData(data);

    //    } while (br.canRead);
    //}

    //protected IEnumerator LoadAsyncAndParse(string tableName, FunctionParseLineAndAddTableData parseCallback)
    //{
    //    if (GameSystem.Instance.UseAssetBundleData)
    //    {
    //        AssetBundleLoadAssetOperation operation = ABDownloader.Instance.LoadAssetAsync<TextAsset>(tableName);
    //        yield return operation;

    //        ParseTableNew(operation.GetAsset<TextAsset>(), parseCallback);
    //    }
    //    else
    //    {
    //        var async = Resources.LoadAsync(tableName);
    //        yield return async;

    //        ParseTableNew(async.asset as TextAsset, parseCallback);
    //    }
    //}
}

