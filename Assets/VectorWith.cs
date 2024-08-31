using UnityEngine;

public static class VectorWith
{
    public static Vector2 BoardPos(this Transform t) {
        var vec = new Vector2(t.position.x, t.position.z);
        return vec;
    }
}

