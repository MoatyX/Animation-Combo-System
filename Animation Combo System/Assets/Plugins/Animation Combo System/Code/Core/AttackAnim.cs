using System;
using UnityEngine;

namespace Generics.Utilities
{
    /// <summary>
    /// Defines 1 single animation in the 1 single Link of the chain
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Attack", menuName = "Animation Combo System/New Attack")]
    public class AttackAnim : ScriptableObject
    {
        //Note: the class is still under-construction for future updates

        /// <summary>
        /// Simple struct that contains data about the damage
        /// </summary>
        [Serializable]
        public struct DamageData
        {
            [Range(0f, 1f), Tooltip("a point where a hit-scan event will be triggered")]
            public float TriggerTime;

            public float Damage;

            internal bool Triggered { get; private set; }

            /// <summary>
            /// Trigger a damage data
            /// </summary>
            /// <param name="attack"></param>
            /// <param name="index"></param>
            /// <param name="callback"></param>
            internal void Trigger(AttackAnim attack, uint index, Action callback)
            {
                if (Triggered) return;

                Dispatcher.OnHitScanning(attack, index, Damage);
                Triggered = true;

                if(callback != null)
                callback.Invoke();
            }

            internal void Reset()
            {
                Triggered = false;
            }
        }

        /// <summary>
        /// A simple struct that contains data about a generic event
        /// </summary>
        [Serializable]
        public struct GenericEvent
        {
            [Tooltip("A descriptive name to the event (has no influence in the system)")]
            public string Name;

            [Range(0f, 1f), Tooltip("a point where the event will be triggered")]
            public float TriggerTime;

            [Tooltip("An argument that will passed to the listening function")]
            public string Key;

            internal bool Triggered { get; private set; }

            /// <summary>
            /// Trigger a generic event
            /// </summary>
            /// <param name="attack"></param>
            /// <param name="index"></param>
            /// <param name="callback"></param>
            internal void Trigger(AttackAnim attack, uint index, Action callback)
            {
                if (Triggered) return;

                Dispatcher.OnGenericEvent(attack, index, Key);
                Triggered = true;

                if(callback != null)
                callback.Invoke();
            }

            internal void Reset()
            {
                Triggered = false;
            }
        }

        public string AnimName;

        [Header("Linking")] [Range(0f, 1f)] public float LinkBegin = 0.2f;
        [Range(0f, 1f)] public float LinkEnd = 1f;
        public float TransitionDuration = 0.1f;

        [Header("Events")] public DamageData[] DamageEvents;
        public GenericEvent[] GenericEvents;


        protected internal int AnimHash { get; private set; }
        protected internal bool HasStarted { get; private set; }

        private uint _genericEventIndex;
        private uint _hitScanIndex;

        private Action _genericEventCallback;
        private Action _damageEventCallback;

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(AnimName) && Application.isPlaying)
            {
                Debug.LogWarning(string.Format("No Animation Name was set in the Attack: {0}", name));
            }

            AnimHash = Animator.StringToHash(AnimName);
            TransitionDuration = Mathf.Abs(TransitionDuration);
            Reset();


            Dispatcher.AttackTriggered += OnAttackTriggered;

            _genericEventCallback = () => _genericEventIndex++;
            _damageEventCallback = () => _hitScanIndex++;
        }

        private void OnDisable()
        {
            Dispatcher.AttackTriggered -= OnAttackTriggered;
            _genericEventCallback = null;
            _damageEventCallback = null;

            Reset();
        }

        /// <summary>
        /// Trigger events
        /// </summary>
        /// <param name="currentNormalisedTime"></param>
        protected internal void TriggerEvents(float currentNormalisedTime)
        {
            for (var i = 0; i < DamageEvents.Length; i++)
                if (Utilities.InRange(currentNormalisedTime, DamageEvents[i].TriggerTime,
                    DamageEvents[i].TriggerTime + Time.deltaTime))
                    TriggerHitScans(i);

            for (var i = 0; i < GenericEvents.Length; i++)
                if (Utilities.InRange(currentNormalisedTime, GenericEvents[i].TriggerTime - Time.deltaTime,
                    GenericEvents[i].TriggerTime))
                    TriggerGenericEvents(i);
        }

        /// <summary>
        /// Trigger hitscans functions
        /// </summary>
        private void TriggerHitScans(int i)
        {
            DamageEvents[i].Trigger(this, _hitScanIndex, _damageEventCallback);
        }

        /// <summary>
        /// Trigger generic events
        /// </summary>
        private void TriggerGenericEvents(int i)
        {
            GenericEvents[i].Trigger(this, _genericEventIndex, _genericEventCallback);
        }

        /// <summary>
        /// Do stuff when the attack is triggered
        /// </summary>
        /// <param name="attack"></param>
        private void OnAttackTriggered(AttackAnim attack)
        {
            if (attack != this) return;

            HasStarted = true;
        }

        /// <summary>
        /// reset the anim
        /// </summary>
        /// <returns></returns>
        protected internal bool Reset()
        {
            HasStarted = false;
            _hitScanIndex = 0;
            _genericEventIndex = 0;

            if (DamageEvents != null)
                for (int i = 0; i < DamageEvents.Length; i++)
                {
                    DamageEvents[i].Reset();
                }

            if (GenericEvents != null)
                for (int i = 0; i < GenericEvents.Length; i++)
                {
                    GenericEvents[i].Reset();
                }

            return true;
        }
    }
}