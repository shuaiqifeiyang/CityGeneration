using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static Vector3Int Vector3toVector3Int(Vector3 v)
    {
        Vector3Int res = new Vector3Int(
            (int)System.Math.Round(v[0]),
            (int)System.Math.Round(v[1]),
            (int)System.Math.Round(v[2])
        );
        return res;
    }
}
