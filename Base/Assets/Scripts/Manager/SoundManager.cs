using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;


public class SoundManager : MonoBehaviour
{
    private static SoundManager s_instance;
    public static SoundManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = FindObjectOfType<SoundManager>();
                
                if (null == s_instance)
                    Debug.LogError("Failed To Find SoundManager Instance");
            }
            return s_instance;
        }
    }

    public enum SoundType
    {
        BGM = 0,
        SFX = 1,
        Voice = 2,
    }
}
