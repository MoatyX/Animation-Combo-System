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
        public List<Combo> combos;

        public Animator animator;
        private bool initiated;

        private Combo activeCombo;

        /// <summary>
        /// Setup and prepare
        /// </summary>
        public bool Initialise()
        {
            combos.All(x => x.Initialise(this));
            initiated = true;
            return initiated;
        }

        /// <summary>
        /// Update all the combos
        /// </summary>
        public void Update()
        {
            if (!initiated)
            {
                Debug.LogError("Make sure to call the Setup() function at the beginning to initiate the combo and/or all erros are resolved");
                return;
            }

            for (int i = 0; i < combos.Count; i++)
            {
                if(activeCombo != null && activeCombo != combos[i]) continue;

                combos[i].Update();
            }

            //IsCurrentlyActive(null);
        }

        /// <summary>
        /// Call the next animation
        /// </summary>
        public void NextAnimation(AttackAnim currentAnim, int targetLayer)
        {
            if (currentAnim != null)
            {
                if(!IsCurrentlyActive(currentAnim, targetLayer,true))
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
        public bool IsCurrentlyActive(AttackAnim currentAnim, int targetLayer, bool onlyActivation = false)
        {
            if (currentAnim == null) return false;

            float currentNormalisedTime = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(targetLayer).normalizedTime);
            float start = currentAnim.LinkBegin;
            float end = currentAnim.LinkEnd;

            //trigger a scan point
            for (int i = 0; i < currentAnim.ScanPoints.Length; i++)
            {
                if (Utilities.InRange(currentNormalisedTime, currentAnim.ScanPoints[i], currentAnim.ScanPoints[i]))
                {
                    Dispatcher.OnHitScanning(currentAnim);
                }
            }

            bool condition1 = animator.GetCurrentAnimatorStateInfo(targetLayer).shortNameHash == currentAnim.AnimHash;      //name match
            bool condition2 = Utilities.InRange(currentNormalisedTime, start , end) || onlyActivation;                      //in range
            bool condition3 = animator.IsInTransition(targetLayer) && condition2;

            return condition1 && condition2 && condition3;
        }

        /// <summary>
        /// Check if the attack anim has crossed the LinkBegin time stamp
        /// </summary>
        /// <param name="attk"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool IsBeginningLink(AttackAnim attk, int layer)
        {
            if (attk == null) return false;

            var normTime = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
            var nameCondition = animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash;      //name match
            var timeCondition = Utilities.InRange(normTime, attk.LinkBegin, 1f);

            return nameCondition && timeCondition;
        }

        /// <summary>
        /// check if an anim has crossed the LinkEnd time stamp
        /// </summary>
        /// <param name="attk"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool IsEndingLink(AttackAnim attk, int layer)
        {
            if (attk == null) return false;

            var normTime = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
            var nameCondition = animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash;      //name match
            var timeCondition = Utilities.InRange(normTime, attk.LinkEnd, 1f);                                   //Range match
            var existingCondition = animator.IsInTransition(layer);

            return nameCondition && (timeCondition || existingCondition);
        }

        public bool WithinLink(AttackAnim attk, int layer)
        {
            if (attk == null) return false;

            var normTime = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
            var nameCondition = animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash;      //name match
            var timeCondition = Utilities.InRange(normTime, attk.LinkBegin, attk.LinkEnd);

            return nameCondition && timeCondition;
        }

        /// <summary>
        /// check if a anim has crossed the LinkEnd time stamp AND has been fully played
        /// </summary>
        /// <param name="attk"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool IsExisting(AttackAnim attk, int layer)
        {
            if (attk == null) return false;

            var normTime = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
            var nameCondition = animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == attk.AnimHash;      //name match
            var timeCondition = Utilities.InRange(normTime, attk.LinkEnd, 1f);
            var existingCondition = animator.IsInTransition(layer);

            return nameCondition && timeCondition && existingCondition;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentCombo"></param>
        public void HoldSequencer(Combo currentCombo)
        {
            activeCombo = currentCombo;
        }

        /// <summary>
        /// Resume
        /// </summary>
        public void ResumeSequencer()
        {
            activeCombo = null;

            //combos.All(x =>
            //{
            //    x.Reset();
            //    return true;
            //});
        }
    }
}