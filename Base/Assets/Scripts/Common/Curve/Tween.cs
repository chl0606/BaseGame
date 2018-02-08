using UnityEngine;
using System.Collections;

public class Tween 
{
    public int totalTick;
    public int tick;
    public float[] x;
    public float[] y;
    public float[] z;
    public float[] rotatex;
    public float[] rotatey;
    public float[] rotatez;
    //public float[] lookx;
    //public float[] looky;
    //public float[] lookz;
    public float[] fov;


    public Tween(int _totalTick)
    {
        totalTick = _totalTick;
        tick = 0;
    }
}
