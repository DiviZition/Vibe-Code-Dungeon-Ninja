using UnityEngine;

public static class VectorExtentions
{
    public static Vector3 ResetY(this Vector3 vector3, float newValue = 0)
    {
        vector3.y = newValue;
        return vector3;
    }

    public static Vector3 ResetZ(this Vector3 vector3, float newValue = 0)
    {
        vector3.z = newValue;
        return vector3;
    }

    public static Vector3 ResetX(this Vector3 vector3, float newValue = 0)
    {
        vector3.x = newValue;
        return vector3;
    }

    public static Vector3 ToVector3XZ(this Vector2 vector2) => new Vector3(vector2.x, 0, vector2.y);
    public static Vector3 ToVector3XY(this Vector2 vector2) => new Vector3(vector2.x, vector2.y, 0);
}
