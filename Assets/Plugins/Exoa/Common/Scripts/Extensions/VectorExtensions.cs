using UnityEngine;

public static class VectorExtensions
{
    public static string Dump(this Vector3 v)
    {
        return "x:" + v.x + " y:" + v.y + " z:" + v.z;
    }

    public static Vector3 SetY(this Vector3 v, float y)
    {
        Vector3 newv = v;
        newv.y = y;
        return newv;
    }

    public static Vector3 SetZ(this Vector3 v, float z)
    {
        Vector3 newv = v;
        newv.z = z;
        return newv;
    }


}
