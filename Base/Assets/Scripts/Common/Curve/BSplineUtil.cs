using UnityEngine;
using System.Collections;

public class BSplineUtil 
{
    public static void setArrayCurve(float[] tween, float[] point, float scale,
            int startTick, int endTick)
	{
		int i;
		
		if (point.Length < 2)
		{
			Debug.LogWarning("setArrayCurve point length Should not be less than 2.");
		}
		BSpline bsp;
		
		// If an awning piece Wise Curve
		switch (point.Length)
		{
			default:
				bsp = new BSpline(2, 3, point.Length, BSpline.BSP_CLAMPED);
				bsp.SetControlPointArray(0, point);
				float[] pointTemp = new float[point.Length];
				bsp.SetControlPointArray(1, pointTemp);
				break;
			case 1:
			case 2:
				bsp = new BSpline(1, 2, point.Length, BSpline.BSP_CLAMPED);
				bsp.SetControlPointArray(0, point);
				break;
		}
		
		float[] outPoint = new float[3];
		float time;
		
		for (i = startTick; i < endTick; i++)
		{
			time = (float) (i - startTick) / (float) (endTick - 1 - startTick);
			bsp.Evaluate(time, outPoint);
			float f = outPoint[0] * scale;
			tween[i] = f;
			// Log.d("TAG", "setArrayCurve i = " + i + " : time = " + time +
			// " : tween = " + tween[i]);
		}
	}

    /**
     * Create a function to initialize the array
     * 
     * @param value
     * @return
     */
    private static float[] initValue(float[] value, int totalTick)
    {
        if (value != null)
            return value;
        value = new float[totalTick];
        for (int i = 0; i < totalTick; i++)
            value[i] = 0.0f;
        return value;
    }

    public static void setSplineCurveX(Tween tween, float[] point, float scale,
            int startTick, int endTick)
    {
        tween.x = initValue(tween.x, tween.totalTick);
        setArrayCurve(tween.x, point, scale, startTick, endTick);
    }

    public static void setSplineCurveY(Tween tween, float[] point, float scale,
            int startTick, int endTick)
    {
        tween.y = initValue(tween.y, tween.totalTick);
        setArrayCurve(tween.y, point, scale, startTick, endTick);
    }

    public static void setSplineCurveZ(Tween tween, float[] point, float scale,
            int startTick, int endTick)
    {
        tween.z = initValue(tween.z, tween.totalTick);
        setArrayCurve(tween.z, point, scale, startTick, endTick);
    }

    //public static void setSplineCurveScaleX(Tween tween, float[] point,
    //        float scale, int startTick, int endTick)
    //{
    //    tween.scalex = initValue(tween.scalex, tween.totalTick);
    //    setArrayCurve(tween.scalex, point, scale, startTick, endTick);
    //}

    //public static void setSplineCurveScaleY(Tween tween, float[] point,
    //        float scale, int startTick, int endTick)
    //{
    //    tween.scaley = initValue(tween.scaley, tween.totalTick);
    //    setArrayCurve(tween.scaley, point, scale, startTick, endTick);
    //}

    //public static void setSplineCurveScaleZ(Tween tween, float[] point,
    //        float scale, int startTick, int endTick)
    //{
    //    tween.scalez = initValue(tween.scalez, tween.totalTick);
    //    setArrayCurve(tween.scalez, point, scale, startTick, endTick);
    //}

    public static void setSplineCurveRotateX(Tween tween, float[] point,
            float scale, int startTick, int endTick)
    {
        tween.rotatex = initValue(tween.rotatex, tween.totalTick);
        setArrayCurve(tween.rotatex, point, scale, startTick, endTick);
    }

    public static void setSplineCurveRotateY(Tween tween, float[] point,
            float scale, int startTick, int endTick)
    {
        tween.rotatey = initValue(tween.rotatey, tween.totalTick);
        setArrayCurve(tween.rotatey, point, scale, startTick, endTick);
    }

    public static void setSplineCurveRotateZ(Tween tween, float[] point,
            float scale, int startTick, int endTick)
    {
        tween.rotatez = initValue(tween.rotatez, tween.totalTick);
        setArrayCurve(tween.rotatez, point, scale, startTick, endTick);
    }

    public static void setSplineCurveFOV(Tween tween, float[] point,
            float scale, int startTick, int endTick)
    {
        tween.fov = initValue(tween.fov, tween.totalTick);
        setArrayCurve(tween.fov, point, scale, startTick, endTick);
    }

    public static void copyTween(Tween tweenSrc, Tween tweenTgt, int st, int ed)
    {
        int i;
        if (tweenSrc.x != null)
        {
            tweenTgt.x = initValue(tweenTgt.x, tweenSrc.totalTick);
            for (i = st; i < ed; i++)
            {
                tweenTgt.x[i] = tweenSrc.x[i];
            }
        }

        if (tweenSrc.y != null)
        {
            tweenTgt.y = initValue(tweenTgt.y, tweenSrc.totalTick);
            for (i = st; i < ed; i++)
            {
                tweenTgt.y[i] = tweenSrc.y[i];
            }
        }

        if (tweenSrc.z != null)
        {
            tweenTgt.z = initValue(tweenTgt.z, tweenSrc.totalTick);
            for (i = st; i < ed; i++)
            {
                tweenTgt.z[i] = tweenSrc.z[i];
            }
        }

        //if (tweenSrc.scalex != null)
        //{
        //    tweenTgt.scalex = initValue(tweenTgt.scalex, tweenSrc.totalTick);
        //    for (i = st; i < ed; i++)
        //    {
        //        tweenTgt.scalex[i] = tweenSrc.scalex[i];
        //    }
        //}

        //if (tweenSrc.scaley != null)
        //{
        //    tweenTgt.scaley = initValue(tweenTgt.scaley, tweenSrc.totalTick);
        //    for (i = st; i < ed; i++)
        //    {
        //        tweenTgt.scaley[i] = tweenSrc.scaley[i];
        //    }
        //}

        //if (tweenSrc.scalez != null)
        //{
        //    tweenTgt.scalez = initValue(tweenTgt.scalez, tweenSrc.totalTick);
        //    for (i = st; i < ed; i++)
        //    {
        //        tweenTgt.scalez[i] = tweenSrc.scalez[i];
        //    }
        //}

        if (tweenSrc.rotatex != null)
        {
            tweenTgt.rotatex = initValue(tweenTgt.rotatex, tweenSrc.totalTick);
            for (i = st; i < ed; i++)
            {
                tweenTgt.rotatex[i] = tweenSrc.rotatex[i];
            }
        }

        if (tweenSrc.rotatey != null)
        {
            tweenTgt.rotatey = initValue(tweenTgt.rotatey, tweenSrc.totalTick);
            for (i = st; i < ed; i++)
            {
                tweenTgt.rotatey[i] = tweenSrc.rotatey[i];
            }
        }

        if (tweenSrc.rotatez != null)
        {
            tweenTgt.rotatez = initValue(tweenTgt.rotatez, tweenSrc.totalTick);
            for (i = st; i < ed; i++)
            {
                tweenTgt.rotatez[i] = tweenSrc.rotatez[i];
            }
        }

        if (tweenSrc.fov != null)
        {
            tweenTgt.fov = initValue(tweenTgt.fov, tweenSrc.totalTick);
            for (i = st; i < ed; i++)
            {
                tweenTgt.fov[i] = tweenSrc.fov[i];
            }
        }
    }
}
