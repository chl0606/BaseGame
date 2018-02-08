using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PanelMain : MonoBehaviour {

	private static PanelMain m_instance = null;

    public static PanelMain Instance
    {
        get
        {
            if (null == m_instance)
            {
                m_instance = FindObjectOfType(typeof(PanelMain)) as PanelMain;
                if (null == m_instance)
                {
                    Debug.LogError("Fail To Get PanelMain Instance");
                }
            }
            return m_instance;
        }
    }

    public bool Initialize()
    {
//        GameObject.DontDestroyOnLoad( this );
		
		return true;
	}
	
    public void Terminate()
    {
        GameObject.DestroyImmediate( gameObject );
    }

    public void ClearAll()
    {
        int count = 0;

        while (transform.childCount > count)
        {
            Transform child = transform.GetChild(count);
            GameObject.DestroyImmediate(child.gameObject);
        }
    }

	public void Clear(List<string> exceptList = null)
	{
        int count = 0;

        Debug.Log("Clear Start!!");
        Debug.Log("transform.childCount : " + transform.childCount);

        if (null == exceptList)
        {
            ClearAll();
            return;
        }

        while (transform.childCount > count)
		{
            Transform child = transform.GetChild(count);

            Debug.Log("child.name : " + child.name);

            if (exceptList.Contains(child.name))
            {
                Debug.Log("count : " + count + ", child.name : " + child.name);
                ++count;
                continue;
            }

            GameObject.DestroyImmediate(child.gameObject);
		}

        Debug.Log("Clear End!!");
	}
	
}
