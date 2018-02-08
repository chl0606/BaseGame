public enum eSceneType
{
    None = 0,
    Download = 1,
    Title = 2,
    Town = 3,
}

public enum eLanguage
{
    KOR = 0,
    JPN = 1,
    INT = 2,
}

public enum eUIDepth
{
    Main = 0,

    Blocker = 5000,
    Toast = 5100,
}

public class UIDepthUtil
{
    public static int GetDepth(eUIDepth uiDepth)
    {
        return (int)uiDepth;
    }
}

public enum eBlockerType
{
    Black = 0,
    Circle = 1,
}
