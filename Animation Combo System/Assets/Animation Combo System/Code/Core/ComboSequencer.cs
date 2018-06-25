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
                Debug.Log("The current Animation in the link is not defined");
            }
        }

        /// <summary>
        /// Check if a current animation in the link is active under its conditions
        /// </summary>
        /// <param name="currentAnim"></param>
        /// <param name="targetLayer">the layer which the anim is on</param>
        /// <param name="onlyActivation">true, if we dont want to consider timing</param>
        /// <param name="isFinishing">true if we only want to include a 'if the state is about to finish and exit' check</param>
        /// <returns></returns>
        public bool IsCurrentlyActive(AttackAnim currentAnim, int targetLayer, bool onlyActivation = false, bool isFinishing = false)
        {
            if (currentAnim == null) return false;

            float currentNormalisedTime = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(targetLayer).normalizedTime);
            float start = isFinishing ? currentAnim.LinkEnd : currentAnim.LinkBegin;
            float end = isFinishing ? 1f : currentAnim.LinkEnd;

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
            bool condition3 = !condition1 || condition2;                                                                    //both must be true to call it a night
            bool condition4 = !isFinishing || animator.IsInTransition(targetLayer) && condition2;

            return condition1 && condition2 && condition3 && condition4;
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