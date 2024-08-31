using UnityEngine;
using static Direction;

public static class VectorWith
{
    public static Vector2Int Go(this Vector2Int vec, Direction dir) {
        if (dir is Up) {
            return new(vec.x, vec.y + 1);
        } else if (dir is Down) {
            return new(vec.x, vec.y - 1);
        } else if (dir is Left) {
            return new(vec.x - 1, vec.y);
        } else if (dir is Right) {
            return new(vec.x + 1, vec.y);
        }
        throw new();
    }

    public static bool IsValid(this Vector2Int vec) {
        if (vec.x > 8 || vec.x < 0)
            return false;
        if (vec.y > 8 || vec.y < 0)
            return false;
        return true;
    }
}

