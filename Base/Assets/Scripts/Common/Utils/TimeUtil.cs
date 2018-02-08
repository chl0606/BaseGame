using UnityEngine;
using System;
using System.Collections;

public class TimeUtil 
{
    public static int tz_offset_sec = 0;
    public static int TIME_GAP = 0; //server와 device의 시간차. 단위는 초를 의미
    private static string DAY = "D";
    private static string HOUR = "H";
    private static string MINUTE = "M";
    private static string SECONDS = "S";

    public static string ConvertTimeFormat(int _sec)
    {
        //if ("" == DAY)
        //{
        //    LanguageLoader lang = LanguageLoader.Instance;
        //    DAY = lang.GetString(121) + " ";
        //    HOUR = lang.GetString(122) + " ";
        //    MINUTE = lang.GetString(123) + " ";
        //    SECONDS = lang.GetString(124);
        //}
        string s = Math.Abs(_sec % 60) + SECONDS;
        int min = Math.Abs(_sec / 60);
        string m = min % 60 + MINUTE;
        int hour = min / 60;
        string h = Math.Abs(hour % 24) + HOUR;
        string d = Math.Abs(hour / 24) + DAY;

        if ((hour / 24) > 0)
        {
            m = "";
            s = "";
        }
        else
        {
            d = "";

            if (hour > 0)
            {
                s = "";
            }
            else
            {
                h = "";

                if (min <= 0)
                {
                    m = "";
                }
            }
        }

        return (_sec < 0 ? "-" : "") + d + h + m + s;
    }

    public static int DateTimeToSec(DateTime _time)
    {
        if (DateTime.MinValue == _time)
        {
            return 0;
        }
        else
        {
            return (int)(_time.Ticks / 10000000); //sec단위로 만들기위해 10000000으로 나눔
        }
    }

    public static int SecToDay(int _sec)
    {
        return (int)Mathf.Ceil((float)_sec / 86400f);
    }

    public static DateTime ToUtc(DateTime _time)
    {
        if (DateTime.MinValue == _time)
        {
            return DateTime.MinValue;
        }
        else
        {
            DateTime t = new DateTime(_time.Ticks);
            t = t.AddSeconds(-tz_offset_sec - TIME_GAP);

            //Debug.Log(_time + "   " + t + "   " + _time.ToUniversalTime());
            return t;
        }
    }

    public static string ActiveTime(int _sec)
    {
        int min = Math.Abs(_sec / 60);

        string m = (min % 60).ToString();

        for (int i = 0; i < 2 - m.Length; i++)
        {
            m = m.Insert(0, "0");
        }
        m += " : ";


        string s = Math.Abs(_sec % 60).ToString();

        for (int i = 0; i < 2 - s.Length; i++)
        {
            s = s.Insert(0, "0");
        }

        return (_sec < 0 ? "-" : "") + m + s;
    }

    private static long TICK_OFFSET = 0;

    public static void SetTickOffset(long _tickOffset)
    {
        TICK_OFFSET = _tickOffset;
    }

    public static long GetTickNow()
    {
        return DateTime.UtcNow.Ticks - TICK_OFFSET;
    }
}
