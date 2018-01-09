using UnityEngine;

/// <summary>
/// Defines 1 Animation
/// </summary>
[System.Serializable]
public class AnimCombo
{
    public string animName;
    [Range(0f, 1f)] public float linkBegin, linkEnd;

    //public int animHash;
}
