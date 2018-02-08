using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using LitJson;

namespace ABManager
{
    public class ABSettingsObject : ScriptableObject
    {
    }


    [CustomEditor(typeof(ABSettingsObject))]
    internal class ABSettingEditor : Editor
    {
        private static ABSettingsObject settingObject = null;

        public static void Show()
        {
            // Show dummy object in inspector
            if (settingObject == null)
            {
                settingObject = CreateInstance<ABSettingsObject>();
                settingObject.hideFlags = HideFlags.DontSave;
                settingObject.name = "AB Settings";
            }

            Selection.activeObject = settingObject;
        }

        public override void OnInspectorGUI()
        {
            //TODO Save Setting
            /*
             * url server
             * url local
             * build output path
             * Caching.maximumAvailableDiskSpace
             * 
             * make json
             * json include file size
             * json include type
             * 
             * downloader use type dic
             * 
             */

            EditorGUILayout.BeginVertical();
            {
                //settingObject.URL_SERVER = EditorGUILayout.TextField("Server url", settingObject.URL_SERVER);

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Save"))
                    {
                        string jsonPath = ABManager.PATH_DATA;
                        if (!Directory.Exists(jsonPath))
                            Directory.CreateDirectory(jsonPath);
                        jsonPath = Path.Combine(jsonPath, "ABSetting.json");
                        Debug.Log("[jsonPath] " + jsonPath);

                        TextWriter tw = new StreamWriter(jsonPath);
                        if (tw == null)
                        {
                            Debug.LogError("Cannot write to " + jsonPath);
                            return;
                        }

                        JsonWriter jw = new JsonWriter();
                        jw.PrettyPrint = true;
                        JsonMapper.ToJson(new ABUtil(), jw);
                        string jsonStr = jw.ToString();
                        jsonStr = jsonStr.Replace("\\", "/").Replace("//", "/");
                        //Debug.Log(jsonStr);
                        tw.Write(jsonStr);

                        tw.Flush();
                        tw.Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}
