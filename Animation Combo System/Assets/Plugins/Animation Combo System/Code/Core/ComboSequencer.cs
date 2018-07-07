using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Generics.Utilities
{
    /// <summary>
    /// the Sequencer Class
    /// </summary>
    [System.Serializable]
    public class ComboSequencer
    {
        public List<Combo> Combos;

        public Animator Animator;
        private bool _initiated;

        #region Public Properties and API
        public Combo ActiveCombo { get; private set; }

        /// <summary>
        /// Setup and prepare
        /// </summary>
        public bool Initialise()
        {
            var all = Combos.All(x => x.Initialise(this));
            _initiated = all;
            return _initiated;
        }

        /// <summary>
        /// TriggerEvents all the combos
        /// </summary>
        public void Update()
        {
            if (!_initiated)
            {
                Debug.LogError(
                    "The Sequencer was not initialised probably, either duo to NullReferances/invalid-input in the inspector or not Calling the Initialise() function");
                return;
            }

            for (var i = 0; i < Combos.Count; i++)
            {
                if(Combos[i] == null) continue;
                if (ActiveCombo != null && ActiveCombo != Combos[i]) continue;

                Combos[i].Update();
            }

            //IsCurrentlyActive(null);
        }

        /// <summary>
        /// true if the ComboSequencer is currently executing a combo
        /// </summary>
        /// <returns>the current state of the Sequencer</returns>
        public bool IsExecuting()
        {
            return ActiveCombo != null;
        }

        #endregion

        #region Internal Calls
        /// <summary>
        /// Call the next animation
        /// </summary>
        protected internal void NextAnimation(AttackAnim currentAnim, int targetLayer)
        {
            if (currentAnim != null)
            {
                if (IsCurrentlyActive(currentAnim, targetLayer) == false)
                {
                    Animator.CrossFadeInFixedTime(currentAnim.AnimName, currentAnim.TransitionDuration, targetLayer);
                }
            }
            else
            {
                Debug.LogError("The current Anim is null");
            }
        }

        /// <summary>
        /// Check if a current animation in the link is active under its conditions
        /// </summary>
        /// <param name="currentAnim"></param>
        /// <param name="targetLayer">the layer which the anim is on</param>
        /// <returns></returns>
        protected internal bool IsCurrentlyActive(AttackAnim currentAnim, int targetLayer)
        {
            if (currentAnim == null) return false;

            var nameCond = Animator.GetCurrentAnimatorStateInfo(targetLayer).shortNameHash == currentAnim.AnimHash;

            return nameCond && currentAnim.HasStarted;
        }

        /// <summary>
        /// Trigger Events on special-predefined time stamps in the Animation's timeline
        /// </summary>
        /// <param name="attk"></param>
        /// <param name="layer"></param>
        protected internal void ScanTimeline(AttackAnim attk, int layer)
        {
            //for now the special timestamps are defined in the inspector

            if (attk == null) return;

            var normTime = Mathf.Clamp01(Animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
            attk.TriggerEvents(normTime);
        }

        /// <summary>
        /// Check if the attack anim has crossed the LinkBegin time stamp
        /// </summary>
        /// <param name="attk"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        protected internal bool IsBeginningLink(AttackAnim attk, int layer)
        {
            if (attk == null) return false;

            var nameCondition = Animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash; //name match
            var normTime = nameCondition ? Mathf.Clamp01(Animator.GetCurrentAnimatorStateInfo(layer).normalizedTime) : 0;
            var timeCondition = Utilities.InRange(normTime, attk.LinkBegin, 1f + Time.deltaTime);

            return nameCondition && timeCondition;
        }

        /// <summary>
        /// check if an anim has crossed the LinkEnd time stamp
        /// </summary>
        /// <param name="attk"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        protected internal bool IsEndingLink(AttackAnim attk, int layer)
        {
            if (attk == null) return false;

            var nameCondition = Animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash; //name match
            var normTime = nameCondition ? Mathf.Clamp01(Animator.GetCurrentAnimatorStateInfo(layer).normalizedTime) : 0;
            var timeCondition = Utilities.InRange(normTime, attk.LinkEnd - Time.deltaTime, 1f); //Range match

            return nameCondition && timeCondition;
        }

        /// <summary>
        /// Check if the anim is within its LinkBegin and LinkEnd
        /// </summary>
        /// <param name="attk"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        protected internal bool WithinLink(AttackAnim attk, int layer)
        {
            if (attk == null) return false;

            var nameCondition = Animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash; //name match
            var normTime = nameCondition ? Mathf.Clamp01(Animator.GetCurrentAnimatorStateInfo(layer).normalizedTime) : 0;
            var timeCondition = Utilities.InRange(normTime, attk.LinkBegin, attk.LinkEnd);

            return nameCondition && timeCondition;
        }

        /// <summary>
        /// check if a anim has crossed the LinkEnd time stamp AND has been fully played
        /// </summary>
        /// <param name="attk"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        protected internal bool IsExisting(AttackAnim attk, int layer)
        {
            if (attk == null) return false;

            var nameCondition = Animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash; //name match
            var normTime = nameCondition ? Mathf.Clamp01(Animator.GetCurrentAnimatorStateInfo(layer).normalizedTime) : 0;
            var timeCondition = Utilities.InRange(normTime, attk.LinkEnd - Time.deltaTime, 1f);
            var existingCondition = Animator.IsInTransition(layer);

            return nameCondition && timeCondition && existingCondition;
        }


        /// <summary>
        /// Register a current combo
        /// </summary>
        /// <param name="currentCombo"></param>
        protected internal void RegisterCurrentCombo(Combo currentCombo)
        {
            if (ActiveCombo == null) ActiveCombo = currentCombo;
        }

        /// <summary>
        /// Unregister the current combo and reset
        /// </summary>
        /// <param name="currentCombo"></param>
        protected internal void UnRegisterCurrentCombo(Combo currentCombo)
        {
            if (currentCombo != ActiveCombo) return;

            ActiveCombo = null;
        }

        #endregion
    }
}