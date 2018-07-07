using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generics.Utilities
{
    /// <summary>
    /// A combo
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Combo", menuName = "Animation Combo System/New Combo")]
    public class Combo : ScriptableObject
    {
        [Tooltip("the name of the layer that has the combo anims")]
        public string layerName;

        [Tooltip("The combo animation")] public AttackAnim[] animations;

        [Tooltip("Setup how the Input triggers the Combo")]
        public KeySequencer inputSequencer;

        private ChainLink currenLink;
        private AttackAnim currentAttk;

        private ComboSequencer brain;
        private ChainLink[] mainChain;
        private Queue<ChainLink> chainQueue;
        private Queue<Action> actions;

        private bool _ignoreInput;
        private int _layer;

        /// <summary>
        /// Setup and prepare
        /// </summary>
        /// <returns></returns>
        protected internal bool Initialise(ComboSequencer brain)
        {
            if (inputSequencer.sequence.Length > animations.Length) inputSequencer.type = SequenceType.Full;
            int chainLength = inputSequencer.type == SequenceType.Full ? 1 : inputSequencer.sequence.Length;

            mainChain = new ChainLink[chainLength];
            _ignoreInput = false;
            this.brain = brain;

            _layer = brain.Animator.GetLayerIndex(layerName);

            for (int i = 0; i < chainLength; i++)
            {
                ChainLink link;

                //put the rest of the anims inside the last chain link
                if (i + 1 == chainLength && animations.Length >= 1)
                {
                    AttackAnim[] rest = new AttackAnim[animations.Length - i];
                    Array.Copy(animations, i, rest, 0, rest.Length);

                    link = new ChainLink(rest);
                    mainChain[i] = link;
                    break;
                }

                link = new ChainLink(animations[i]);
                mainChain[i] = link;
            }

            chainQueue = new Queue<ChainLink>(mainChain);
            actions = new Queue<Action>();
            inputSequencer.Setup();
            return true;
        }

        /// <summary>
        /// process
        /// </summary>
        protected internal void Update()
        {
            switch (inputSequencer.type)
            {
                case SequenceType.Full:
                    FullSequencer();
                    break;
                case SequenceType.Partial:
                    TimedPartialSequencer();
                    break;
                case SequenceType.PartialAppending:
                    AppendSequencer();
                    break;
                default:
                    Debug.Log("Achievement unlocked: you have broken the system !");
                    break;
            }
        }

        /// <summary>
        /// Full input sequencer
        /// </summary>
        private void FullSequencer()
        {
            KeySequencer.SequenceState inputState = inputSequencer.Listen(_ignoreInput);

            switch (inputState)
            {
                case KeySequencer.SequenceState.Interupted:
                    ResetAll();
                    break;
                case KeySequencer.SequenceState.Neutrial:
                    if (currenLink == null) break;

                    //move automatically to the next combo in the chain
                    if (brain.IsBeginningLink(currentAttk, _layer) && currenLink.Combos.Count > 0)
                    {
                        //play next
                        currentAttk.Reset();
                        currentAttk = currenLink.Combos.Dequeue();
                        brain.NextAnimation(currentAttk, _layer);
                        Dispatcher.OnAttackTriggered(currentAttk);
                    }

                    //be ready to trigger evenets
                    brain.ScanTimeline(currentAttk, _layer);

                    //finished playing the whole chain
                    if (currenLink.Combos.Count <= 0 && brain.IsEndingLink(currentAttk, _layer))
                    {
                        ResetAll();
                        Dispatcher.OnComboCompleted(this);
                    }

                    break;
                case KeySequencer.SequenceState.Completed:
                    currenLink = chainQueue.Peek();

                    //start the execution
                    brain.RegisterCurrentCombo(this);
                    _ignoreInput = true;
                    currentAttk = currenLink.Combos.Dequeue();
                    brain.NextAnimation(currentAttk, _layer);

                    Dispatcher.OnAttackTriggered(currentAttk);
                    break;
            }
        }

        /// <summary>
        /// The Timed Partial Sequencer logic
        /// </summary>
        private void TimedPartialSequencer()
        {
            KeySequencer.SequenceState inputState = inputSequencer.Listen(_ignoreInput);
            switch (inputState)
            {
                case KeySequencer.SequenceState.Success:
                    Link();
                    break;
                case KeySequencer.SequenceState.Interupted:
                    ResetAll();
                    break;
                case KeySequencer.SequenceState.Neutrial:
                    if (currenLink == null) break;

                    /* Duties:
                     * scan the playing animation's timeline for trigger points
                     * ignore input as long as the current playing anim is not within the linking phase
                     * Excute the long final chain link (if available) automatically
                     * reset the sequencer if the animation finishes playing and no input was received
                     */

                    //scan for trigger points
                    brain.ScanTimeline(currentAttk, _layer);

                    //determain timing
                    _ignoreInput = !brain.WithinLink(currentAttk, _layer);

                    //check for a long combo link and execute it
                    if (currenLink.Combos.Count > 0 && brain.IsEndingLink(currentAttk, _layer))
                    {
                        _ignoreInput = true;
                        currentAttk.Reset();
                        currentAttk = currenLink.Combos.Dequeue();
                        brain.NextAnimation(currentAttk, _layer);

                        Dispatcher.OnAttackTriggered(currentAttk);
                    }

                    //reset if no input and the animation has finished playing
                    currenLink.HasFinished = currenLink.Combos.Count == 0 && brain.IsExisting(currentAttk, _layer);
                    if (currenLink.HasFinished)
                    {
                        if (currenLink.Combos.Count == 0 && chainQueue.Count == 0)
                        {
                            Dispatcher.OnComboCompleted(this);
                        }

                        ResetAll();
                    }

                    break;
                case KeySequencer.SequenceState.Completed:
                    Link();
                    break;
            }
        }

        /// <summary>
        /// Buffered Sequencer
        /// </summary>
        private void AppendSequencer()
        {
            KeySequencer.SequenceState inputState = inputSequencer.BufferedListening();

            //put all Actions corresponding to the SequenceState in the Buffer and execute them when the Key Sequencer is in a neutral state
            switch (inputState)
            {
                case KeySequencer.SequenceState.Success:
                    actions.Enqueue(Link);
                    break;
                case KeySequencer.SequenceState.Completed:
                    actions.Enqueue(Link);
                    actions.Enqueue(delegate { Dispatcher.OnComboCompleted(this); });
                    break;
                case KeySequencer.SequenceState.Interupted:
                    actions.Enqueue(ResetAll);
                    break;
                case KeySequencer.SequenceState.Neutrial:

                    if (currenLink == null && actions.Count > 0) //start
                    {
                        actions.Dequeue().Invoke();
                    }

                    if (currenLink != null)
                    {
                        currenLink.HasFinished = brain.IsExisting(currentAttk, _layer);
                        if (currenLink.HasFinished)
                        {
                            //no key strokes and the anim has finished playing. reset !
                            ResetAll();
                            break;
                        }

                        if (brain.IsEndingLink(currentAttk, _layer))
                        {
                            if (actions.Count > 0) //transition
                            {
                                actions.Dequeue().Invoke();
                            }
                            else if (currenLink.Combos.Count > 0) //finish off
                            {
                                currentAttk = currenLink.Combos.Dequeue();
                                brain.NextAnimation(currentAttk, _layer);

                                Dispatcher.OnAttackTriggered(currentAttk);
                            }
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Link a new Chain Link combo to the sequencer
        /// </summary>
        private void Link()
        {
            if (chainQueue.Count <= 0) return;
            brain.RegisterCurrentCombo(this);

            currenLink = chainQueue.Dequeue();
            if (currentAttk) currentAttk.Reset(); //reset the last combo first
            currentAttk = currenLink.Combos.Dequeue();
            brain.NextAnimation(currentAttk, _layer);

            currenLink.HasFinished = false;

            Dispatcher.OnAttackTriggered(currentAttk);
        }

        /// <summary>
        /// Universal reset switch
        /// </summary>
        public void ResetAll()
        {
            switch (inputSequencer.type)
            {
                case SequenceType.Full:
                    ResetFullSequence();
                    break;
                case SequenceType.Partial:
                    ResetPartialSequence();
                    break;
                case SequenceType.PartialAppending:
                    ResetPartialSequence();
                    break;
            }
        }

        /// <summary>
        /// Reset the partial sequencer
        /// </summary>
        private void ResetPartialSequence()
        {
            currenLink = null;
            currentAttk = null;
            _ignoreInput = false;

            brain.UnRegisterCurrentCombo(this);
            if (chainQueue.Count != mainChain.Length)
            {
                //Enumerable.All(mainChain, x => x.Reset());
                for (int i = 0; i < mainChain.Length; i++)
                {
                    mainChain[i].Reset();
                }

                chainQueue = new Queue<ChainLink>(mainChain);
            }

            inputSequencer.Reset();
        }

        /// <summary>
        /// Reset the full-input sequence
        /// </summary>
        private void ResetFullSequence()
        {
            brain.UnRegisterCurrentCombo(this);

            chainQueue.Peek().Reset();
            currentAttk = null;
            _ignoreInput = false;
        }
    }
}