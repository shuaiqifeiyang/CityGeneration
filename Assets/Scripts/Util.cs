using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyUtil
{
    public static bool VectorEqual(Vector3 a, Vector3 b)
    {
        return (
            System.Math.Abs(a.x - b.x) < 0.000001 &&
            System.Math.Abs(a.y - b.y) < 0.000001 &&
            System.Math.Abs(a.z - b.z) < 0.000001
            );
    }
}
