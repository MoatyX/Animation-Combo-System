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

        public Animator animator;
        private bool _initiated;

        public Combo ActiveCombo { get; private set; }

        /// <summary>
        /// Setup and prepare
        /// </summary>
        public bool Initialise()
        {
            Combos.All(x => x.Initialise(this));
            _initiated = true;
            return _initiated;
        }

        /// <summary>
        /// Update all the combos
        /// </summary>
        public void Update()
        {
            if (!_initiated)
            {
                Debug.LogError(
                    "Make sure all necessary references are set, and to call the Setup() function at the beginning to initiate the combo");
                return;
            }

            for (var i = 0; i < Combos.Count; i++)
            {
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

        /// <summary>
        /// Call the next animation
        /// </summary>
        protected internal void NextAnimation(AttackAnim currentAnim, int targetLayer)
        {
            if (currentAnim != null)
            {
                if (!IsCurrentlyActive(currentAnim, targetLayer, true))
                    animator.CrossFadeInFixedTime(currentAnim.AnimName, currentAnim.TransitionDuration, targetLayer);
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
        /// <param name="onlyActivation">true, if we dont want to consider timing</param>
        /// <returns></returns>
        protected internal bool IsCurrentlyActive(AttackAnim currentAnim, int targetLayer, bool onlyActivation = false)
        {
            if (currentAnim == null) return false;

            float currentNormalisedTime =
                Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(targetLayer).normalizedTime);
            float start = currentAnim.LinkBegin;
            float end = currentAnim.LinkEnd;

            bool condition1 =
                animator.GetCurrentAnimatorStateInfo(targetLayer).shortNameHash == currentAnim.AnimHash; //name match
            bool condition2 = Utilities.InRange(currentNormalisedTime, start, end) || onlyActivation; //in range
            bool condition3 = animator.IsInTransition(targetLayer) && condition2;

            return condition1 && condition2 && condition3;
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

            float normTime = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
            for (int i = 0; i < attk.ScanPoints.Length; i++)
            {
                if (Utilities.InRange(normTime, attk.ScanPoints[i], attk.ScanPoints[i] + Time.deltaTime))
                {
                    Dispatcher.OnHitScanning(attk);
                }
            }
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

            var normTime = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
            var nameCondition = animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash; //name match
            var timeCondition = Utilities.InRange(normTime, attk.LinkBegin, 1f);

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

            var normTime = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
            var nameCondition = animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash; //name match
            var timeCondition = Utilities.InRange(normTime, attk.LinkEnd, 1f); //Range match
            var existingCondition = animator.IsInTransition(layer);

            return nameCondition && (timeCondition || existingCondition);
        }

        protected internal bool WithinLink(AttackAnim attk, int layer)
        {
            if (attk == null) return false;

            var normTime = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
            var nameCondition = animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash; //name match
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

            var normTime = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
            var nameCondition = animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash; //name match
            var timeCondition = Utilities.InRange(normTime, attk.LinkEnd, 1f);
            var existingCondition = animator.IsInTransition(layer);

            return nameCondition && timeCondition && existingCondition;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentCombo"></param>
        protected internal void RegisterCurrentCombo(Combo currentCombo)
        {
            if (ActiveCombo == null) ActiveCombo = currentCombo;
        }

        protected internal void UnRegisterCurrentCombo(Combo currentCombo)
        {
            if (currentCombo != ActiveCombo) return;

            ActiveCombo = null;
        }
    }
}