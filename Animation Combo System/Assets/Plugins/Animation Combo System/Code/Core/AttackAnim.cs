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

        /// <summary>
        /// Simple struct that contains data about the damage
        /// </summary>
        [System.Serializable]
        public struct DamageData
        {
            [Range(0f, 1f), Tooltip("a point where a hit-scan event will be triggered")]
            public float TriggerTime;

            public float Damage;
        }

        /// <summary>
        /// A simple struct that contains data about a generic event
        /// </summary>
        [System.Serializable]
        public struct GenericEvent
        {
            [Tooltip("A descriptive name to the event (has no influence in the system)")]
            public string Name;

            [Range(0f, 1f), Tooltip("a point where the event will be triggered")]
            public float TriggerTime;

            [Tooltip("An argument that will passed to the listening function")]
            public string Key;
        }

        public string AnimName;

        [Header("Linking")]
        [Range(0f, 1f)] public float LinkBegin = 0.2f;
        [Range(0f, 1f)] public float LinkEnd = 1f;
        public float TransitionDuration = 0.1f;

        [Header("Events")]
        public DamageData[] DamageEvents;
        public GenericEvent[] GenericEvents;


        protected internal int AnimHash { get; private set; }
        //protected internal bool HasFinishedPlaying { get; set; }
        protected internal bool HasStarted { get; set; }

        private uint _genericEventIndex;
        private uint _hitScanIndex;

        private void OnEnable()
        {
            AnimHash = Animator.StringToHash(AnimName);
            TransitionDuration = Mathf.Abs(TransitionDuration);
            //HasFinishedPlaying = false;
            Reset();

            if (Application.isPlaying)
            {
                Dispatcher.AttackTriggered += OnAttackTriggered;
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;

            Dispatcher.AttackTriggered -= OnAttackTriggered;
            Reset();
        }

        /// <summary>
        /// Trigger events
        /// </summary>
        /// <param name="currentNormalisedTime"></param>
        protected internal void TriggerEvents(float currentNormalisedTime)
        {
            for (var i = 0; i < DamageEvents.Length; i++)
                if (Utilities.InRange(currentNormalisedTime, DamageEvents[i].TriggerTime, DamageEvents[i].TriggerTime + Time.deltaTime))
                TriggerHitScans(i);

            for (var i = 0; i < GenericEvents.Length; i++)
                if (Utilities.InRange(currentNormalisedTime, GenericEvents[i].TriggerTime, GenericEvents[i].TriggerTime + Time.deltaTime))
                TriggerGenericEvents(i);
        }

        /// <summary>
        /// Trigger hitscans functions
        /// </summary>
        private void TriggerHitScans(int i)
        {
            Dispatcher.OnHitScanning(this, _hitScanIndex, DamageEvents[i].Damage);
            _hitScanIndex++;
        }

        /// <summary>
        /// Trigger generic events
        /// </summary>
        private void TriggerGenericEvents(int i)
        {
            Dispatcher.OnGenericEvent(this, _genericEventIndex, GenericEvents[i].Key);
            _genericEventIndex++;
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
            _hitScanIndex = 0;
            _genericEventIndex = 0;
            return true;
        }
    }
}