using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

internal class ABStyleUtil
{
    public enum Type
    {
        LabelNormal,
        LabelEmpty,
        LabelRecieving,
        LabelSelected,
        LabelRenaming,
        LabelSpaceNormal,
        LabelSpaceRecieving,
    }

    private Dictionary<Type, GUIStyle> dicStyle = new Dictionary<Type, GUIStyle>();

    private static ABStyleUtil instance = null;
    private static ABStyleUtil getInstance()
    {
        if (null == instance)
        {
            instance = new ABStyleUtil();
            instance.SetStyles();
        }

        return instance;
    }

    public static GUIStyle GetStyle(Type type)
    {
        if (!getInstance().dicStyle.ContainsKey(type))
        {
            getInstance().dicStyle.Add(type, new GUIStyle());
        }
        return getInstance().dicStyle[type];
    }

    private void SetStyles()
    {
        //foreach (GUIStyle ss in GUI.skin.customStyles)
        //{
        //    if (ss.name == "PR Label")
        //        dicStyle.Add(Type.LabelNormal, ss);
        //    else if (ss.name == "PR PrefabLabel")
        //        dicStyle.Add(Type.LabelSelected, ss);
        //}

        SetStylesSafety();
    }

    private void SetStylesSafety()
    {
        GUIStyle style;

        if (!dicStyle.ContainsKey(Type.LabelNormal))
        {
            style = new GUIStyle();
            style.padding = new RectOffset(6, 6, 2, 2);
            style.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            dicStyle.Add(Type.LabelNormal, style);
        }

        if (!dicStyle.ContainsKey(Type.LabelEmpty))
        {
            style = new GUIStyle();
            style.padding = new RectOffset(6, 6, 2, 2);
            style.normal.textColor = new Color(1.0f, 0.5f, 0.5f, 1f);
            dicStyle.Add(Type.LabelEmpty, style);
        }

        if (!dicStyle.ContainsKey(Type.LabelRecieving))
        {
            style = new GUIStyle();
            style.padding = new RectOffset(6, 6, 2, 2);
            style.normal.textColor = new Color(0.5f, 1.0f, 0.5f, 1f);
            dicStyle.Add(Type.LabelRecieving, style);
        }

        if (!dicStyle.ContainsKey(Type.LabelSelected))
        {
            style = new GUIStyle();
            style.padding = new RectOffset(6, 6, 2, 2);
            style.normal.textColor = new Color(0.5f, 0.5f, 1.0f, 1f);
            dicStyle.Add(Type.LabelSelected, style);
        }

        if (!dicStyle.ContainsKey(Type.LabelRenaming))
        {
            style = new GUIStyle();
            style.padding = new RectOffset(6, 6, 2, 2);
            style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1f);
            dicStyle.Add(Type.LabelRenaming, style);
        }

        if (!dicStyle.ContainsKey(Type.LabelSpaceNormal))
        {
            style = new GUIStyle();
            style.fontSize = 13;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            style.alignment = TextAnchor.MiddleCenter;
            style.fixedHeight = 60;
            dicStyle.Add(Type.LabelSpaceNormal, style);
        }

        if (!dicStyle.ContainsKey(Type.LabelSpaceRecieving))
        {
            style = new GUIStyle();
            style.fontSize = 13;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = new Color(0.5f, 1.0f, 0.5f, 1f);
            style.alignment = TextAnchor.MiddleCenter;
            style.fixedHeight = 60;
            dicStyle.Add(Type.LabelSpaceRecieving, style);
        }
    }
}
