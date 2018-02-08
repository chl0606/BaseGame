using UnityEngine;
using System;
using System.Collections;

public class RandomUtil 
{
    private static float[] m_aryValue = null;
    private static int m_index = 0;
    
    //0f ~ 1.0f사이의 값을 리턴
    public static float ValueFloat
    {
        get
        {
            if (null == m_aryValue)
            {
                MakeArray();
            }
            else
            {
                m_index++;
                m_index = m_aryValue.Length <= m_index ? 0 : m_index;
            }

            return m_aryValue[m_index];
        }
    }

    //1 ~ 100사이의 값을 리턴
    public static int ValueInt
    {
        get
        {
            if (null == m_aryValue)
            {
                MakeArray();
            }
            else
            {
                m_index++;
                m_index = m_aryValue.Length <= m_index ? 0 : m_index;
            }

            return (int)Math.Round(m_aryValue[m_index] * 100f);
        }
    }

    public int Index { get { return m_index; } }

    private static void MakeArray()
    {
        m_index = 0;
        m_aryValue = new float[1000];
        for (int i = 0; i < m_aryValue.Length; i++)
        {
            m_aryValue[i] = UnityEngine.Random.Range(0f, 1.0f);
        }
    }


}
