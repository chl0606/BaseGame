using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ABManager
{
    [CustomEditor(typeof(ScriptableObject))]
    public class ABInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            ABInspectorDrawer.OnInspectorGUI();
        }

        void OnEnable()
        {
            ABInspectorDrawer.CurrentEditor = this;
        }
    }

    public class ABInspectorDrawer
    {
        private const string VALUE_TOTAL = "Total [%s1]";
        private const string VALUE_REMOVE = "Remove : ";
        private const string VALUE_ITEMS = " items";
        private const string VALUE_UNSELECTED = "Select bundle to check its content.";

        private const float ITEM_HEIGHT = 22f;

        public static Editor CurrentEditor = null;

        private static ScriptableObject inspectorObject = null;

        private static string assetBundleName;
        private static List<string> pathList = null;

        private static Vector2 scrollPos = Vector2.zero;
        private static List<string> selectionList = new List<string>();
        private static string scrollSelection = string.Empty;

        public static void ShowBundle(string bundleName)
        {
            assetBundleName = bundleName;
            if (null == inspectorObject)
            {
                inspectorObject = ScriptableObject.CreateInstance<ScriptableObject>();
                inspectorObject.hideFlags = HideFlags.DontSave;
            }
            Selection.activeObject = inspectorObject;

            scrollSelection = string.Empty;

            Refresh();
        }

        public static void Refresh()
        {
            pathList = new List<string>(AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName));

            if (!string.IsNullOrEmpty(assetBundleName) && null != Selection.activeObject)
                Selection.activeObject.name = assetBundleName;

            if (CurrentEditor != null)
                CurrentEditor.Repaint();
        }

        public static void OnInspectorGUI()
        {
            if (null == pathList)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(VALUE_UNSELECTED);
                GUILayout.FlexibleSpace();
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal(GUIStyle.none);
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.Label(VALUE_TOTAL.Replace("[%s1]", pathList.Count.ToString()), EditorStyles.largeLabel);

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                {
                    for (int i = 0; i < pathList.Count; i++)
                    {
                        GUI_Item(pathList[i]);
                    }

                    KeyProcess();

                }
                EditorGUILayout.EndScrollView();

                Rect scrollViewRect = GUILayoutUtility.GetLastRect();
                if (scrollViewRect.height != 1)
                    UpdateScroll(scrollViewRect.height);

            }
            EditorGUILayout.EndVertical();
        }

        static bool GUI_Item(string path)
        {
            bool isSelected = selectionList.Contains(path);

            Rect itemRect = EditorGUILayout.BeginHorizontal(EditorStyles.textArea);
            //GUILayout.Label(false ? icon_sceneBundleIcon : icon_assetBundleIcon
            //    , style_BItemLabelNormal, expandwidth_false);

            string[] pathSplit = path.Split('/');
            string assetName = pathSplit[pathSplit.Length - 1];
            GUILayout.Label(assetName, ABStyleUtil.GetStyle(isSelected ? ABStyleUtil.Type.LabelSelected : ABStyleUtil.Type.LabelNormal));
            EditorGUILayout.EndHorizontal();

            SelectProcess(itemRect, path);

            RightClickProcess(itemRect, assetName);

            return true;
        }

        #region Process

        static void SelectProcess(Rect itemRect, string path)
        {
            if (RectClicked(itemRect))
            {
                if (ControlPressed())
                {
                    if (selectionList.Contains(path))
                        selectionList.Remove(path);
                    else
                        selectionList.Add(path);
                }
                else if (ShiftPressed())
                {
                    ShiftSelection(path);
                }
                else if (Event.current.button == 0 || !selectionList.Contains(path))
                {
                    selectionList.Clear();
                    selectionList.Add(path);

                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
                }

                CurrentEditor.Repaint();
            }
        }

        static void KeyProcess()
        {
            if (0 == pathList.Count)
                return;

            switch (Event.current.type)
            {
                case EventType.KeyDown:
                    KeyCode key = Event.current.keyCode;
                    switch (key)
                    {
                        case KeyCode.UpArrow:
                        case KeyCode.DownArrow:
                            string lastSelect = string.Empty;
                            if (selectionList.Count > 0)
                                lastSelect = selectionList[selectionList.Count - 1];

                            int lastIndex = pathList.FindIndex(x => x == lastSelect);
                            int newIndex = lastIndex + (key == KeyCode.UpArrow ? -1 : +1);
                            if (newIndex < 0)
                                newIndex = 0;
                            else if (newIndex >= pathList.Count)
                                newIndex = pathList.Count - 1;

                            string addPath = pathList[newIndex];
                            if (Event.current.shift)
                            {
                                ShiftSelection(addPath);
                            }
                            else
                            {
                                selectionList.Clear();
                                selectionList.Add(addPath);
                            }

                            Event.current.Use();
                            CurrentEditor.Repaint();
                            break;
                    }
                    break;
            }
        }

        static void RightClickProcess(Rect itemRect, string path)
        {
            GenericMenu rightClickMenu = new GenericMenu();

            string deleteTarget = path;
            if (1 < selectionList.Count)
                deleteTarget = selectionList.Count + VALUE_ITEMS;

            rightClickMenu.AddItem(new GUIContent(VALUE_REMOVE + deleteTarget), false, RemoveSelected);
            if (MouseOn(itemRect) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                Vector2 mousePos = Event.current.mousePosition;
                rightClickMenu.DropDown(new Rect(mousePos.x, mousePos.y, 0, 0));
            }
        }

        static void UpdateScroll(float viewHeight)
        {
            if (selectionList.Count == 0)
            {
                scrollSelection = string.Empty;
                return;
            }

            string newSelection = selectionList[selectionList.Count - 1];
            if (newSelection == scrollSelection)
                return;

            scrollSelection = newSelection;

            int selectionRow = pathList.FindIndex(x => x == newSelection);
            if (selectionRow < 0)
                return;

            float selectTopOffset = selectionRow * ITEM_HEIGHT;
            if (selectTopOffset < scrollPos.y)
                scrollPos.y = selectTopOffset;
            else if (selectTopOffset + ITEM_HEIGHT > scrollPos.y + viewHeight)
                scrollPos.y = selectTopOffset + ITEM_HEIGHT - viewHeight;

            CurrentEditor.Repaint();
        }

        #endregion

        #region WorkFunction

        static void ShiftSelection(string path)
        {
            if (selectionList.Count == 0)
            {
                selectionList.Add(path);
                return;
            }

            int minIndex = int.MaxValue;
            int maxIndex = int.MinValue;
            foreach (string selection in selectionList)
            {
                int selIndex = pathList.IndexOf(selection);
                if (selIndex == -1)
                    continue;

                if (minIndex > selIndex)
                    minIndex = selIndex;
                if (maxIndex < selIndex)
                    maxIndex = selIndex;
            }

            if (minIndex == int.MaxValue || maxIndex == int.MinValue)
            {
                selectionList.Add(path);
                return;
            }

            int from = 0;
            int to = pathList.IndexOf(path);
            if (to >= minIndex && to <= maxIndex)
                from = pathList.IndexOf(selectionList[0]);
            else if (to < minIndex)
                from = maxIndex;
            else if (to > maxIndex)
                from = minIndex;

            int step = to > from ? 1 : -1;
            selectionList.Clear();
            while (from != to + step)
            {
                selectionList.Add(pathList[from]);
                from += step;
            }
        }

        static void RemoveSelected()
        {
            foreach (string path in selectionList)
            {
                AssetImporter ai = AssetImporter.GetAtPath(path);

                if (string.IsNullOrEmpty(ai.assetBundleName))
                {
                    EditorUtility.DisplayDialog(Messages.TITLE_ERROR
                                , Messages.MESSAGE_ERROR_FOLDERASSET, Messages.BUTTON_CLOSE);
                    return;
                }
            }

            foreach (string path in selectionList)
            {
                ABManager.SetAssetBundleName(path, null, null);
            }

            AssetDatabase.Refresh();
            Refresh();
        }

        #endregion

        static bool RectClicked(Rect rect)
        {
            return Event.current.type == EventType.MouseDown && MouseOn(rect);
        }

        static bool ControlPressed()
        {
            return (Event.current.control && Application.platform == RuntimePlatform.WindowsEditor) ||
                (Event.current.command && Application.platform == RuntimePlatform.OSXEditor);
        }

        static bool ShiftPressed()
        {
            return Event.current.shift;
        }

        static bool MouseOn(Rect rect)
        {
            return rect.Contains(Event.current.mousePosition);
        }
    }
}
