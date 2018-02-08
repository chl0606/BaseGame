using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ABManager
{
    internal class ABMonitor : EditorWindow
    {
        private const string VALUE_REFRESH = "Refresh";

        private Vector2 scrollPos = Vector2.zero;
        private Dictionary<string, LoadedAssetBundle> dicLoadedAssetBundles;
        private Dictionary<string, string[]> dicDependencies = new Dictionary<string, string[]>();

        [MenuItem("Window/AB Monitor")]
        static void Init()
        {
            EditorWindow.GetWindow<ABMonitor>("AB Monitor");
        }

        void OnFocus()
        {
            Repaint();
        }

        void Awake()
        {
        }

        void Update()
        {
            if (Application.isPlaying == false || null == ABDownloader.Instance)
            {
                return;
            }

            if (null == dicLoadedAssetBundles)
            {
                dicLoadedAssetBundles = ABDownloader.Instance.getDicLoadedAssetBundles;
                dicDependencies = ABDownloader.Instance.getDicDependencies;
            }
        }


        #region GUI

        void OnGUI()
        {
            if (Application.isPlaying == false || null == ABDownloader.Instance)
            {
                EditorGUILayout.HelpBox("Runs in Logined Play mode", MessageType.Info);
                return;
            }

            if (null == dicLoadedAssetBundles) return;

            EditorGUILayout.BeginVertical();
            {
                // Toobar
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                {
                    if (GUILayout.Button(VALUE_REFRESH, EditorStyles.toolbarButton))
                    {
                        Repaint();
                    }

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                // Loaded
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.ExpandWidth(true));
                    {
                        GUILayout.Label("LOADED COUNT : ", EditorStyles.label);

                        GUILayout.FlexibleSpace();

                        GUILayout.Label(dicLoadedAssetBundles.Count.ToString(), EditorStyles.label);
                    }
                    EditorGUILayout.EndHorizontal();

                    Dictionary<string, LoadedAssetBundle>.Enumerator enumLoaded = dicLoadedAssetBundles.GetEnumerator();
                    while (enumLoaded.MoveNext())
                    {
                        if (!GUI_ItemLoaded(enumLoaded.Current))
                        {
                            Repaint();
                            break;
                        }
                    }

                    // Dependencie
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.ExpandWidth(true));
                    {
                        GUILayout.Label("Dependencies COUNT : ", EditorStyles.label);

                        GUILayout.FlexibleSpace();

                        GUILayout.Label(dicDependencies.Count.ToString(), EditorStyles.label);
                    }
                    EditorGUILayout.EndHorizontal();

                    Dictionary<string, string[]>.Enumerator enumDependencie = dicDependencies.GetEnumerator();
                    while (enumDependencie.MoveNext())
                    {
                        if (!GUI_ItemDependencie(enumDependencie.Current))
                        {
                            Repaint();
                            break;
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        bool GUI_ItemLoaded(KeyValuePair<string, LoadedAssetBundle> keyValue)
        {
            ABStyleUtil.Type styleType = ABStyleUtil.Type.LabelNormal;

            GUIStyle style = ABStyleUtil.GetStyle(styleType);

            EditorGUILayout.BeginHorizontal(EditorStyles.textArea);
            {
                GUILayout.Label(keyValue.Key, style);
                GUILayout.FlexibleSpace();
                GUILayout.Label(keyValue.Value.referencedCount.ToString(), style);
            }
            EditorGUILayout.EndHorizontal();

            return true;
        }

        bool GUI_ItemDependencie(KeyValuePair<string, string[]> keyValue)
        {
            ABStyleUtil.Type styleType = ABStyleUtil.Type.LabelNormal;

            GUIStyle style = ABStyleUtil.GetStyle(styleType);

            EditorGUILayout.BeginHorizontal(EditorStyles.textArea);
            {
                GUILayout.Label(keyValue.Key, style);
                GUILayout.FlexibleSpace();
                GUILayout.Label(keyValue.Value.Length.ToString(), style);
            }
            EditorGUILayout.EndHorizontal();

            return true;
        }

        #endregion

    }
}

