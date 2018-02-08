using UnityEngine;
using System.IO;
using System.Collections;

public class Rotation : MonoBehaviour
{
    public Vector3 rotatePerFrame = Vector3.zero;
    void Update()
    {
        transform.Rotate(rotatePerFrame);
    }
}
