using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ABManager
{
    internal class ABWindow : EditorWindow
    {
        private const string KEY_DRAG_HERE_ONE = "DRAG HERE \nMake One AssetBundle";
        private const string KEY_DRAG_HERE_EACH = "DRAG HERE \nMake Each AssetBundle";
        private const string KEY_DRAG_IDENTIFIER = "ABWindow";
        private const string KEY_PREFIX_TEXTFIELDNAME = "PrefixTextfieldName";
        private const string KEY_RENAME_TEXTFIELDNAME = "RenameTextfieldName";
        private const string KEY_REVARIANT_TEXTFIELDNAME = "RevariantTextfieldName";

        private const string VALUE_ISRUNNING = "Application is Running";
        private const string VALUE_MENU = "Menu";
        private const string VALUE_REFRESH = "Refresh";
        private const string VALUE_TOTAL = "Total {0}";
        private const string VALUE_PREFIX = "Prefix :";
        private const string VALUE_REMOVE_UNUSED = "Remove Unused";
        private const string VALUE_BUILD_ASSETBUNDLE = "Build AssetBundle";
        private const string VALUE_BUILD_ASSETBUNDLE_REPLACE = "Build AssetBundle Replace";
        private const string VALUE_OPEN_BUILDFOLDER = "Open Build Folder";
        private const string VALUE_ITEMS = "{0} items";
        private const string VALUE_VARIANT = " variant : ";
        private const string VALUE_RENAME = "Rename : ";
        private const string VALUE_ADDVARINT = "AddVariant";
        private const string VALUE_REVARIANT = "Revariant : ";
        private const string VALUE_DELETE = "Delete : ";

        private const string EXTENSION_SCENE = ".unity";
        private const string STRING_DOT = ".";
        private const char CHAR_DOT = '.';

        private const float ITEM_HEIGHT = 22f;

        private bool isSetPrefix = false;
        private string prefixString = string.Empty;

        private List<string> nameList;
        private List<int> countList;
        private List<string> variantList;

        private Vector2 scrollPos = Vector2.zero;
        private List<string> selectionList = new List<string>();
        private string scrollSelection = string.Empty;

        private string currentRecieving = string.Empty;
        private DragHandler dragHandler = null;

        private string currentRenaming = string.Empty;
        private string nextRenaming = string.Empty;
        private bool renameNeedFocus = false;
        private bool revariantNeedFocus = false;
        private string editString = string.Empty;

        private bool isRenaming = false;
        private bool isRevariant = false;

        [MenuItem("Window/AB Viewer")]
        static void Init()
        {
            EditorWindow.GetWindow<ABWindow>("AB Viewer");
        }

        void OnFocus()
        {
            if (!Application.isPlaying)
                EditorApplication.delayCall += RefreshAssetDatabase;
        }

        void Awake()
        {
        }

        void Update()
        {
            if (Application.isPlaying)
                return;

            if (null == nameList)
            {
                RefreshAssetDatabase();
            }
            else if (!string.IsNullOrEmpty(nextRenaming))
            {
                StartEditAssetBundleName();
            }
        }

        void RemoveUnusedAssetBundleNames()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            EditorApplication.delayCall += RefreshAssetDatabase;
        }

        void OpenBuildFolder()
        {
            ABManager.ShowExplorerOutputPath();
        }

        void BuildAssetsBundles()
        {
            ABManager.BuildAllAssetBundles(true, false);
        }

        void BuildAssetsBundlesReplace()
        {
            ABManager.BuildAllAssetBundles(true, true);
        }

        #region GUI

        void OnGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical();
                {
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    {
                        GUILayout.FlexibleSpace();
                        GUIStyle style = ABStyleUtil.GetStyle(ABStyleUtil.Type.LabelSpaceNormal);
                        GUILayout.Label(VALUE_ISRUNNING, style);
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndVertical();
                return;
            }

            if (null == nameList) return;

            if (null == dragHandler)
            {
                // Setup GUI handler
                dragHandler = new DragHandler();
                dragHandler.dragIdentifier = KEY_DRAG_IDENTIFIER;
                dragHandler.AddRecieveIdentifier(dragHandler.dragIdentifier);
                dragHandler.canRecieveCallBack = OnCanRecieve;
                dragHandler.reciveDragCallBack = OnRecieve;
            }

            EditorGUILayout.BeginVertical();
            {
                // Toobar
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                {
                    Rect buildRect = GUILayoutUtility.GetRect(new GUIContent(VALUE_MENU), EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
                    if (GUI.Button(buildRect, VALUE_MENU, EditorStyles.toolbarDropDown))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent(VALUE_REMOVE_UNUSED), false, RemoveUnusedAssetBundleNames);
                        menu.AddItem(new GUIContent(VALUE_OPEN_BUILDFOLDER), false, OpenBuildFolder);
                        menu.AddItem(new GUIContent(VALUE_BUILD_ASSETBUNDLE), false, BuildAssetsBundles);
                        menu.AddItem(new GUIContent(VALUE_BUILD_ASSETBUNDLE_REPLACE), false, BuildAssetsBundlesReplace);
                        menu.DropDown(buildRect);
                    }

                    //Texture2D texture = EditorGUIUtility.FindTexture("Texture Icon");
                    //Rect textureRect = GUILayoutUtility.GetRect(texture.width, texture.width, texture.height, texture.height, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
                    //EditorGUI.DrawTextureTransparent(textureRect, texture);


                    if (GUILayout.Button(VALUE_REFRESH, EditorStyles.toolbarButton))
                    {
                        EditorApplication.delayCall += RefreshAssetDatabase;
                    }

                    GUILayout.FlexibleSpace();


                    GUILayout.Label(string.Format(VALUE_TOTAL, nameList.Count.ToString()), EditorStyles.toolbarButton);
                }
                EditorGUILayout.EndHorizontal();

                //Prefix
                if (!GUI_Prefix())
                {
                    Repaint();
                    return;
                }

                if (!GUI_MakeOne())
                {
                    Repaint();
                    return;
                }
                if (!GUI_MakeEach())
                {
                    Repaint();
                    return;
                }

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                {
                    //Items
                    if (null != countList)
                    {
                        for (int i = 0; i < nameList.Count; i++)
                        {
                            if (!GUI_Item(nameList[i], i < countList.Count ? countList[i] : 0))
                            {
                                Repaint();
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(currentRenaming))
                        {
                            KeyProcess();
                        }
                    }

                    //Rect spaceRect = EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
                    //GUILayout.Space(ITEM_HEIGHT);
                    //EditorGUILayout.EndVertical();
                    //EmptySpaceProcess(spaceRect);
                }
                EditorGUILayout.EndScrollView();
            }

            Rect scrollViewRect = GUILayoutUtility.GetLastRect();
            if (scrollViewRect.height != 1)
                UpdateScroll(scrollViewRect.height);

            EditorGUILayout.EndVertical();
        }

        bool GUI_Prefix()
        {
            Rect rect = EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                GUILayout.Label(VALUE_PREFIX, EditorStyles.toolbarButton);

                if (isSetPrefix)
                {
                    //GUI.SetNextControlName(KEY_PREFIX_TEXTFIELDNAME);
                    prefixString = GUILayout.TextField(prefixString, GUILayout.MinWidth(50));

                    // If lose focus, end this Reneme
                    bool clickOutSideTheTextField = Event.current.type == EventType.MouseDown && !RectClicked(rect);
                    bool isFinishedReneme = Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return;
                    if (!HasFocuse() || clickOutSideTheTextField || isFinishedReneme)
                    {
                        if (string.Empty != prefixString && !IsValidName(prefixString))
                        {
                            EditorUtility.DisplayDialog(Messages.TITLE_ERROR_PREFIX
                                , prefixString + Messages.MESSAGE_ERROR_PREFIX, Messages.BUTTON_CLOSE);
                            return false;
                        }

                        if (Event.current.type == EventType.Layout)
                            return false;

                        prefixString = prefixString.ToLower();
                        isSetPrefix = false;
                        Repaint();
                        GUIUtility.keyboardControl = 0;

                        return false;
                    }
                }
                else
                {
                    GUILayout.Label(prefixString, EditorStyles.label);
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            //Process
            if (RectClicked(rect))
            {
                isSetPrefix = true;
                return false;
            }

            return true;
        }

        bool GUI_MakeOne()
        {
            bool isRecieving = KEY_DRAG_HERE_ONE == currentRecieving;

            ABStyleUtil.Type styleType = ABStyleUtil.Type.LabelSpaceNormal;

            if (isRecieving)
                styleType = ABStyleUtil.Type.LabelSpaceRecieving;

            GUIStyle style = ABStyleUtil.GetStyle(styleType);

            Rect rect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(KEY_DRAG_HERE_ONE, style);
            EditorGUILayout.EndVertical();

            //Process
            if (DragProcess(rect, KEY_DRAG_HERE_ONE)) return false;

            return true;
        }

        bool GUI_MakeEach()
        {
            bool isRecieving = KEY_DRAG_HERE_EACH == currentRecieving;

            ABStyleUtil.Type styleType = ABStyleUtil.Type.LabelSpaceNormal;

            if (isRecieving)
                styleType = ABStyleUtil.Type.LabelSpaceRecieving;

            GUIStyle style = ABStyleUtil.GetStyle(styleType);

            Rect rect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(KEY_DRAG_HERE_EACH, style);
            EditorGUILayout.EndVertical();

            //Process
            if (DragProcess(rect, KEY_DRAG_HERE_EACH)) return false;

            return true;
        }

        bool GUI_Item(string name, int itemCount)
        {
            bool isEditing = name == currentRenaming;
            bool isRecieving = name == currentRecieving;
            bool isSelected = selectionList.Contains(name);

            ABData nameData = GetABDataByAssetBundleName(name);
            string bundleName = nameData.name;
            string variantName = nameData.variant;

            ABStyleUtil.Type styleType = ABStyleUtil.Type.LabelNormal;

            if (isEditing)
                styleType = ABStyleUtil.Type.LabelSelected;
            else if (isRecieving)
                styleType = ABStyleUtil.Type.LabelRecieving;
            else if (isSelected)
                styleType = ABStyleUtil.Type.LabelSelected;
            else if (1 > itemCount)
                styleType = ABStyleUtil.Type.LabelEmpty;

            GUIStyle style = ABStyleUtil.GetStyle(styleType);

            Rect itemRect = EditorGUILayout.BeginHorizontal(EditorStyles.textArea);
            //GUILayout.Label(false ? icon_sceneBundleIcon : icon_assetBundleIcon
            //    , style_BItemLabelNormal, expandwidth_false);

            if (isEditing)
            {
                if (isRenaming)
                {
                    GUI.SetNextControlName(KEY_RENAME_TEXTFIELDNAME);
                    editString = GUILayout.TextField(editString, ABStyleUtil.GetStyle(ABStyleUtil.Type.LabelRenaming));

                    GUILayout.FlexibleSpace();

                    if (!string.IsNullOrEmpty(variantName))
                        GUILayout.Label(VALUE_VARIANT + variantName, style);
                }
                else if (isRevariant)
                {
                    //Name
                    GUILayout.Label(bundleName, style);

                    GUILayout.FlexibleSpace();

                    GUI.SetNextControlName(KEY_REVARIANT_TEXTFIELDNAME);
                    editString = GUILayout.TextField(editString, ABStyleUtil.GetStyle(ABStyleUtil.Type.LabelRenaming));
                }
            }
            else
            {
                //Name
                GUILayout.Label(bundleName, style);

                GUILayout.FlexibleSpace();

                if (!string.IsNullOrEmpty(variantName))
                    GUILayout.Label(VALUE_VARIANT + variantName, style);
            }

            //Count
            GUILayout.Label(string.Format(VALUE_ITEMS, itemCount), style);

            EditorGUILayout.EndHorizontal();

            //Process
            if (EditProcess(itemRect, name)) return false;

            if (DragProcess(itemRect, name)) return false;

            SelectProcess(itemRect, name);

            RightClickProcess(itemRect, name, itemCount);

            return true;
        }

        #endregion

        #region Process

        void SelectProcess(Rect itemRect, string name)
        {
            if (RectClicked(itemRect) && currentRenaming != name)
            {
                if (ControlPressed())
                {
                    if (selectionList.Contains(name))
                        selectionList.Remove(name);
                    else
                        selectionList.Add(name);
                }
                else if (ShiftPressed())
                {
                    ShiftSelection(name);
                }
                else if (Event.current.button == 0 || !selectionList.Contains(name))
                {
                    selectionList.Clear();
                    selectionList.Add(name);
                }

                currentRenaming = string.Empty;
                isRenaming = false;
                isRevariant = false;

                EditorApplication.delayCall += ShowCurrentSelected;
                Repaint();
            }
        }

        void KeyProcess()
        {
            if (0 == nameList.Count)
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

                            int lastIndex = nameList.FindIndex(x => x == lastSelect);
                            int newIndex = lastIndex + (key == KeyCode.UpArrow ? -1 : +1);
                            if (newIndex < 0)
                                newIndex = 0;
                            else if (newIndex >= nameList.Count)
                                newIndex = nameList.Count - 1;

                            string addBundle = nameList[newIndex];
                            if (Event.current.shift)
                            {
                                ShiftSelection(addBundle);
                            }
                            else
                            {
                                selectionList.Clear();
                                selectionList.Add(addBundle);
                            }

                            Event.current.Use();
                            Repaint();
                            break;
                    }
                    break;
            }
        }

        bool DragProcess(Rect itemRect, string name)
        {
            if (Event.current.type == EventType.Repaint || itemRect.height <= 0)
                return false;

            if (!MouseOn(itemRect))
            {
                if (!string.IsNullOrEmpty(currentRecieving) && name == currentRecieving)
                {
                    currentRecieving = string.Empty;
                    Repaint();
                }

                return false;
            }

            dragHandler.detectRect = itemRect;
            dragHandler.dragable = !string.IsNullOrEmpty(name);
            dragHandler.SetCustomDragData(name);

            var dragState = dragHandler.GUI_DragUpdate();
            if (dragState == DragHandler.DragState.Receiving)
            {
                currentRecieving = name;
                Repaint();
            }
            else if (dragState == DragHandler.DragState.Received)
            {
                ABInspectorDrawer.Refresh();
                currentRecieving = string.Empty;
            }
            else if (currentRecieving == name)
            {
                // Drag cursor leaved
                currentRecieving = string.Empty;
                Repaint();
            }

            return dragState == DragHandler.DragState.Received;
        }

        bool EditProcess(Rect itemRect, string name)
        {
            if (currentRenaming == name)
            {
                if (renameNeedFocus)
                {
                    // First time after reneme started. Set focuse for the text field control
                    GUI.FocusControl(KEY_RENAME_TEXTFIELDNAME);
                    renameNeedFocus = false;
                    Repaint();
                    return false;
                }

                if (revariantNeedFocus)
                {
                    // First time after revariant started. Set focuse for the text field control
                    GUI.FocusControl(KEY_REVARIANT_TEXTFIELDNAME);
                    revariantNeedFocus = false;
                    Repaint();
                    return false;
                }

                // If lose focus, end this Reneme
                bool clickOutSideTheTextField = !string.IsNullOrEmpty(currentRenaming) && Event.current.type == EventType.MouseDown && !RectClicked(itemRect);
                bool isFinishedReneme = Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return;
                if (!HasFocuse() || clickOutSideTheTextField || isFinishedReneme)
                {
                    string newBundleName = string.Empty;
                    string newVariantName = string.Empty;
                    ABData nameData = GetABDataByAssetBundleName(currentRenaming);
                    if (isRenaming)
                    {
                        if (IsValidName(editString))
                        {
                            newBundleName = editString;
                            newVariantName = nameData.variant;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog(Messages.TITLE_ERROR_RENAME
                                , editString + Messages.MESSAGE_ERROR_RENAME, Messages.BUTTON_CLOSE);
                        }
                    }
                    else if (isRevariant)
                    {
                        if (string.Empty == editString || IsValidName(editString))
                        {
                            newBundleName = nameData.name;
                            newVariantName = editString;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog(Messages.TITLE_ERROR_RENAME
                                , editString + Messages.MESSAGE_ERROR_RENAME, Messages.BUTTON_CLOSE);
                        }
                    }

                    string changedName = newBundleName + (string.Empty != newVariantName ? STRING_DOT + newVariantName : string.Empty);
                    if (changedName != currentRenaming)
                    {
                        if (RenameAssetBundle(currentRenaming, newBundleName, newVariantName, AssetDatabase.GetAssetPathsFromAssetBundle(currentRenaming)))
                        {
                            RemoveAssetBundleName(new List<string> { currentRenaming }, false);
                            return false;
                        }
                    }

                    if (Event.current.type == EventType.Layout)
                        return false;

                    editString = string.Empty;
                    currentRenaming = string.Empty;
                    isRenaming = false;
                    isRevariant = false;
                    Repaint();
                    GUIUtility.keyboardControl = 0;

                    return true;
                }
            }
            //else if (IsRectClicked(itemRect) && 0 == Event.current.button 
            //    && currentSelected == name)
            //{
            //    nextRenaming = name;
            //}

            return false;
        }

        void RightClickProcess(Rect itemRect, string name, int itemCount)
        {
            GenericMenu menu = new GenericMenu();

            string deleteTarget = name;
            if (1 == selectionList.Count)
            {
                if (0 < itemCount)
                {
                    string bundleName, variantName;
                    if (variantList.Contains(name))
                    {
                        string[] nameSplit = name.Split(CHAR_DOT);
                        bundleName = name.Replace(STRING_DOT + nameSplit[nameSplit.Length - 1], string.Empty);
                        variantName = nameSplit[nameSplit.Length - 1];
                    }
                    else
                    {
                        bundleName = name;
                        variantName = string.Empty;
                    }
                    menu.AddItem(new GUIContent(VALUE_RENAME + bundleName), false, RenameSelectedAssetBundle);

                    if (string.Empty == variantName)
                        menu.AddItem(new GUIContent(VALUE_ADDVARINT), false, RevariantSelectedAssetBundle);
                    else
                        menu.AddItem(new GUIContent(VALUE_REVARIANT + variantName), false, RevariantSelectedAssetBundle);
                }
            }
            else
                deleteTarget = string.Format(VALUE_ITEMS, selectionList.Count);

            menu.AddItem(new GUIContent(VALUE_DELETE + deleteTarget), false, DeleteSelectedAssetBundleName);
            if (MouseOn(itemRect) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                Vector2 mousePos = Event.current.mousePosition;
                menu.DropDown(new Rect(mousePos.x, mousePos.y, 0, 0));
            }
        }

        void EmptySpaceProcess(Rect spaceRect)
        {
            if (RectClicked(spaceRect) && !(ControlPressed() || Event.current.shift))
            {
                selectionList.Clear();
                currentRenaming = string.Empty;
                isRenaming = false;
                isRevariant = false;
                Repaint();
                Event.current.Use();
            }

            //DragProcess(spaceRect, string.Empty);
        }

        void ShiftSelection(string name)
        {
            if (selectionList.Count == 0)
            {
                selectionList.Add(name);
                return;
            }

            int minIndex = int.MaxValue;
            int maxIndex = int.MinValue;
            foreach (string selection in selectionList)
            {
                int selIndex = nameList.IndexOf(selection);
                if (selIndex == -1)
                    continue;

                if (minIndex > selIndex)
                    minIndex = selIndex;
                if (maxIndex < selIndex)
                    maxIndex = selIndex;
            }

            if (minIndex == int.MaxValue || maxIndex == int.MinValue)
            {
                selectionList.Add(name);
                return;
            }

            int from = 0;
            int to = nameList.IndexOf(name);
            if (to >= minIndex && to <= maxIndex)
                from = nameList.IndexOf(selectionList[0]);
            else if (to < minIndex)
                from = maxIndex;
            else if (to > maxIndex)
                from = minIndex;

            int step = to > from ? 1 : -1;
            selectionList.Clear();
            while (from != to + step)
            {
                selectionList.Add(nameList[from]);
                from += step;
            }
        }

        void UpdateScroll(float viewHeight)
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

            int selectionRow = nameList.FindIndex(x => x == newSelection);
            if (selectionRow < 0)
                return;

            float selectTopOffset = selectionRow * ITEM_HEIGHT;
            if (selectTopOffset < scrollPos.y)
                scrollPos.y = selectTopOffset;
            else if (selectTopOffset + ITEM_HEIGHT > scrollPos.y + viewHeight)
                scrollPos.y = selectTopOffset + ITEM_HEIGHT - viewHeight;

            Repaint();
        }

        #endregion

        #region WorkFunction

        bool OnCanRecieve(DragHandler.DragDatas recieverData, DragHandler.DragDatas dragData)
        {
            if (!string.IsNullOrEmpty((string)recieverData.customDragData) && 0 < dragData.dragPaths.Length)
            {
                for (int i = 0; i < dragData.dragPaths.Length; i++)
                {
                    string dragPath = dragData.dragPaths[i];

                    if (string.IsNullOrEmpty(dragPath))
                        continue;

                    return true;
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        void OnRecieve(DragHandler.DragDatas recieverData, DragHandler.DragDatas dragData)
        {
            string assetBundleName = (string)recieverData.customDragData;
            if (0 < dragData.dragPaths.Length && !string.IsNullOrEmpty(assetBundleName))
            {
                if (KEY_DRAG_HERE_ONE == assetBundleName)
                {
                    string[] split = dragData.dragPaths[0].Split('/');
                    split = split[split.Length - 1].Split(CHAR_DOT);
                    string name = prefixString + split[0].ToLower();
                    //if (SetAssetNames(name, name, string.Empty, dragData.dragPaths))
                    //    nextRenaming = name;
                    SetAssetNames(name, name, string.Empty, dragData.dragPaths);
                }
                else if (KEY_DRAG_HERE_EACH == assetBundleName)
                {
                    foreach (string path in dragData.dragPaths)
                    {
                        string[] split = path.Split('/');
                        split = split[split.Length - 1].Split(CHAR_DOT);
                        string name = prefixString + split[0].ToLower();
                        SetAssetNames(name, name, string.Empty, new string[] { path });
                    }
                }
                else//Drop already exist AssetBundleName
                {
                    ABData nameData = GetABDataByAssetBundleName(assetBundleName);
                    SetAssetNames(assetBundleName, nameData.name, nameData.variant, dragData.dragPaths);
                }
            }
        }

        bool SetAssetNames(string prevName, string newBundleName, string newVariantName, string[] pathArray)
        {
            prevName = prevName.ToLower();
            newBundleName = newBundleName.ToLower();
            newVariantName = newVariantName.ToLower();
            string newName = newBundleName + (string.Empty != newVariantName ? STRING_DOT + newVariantName : string.Empty);

            //check Asset type
            int assetType = 0;  //0 : noset, 1 : scene, 2 : assets
            List<string> pathList = new List<string>(AssetDatabase.GetAssetPathsFromAssetBundle(newName));
            if (null != pathList && 0 < pathList.Count)
            {
                if (null != pathList.Find(x => EXTENSION_SCENE == x.Substring(x.Length - 6, 6)))
                {
                    assetType = 1;
                }
                else
                {
                    assetType = 2;
                }
            }

            for (int i = 0; i < pathArray.Length; i++)
            {
                string extension = pathArray[i].Substring(pathArray[i].Length - 6, 6);
                if (EXTENSION_SCENE == extension)
                {
                    switch (assetType)
                    {
                        case 0:
                            assetType = 1;
                            break;
                        case 2:
                            EditorUtility.DisplayDialog(Messages.TITLE_ERROR, Messages.MESSAGE_ERROR_EXTENSION, Messages.BUTTON_CLOSE);
                            return false;
                    }
                }
                else
                {
                    switch (assetType)
                    {
                        case 0:
                            assetType = 2;
                            break;
                        case 1:
                            EditorUtility.DisplayDialog(Messages.TITLE_ERROR, Messages.MESSAGE_ERROR_EXTENSION, Messages.BUTTON_CLOSE);
                            return false;
                    }
                }
            }

            for (int i = 0; i < pathArray.Length; i++)
            {
                if (string.IsNullOrEmpty(pathArray[i]))
                    continue;

                ABManager.SetAssetBundleName(pathArray[i], newBundleName, newVariantName);
            }
            
            RefreshAssetDatabase();

            selectionList.Clear();
            selectionList.Add(newName);
            Repaint();

            EditorApplication.delayCall += ShowCurrentSelected;

            return true;
        }

        bool RenameAssetBundle(string prevName, string newBundleName, string newVariantName, string[] pathArray)
        {
            if (null == pathArray || 0 == pathArray.Length) return false;

            return SetAssetNames(prevName, newBundleName, newVariantName, CheckFolderHaveAssetBundleName(pathArray));
        }

        string[] CheckFolderHaveAssetBundleName(string[] pathArray)
        {
            //Check Folder have AssetBundleName
            string firstPath = pathArray[0];
            if (string.IsNullOrEmpty(AssetImporter.GetAtPath(firstPath).assetBundleName))
            {
                firstPath = firstPath.Replace('\\', '/');
                string[] pathSplit = firstPath.Split('/');

                //loop except 0 (Assets root folder)
                for (int i = pathSplit.Length - 1; i > 0; i--)
                {
                    firstPath = firstPath.Replace("/" + pathSplit[i], string.Empty);

                    if (!string.IsNullOrEmpty(AssetImporter.GetAtPath(firstPath).assetBundleName))
                        break;
                }

                return new string[] { firstPath };
            }
            else
            {
                return pathArray;
            }
        }

        void ShowCurrentSelected()
        {
            if (0 < selectionList.Count)
                ABInspectorDrawer.ShowBundle(selectionList[0]);
        }

        void RenameSelectedAssetBundle()
        {
            if (1 == selectionList.Count)
                nextRenaming = selectionList[0];

            isRenaming = true;
            isRevariant = false;
        }

        void RevariantSelectedAssetBundle()
        {
            if (1 == selectionList.Count)
                nextRenaming = selectionList[0];

            isRenaming = false;
            isRevariant = true;
        }

        void StartEditAssetBundleName()
        {
            currentRenaming = nextRenaming;

            if (isRenaming)
            {
                ABData nameData = GetABDataByAssetBundleName(currentRenaming);
                editString = nameData.name;

                renameNeedFocus = true;
                revariantNeedFocus = false;
            }
            else if  (isRevariant)
            {
                ABData nameData = GetABDataByAssetBundleName(currentRenaming);
                editString = string.Empty != nameData.variant ? nameData.variant : "newVriant";

                renameNeedFocus = false;
                revariantNeedFocus = true;
            }

            nextRenaming = string.Empty;

            Repaint();
        }

        void DeleteSelectedAssetBundleName()
        {
            if (0 >= selectionList.Count) return;

            int index = nameList.FindIndex(x => x == selectionList[0]);

            RemoveAssetBundleName(selectionList, true);

            selectionList.Clear();
            if (1 < nameList.Count)
            {
                if (index + 1 < nameList.Count)
                    index++;
                else
                    index--;

                selectionList.Add(nameList[index]);
            }
        }

        void RemoveAssetBundleName(List<string> bundleNames, bool forceRemove)
        {
            foreach (string bundle in bundleNames)
                AssetDatabase.RemoveAssetBundleName(bundle, forceRemove);

            RefreshAssetDatabase();
        }

        ABData GetABDataByAssetBundleName(string assetBundleName)
        {
            ABData data = new ABData();
            if (variantList.Contains(assetBundleName))
            {
                string[] nameSplit = assetBundleName.Split(CHAR_DOT);
                data.name = assetBundleName.Replace(STRING_DOT + nameSplit[nameSplit.Length - 1], string.Empty);
                data.variant = nameSplit[nameSplit.Length - 1];
            }
            else
            {
                data.name = assetBundleName;
                data.variant = string.Empty;
            }

            return data;
        }

        void RefreshAssetDatabase()
        {
            if (Application.isPlaying)
                return;

            AssetDatabase.Refresh();
            nameList = new List<string>(AssetDatabase.GetAllAssetBundleNames());
            countList = new List<int>();
            variantList = new List<string>();
            string[] pathArray;
            for (int i = 0; i < nameList.Count; i++)
            {
                pathArray = AssetDatabase.GetAssetPathsFromAssetBundle(nameList[i]);
                countList.Add(pathArray.Length);

                if (null != pathArray && 0 < pathArray.Length)
                {
                    pathArray = CheckFolderHaveAssetBundleName(pathArray);
                    AssetImporter ai = AssetImporter.GetAtPath(pathArray[0]);

                    if (!string.IsNullOrEmpty(ai.assetBundleVariant))
                    {
                        if (!variantList.Contains(nameList[i]))
                        {
                            variantList.Add(nameList[i]);
                        }
                    }
                }
            }

            Repaint();
        }

        #endregion

        bool HasFocuse()
        {
            return this == EditorWindow.focusedWindow;
        }

        bool IsValidName(string name)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-z0-9_#-][A-Za-z0-9_#-/]*[A-Za-z0-9_#-]$");
        }

        bool RectClicked(Rect rect)
        {
            return Event.current.type == EventType.MouseDown && MouseOn(rect);
        }

        bool ControlPressed()
        {
            return (Event.current.control && Application.platform == RuntimePlatform.WindowsEditor) ||
                (Event.current.command && Application.platform == RuntimePlatform.OSXEditor);
        }

        bool ShiftPressed()
        {
            return Event.current.shift;
        }

        bool MouseOn(Rect rect)
        {
            return rect.Contains(Event.current.mousePosition);
        }
    }
}

