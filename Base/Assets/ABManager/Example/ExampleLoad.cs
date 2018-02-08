using UnityEngine;
using System.IO;
using System.Collections;

public class ExampleLoad : MonoBehaviour
{
    private bool useServerBundle = false;

    void Awake()
    {
        gameObject.AddComponent<ABDownloader>();
        ABDownloader.Instance.rootUrl
            = Path.Combine(useServerBundle ? "http://yourUrl" : "file://" + Application.dataPath.Replace("Assets", ABUtil.PATH_OUTPUT), ABUtil.GetPlatformName());
        Debug.Log("rootUrl : " + ABDownloader.Instance.rootUrl);

        CachingPreference();
    }

    void CachingPreference()
    {
        Caching.compressionEnabled = true;

#if UNITY_EDITOR
        //Caching space 4GB
        Caching.maximumAvailableDiskSpace = 1073700000 * (long)4;
#else
        //Caching space 1GB
        Caching.maximumAvailableDiskSpace = 1073700000;
#endif

        Debug.Log("Caching.maximumAvailableDiskSpace : " + Caching.maximumAvailableDiskSpace);
        Debug.Log("Caching.spaceOccupied : " + Caching.spaceOccupied);
        Debug.Log("Caching.spaceFree : " + Caching.spaceFree);
    }

    #region GUI

    protected GUIStyle labelStyle;
    protected string text = string.Empty;

    protected virtual void OnGUI()
     {
        if (null == labelStyle)
        {
            labelStyle = new GUIStyle();
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 20;
            labelStyle.richText = true;
        }
        DrawOutline(new Rect(0, 0, Screen.width, Screen.height), text, labelStyle, Color.black);
     }

    protected void DrawOutline(Rect position, string text, GUIStyle style, Color outColor)
     {
         GUIStyle backupStyle = style;
         Color oldColor = style.normal.textColor;
         style.normal.textColor = outColor;
         position.x--;
         GUI.Label(position, text, style);
         position.x += 2;
         GUI.Label(position, text, style);
         position.x--;
         position.y--;
         GUI.Label(position, text, style);
         position.y += 2;
         GUI.Label(position, text, style);
         position.y--;
         style.normal.textColor = oldColor;
         GUI.Label(position, text, style);
         style = backupStyle;
     }

    #endregion
}
