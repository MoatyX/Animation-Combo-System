using UnityEngine;

namespace Generics.Utilities
{
    /// <summary>
    /// Defines 1 single animation in the 1 single Link of the chain
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Attack", menuName = "Animation Combo System/New Attack")]
    public class AttackAnim : ScriptableObject
    {
        //Note: the class is still under-construction for future updates

        public string AnimName;

        [Header("Linking")]
        [Range(0f, 1f)] public float LinkBegin;
        [Range(0f, 1f)] public float LinkEnd;
        public float TransitionDuration = 0.1f;

        [Header("Damage")]
        [Range(0f, 1f), Tooltip("a point where a hit-scan message will be sent")] public float[] ScanPoints;
        public float Damage;


        public int AnimHash { get; private set; }

        private void OnEnable()
        {
            AnimHash = Animator.StringToHash(AnimName);
            TransitionDuration = Mathf.Abs(TransitionDuration);
        }


    }

    //coming soon
    //[System.Serializable]
    //public class LinkAnimVarients
    //{
    //    public string animName;
    //    [Range(0f, 1f)] public float frequency;
    //}
}