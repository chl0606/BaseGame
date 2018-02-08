using UnityEngine;
using System;
using System.Collections;

public class UIUtil
{
    public static void ChangeAlpha(Transform _target, float _alpha, bool _applySprite = true, bool _applyLabel = true)
    {
        if (_applySprite)
        {
            UISprite[] sprites = _target.GetComponentsInChildren<UISprite>();

            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].alpha = _alpha;
            }
        }

        if (_applyLabel)
        {
            UILabel[] labels = _target.GetComponentsInChildren<UILabel>();

            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].alpha = _alpha;
            }
        }
    }

    public static bool Active(GameObject _go, bool _active)
    {
        if (_active != _go.activeSelf)
        {
            _go.SetActive(_active);
            return true;
        }
        return false;
    }

    public static float IntToPercent2(float min, float max, float width)
    {
        if (max == 0)
        {
            return 0;
        }
        else
            return PercentToInt((min / max) * 100, width);
    }

    public static float IntToPercent(float min, float max)
    {
        return (min / max) * 100;
    }

    public static float PercentToInt(float per, float max)
    {
        return (max * per) / 100;
    }

    public static string ChangeTimeData(string szData)
    {
        string szRet = "";
        double sec = double.Parse(szData);

        TimeSpan ts = TimeSpan.FromSeconds(sec);
        szRet = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        return szRet;
    }

    public static float getDelaySec(string _time)
    {
        DateTime _collectable = DateTime.Parse(_time);
        TimeSpan _result = _collectable - DateTime.Now;
        return Convert.ToSingle(_result.TotalSeconds);
    }

    public static float getDelaySec(DateTime _time)
    {
        DateTime _collectable = _time;
        TimeSpan _result = _collectable - DateTime.Now;
        return Convert.ToSingle(_result.TotalSeconds);
    }

    public static string setCommaCount(int _num)
    {
        return _num.ToString("#,##0");
    }
}
