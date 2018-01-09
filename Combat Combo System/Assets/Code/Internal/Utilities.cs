using UnityEngine;

public static class Utilities
{
    /// <summary>
    /// returns min < value < max
    /// </summary>
    /// <param name="x"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool InRange(float x, float min, float max)
    {
        return x >= min && x <= max;
    }
}
