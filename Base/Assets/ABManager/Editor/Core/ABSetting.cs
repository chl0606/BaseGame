using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace ABManager
{
    internal class ABSetting : ScriptableObject
    {
        public int maximumAvailableDiskSpace = 1073700000;

        //[MenuItem("Windows/Create ABSetting")]
        //public static void CreateABSetting()
        //{
        //    ABSetting asset = new ABSetting();  //scriptable object
        //    AssetDatabase.CreateAsset(asset, Path.Combine(ABManager.PATH_DATA, "ABSetting.asset"));
        //    AssetDatabase.SaveAssets();
        //    EditorUtility.FocusProjectWindow();
        //    Selection.activeObject = asset;
        //}
    }
}