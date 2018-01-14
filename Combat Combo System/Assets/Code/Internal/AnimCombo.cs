using UnityEngine;

/// <summary>
/// Defines 1 Animation
/// </summary>
[System.Serializable]
public class AnimCombo
{
    public string animName;
    [Range(0f, 1f)] public float linkBegin, linkEnd;
    public float transitionDuration = 0.1f;

    public int animHash { get; private set; }

    public bool Setup()
    {
        animHash = Animator.StringToHash(animName);
        transitionDuration = Mathf.Abs(transitionDuration);
        return true;
    }
}
