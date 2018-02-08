using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DataProvider : MonoBehaviour
{
    private static DataProvider s_instance;
    static public DataProvider Instance
    {
        get
        {
            if (null == s_instance)
            {
                Debug.LogError("Failed To Find DataProvider Instance");
            }
            return s_instance;
        }
    }

    void Awake()
    {
        s_instance = FindObjectOfType<DataProvider>();
    }

    public string sData = "Data string";

    public PlayerInfo playerInfo;


    public IEnumerator LoadPlayerInfo()
    {
        yield return null;
        playerInfo = new PlayerInfo();
        playerInfo.id = "id_diablo";
        playerInfo.name = "Diablo";
    }
}
