using System;
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
        [Range(0f, 1f)] public float LinkBegin = 0.2f;
        [Range(0f, 1f)] public float LinkEnd = 1f;
        public float TransitionDuration = 0.1f;

        [Header("Damage")]
        [Range(0f, 1f), Tooltip("a point where a hit-scan message will be sent")] public float[] ScanPoints;
        public float Damage;


        protected internal int AnimHash { get; private set; }
        //protected internal bool HasFinishedPlaying { get; set; }
        protected internal bool HasStarted { get; set; }

        private void OnEnable()
        {
            AnimHash = Animator.StringToHash(AnimName);
            TransitionDuration = Mathf.Abs(TransitionDuration);
            //HasFinishedPlaying = false;
            HasStarted = false;

            if (Application.isPlaying)
            {
                Dispatcher.AttackTriggered += OnAttackTriggered;
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                Dispatcher.AttackTriggered -= OnAttackTriggered;
            }
        }

        /// <summary>
        /// Do stuff when the attack is triggered
        /// </summary>
        /// <param name="attack"></param>
        private void OnAttackTriggered(AttackAnim attack)
        {
            if(attack != this) return;

            HasStarted = true;
        }

        /// <summary>
        /// reset the anim
        /// </summary>
        /// <returns></returns>
        protected internal bool Reset()
        {
            //HasFinishedPlaying = false;
            HasStarted = false;
            return true;
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