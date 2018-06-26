using System;
using System.Linq;
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

        [Tooltip("The combo animation")]
        public AttackAnim[] animations;

        [Tooltip("Setup how the Input triggers the Combo")]
        public KeySequencer inputSequencer;

        private ChainLink currenLink;
        private AttackAnim currentCombo;

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
        public bool Initialise(ComboSequencer brain)
        {
            if (inputSequencer.sequence.Length > animations.Length) inputSequencer.type = SequenceType.Full;
            int chainLength = inputSequencer.type == SequenceType.Full ? 1 : inputSequencer.sequence.Length;

            mainChain = new ChainLink[chainLength];
            _ignoreInput = false;
            this.brain = brain;

            _layer = brain.animator.GetLayerIndex(layerName);

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
        public void Update()
        {
            switch (inputSequencer.type)
            {
                case SequenceType.Full:
                    FullSequencer();
                    break;
                case SequenceType.Partial:
                    PartialSequencer();
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
                    if (brain.IsBeginningLink(currentCombo, _layer) && currenLink.Combos.Count > 0)
                    {
                        //play next
                        currentCombo = currenLink.Combos.Dequeue();
                        brain.NextAnimation(currentCombo, _layer);
                        Dispatcher.OnAttackTriggered(currentCombo);
                    }

                    //be ready to trigger evenets
                    brain.ScanTimeline(currentCombo, _layer);

                    //finished playing the whole chain
                    if (currenLink.Combos.Count <= 0 && brain.IsEndingLink(currentCombo, _layer))
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
                    currentCombo = currenLink.Combos.Dequeue();
                    brain.NextAnimation(currentCombo, _layer);

                    Dispatcher.OnAttackTriggered(currentCombo);
                    break;
            }
        }

        /// <summary>
        /// Partial-non-buffered Sequencer
        /// </summary>
        private void PartialSequencer()
        {
            KeySequencer.SequenceState inputState = inputSequencer.Listen(_ignoreInput);

            switch (inputState)
            {
                case KeySequencer.SequenceState.Success:
                    Link();
                    break;
                case KeySequencer.SequenceState.Completed:
                    Link();
                    break;
                case KeySequencer.SequenceState.Interupted:
                    //we have finised the combo
                    if (currenLink != null && currenLink.Combos.Count <= 0)
                    {
                        Dispatcher.OnComboCompleted(this);
                    }

                    ResetAll();
                    break;
                case KeySequencer.SequenceState.Neutrial:
                    if (currenLink != null)
                    {
                        currenLink.HasFinished = brain.IsExisting(currentCombo, _layer);
                        if (currenLink.HasFinished)
                        {
                            //no key strokes and the anim has finished playing. reset !
                            ResetAll();
                            break;
                        }

                        //execute links with long combos automatically
                        if (brain.IsEndingLink(currentCombo, _layer) && currenLink.Combos.Count > 0)
                        {
                            currentCombo = currenLink.Combos.Dequeue();
                            brain.NextAnimation(currentCombo, _layer);
                            _ignoreInput = true;

                            Dispatcher.OnAttackTriggered(currentCombo);
                        }

                        _ignoreInput = !brain.WithinLink(currentCombo, _layer);  //determain timing
                    }
                    else
                    {
                        _ignoreInput = false;
                    }
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
                    actions.Enqueue(delegate
                    {
                        Dispatcher.OnComboCompleted(this);
                    });
                    break;
                case KeySequencer.SequenceState.Interupted:
                    actions.Enqueue(ResetAll);
                    break;
                case KeySequencer.SequenceState.Neutrial:

                    if (currenLink == null && actions.Count > 0)  //start
                    {
                        actions.Dequeue().Invoke();
                    }

                    if (currenLink != null)
                    {
                        currenLink.HasFinished = brain.IsExisting(currentCombo, _layer);
                        if (currenLink.HasFinished)
                        {
                            //no key strokes and the anim has finished playing. reset !
                            ResetAll();
                            break;
                        }

                        if (brain.IsEndingLink(currentCombo, _layer))
                        {
                            if (actions.Count > 0) //transition
                            {
                                actions.Dequeue().Invoke();
                            }
                            else if (currenLink.Combos.Count > 0) //finish off
                            {
                                currentCombo = currenLink.Combos.Dequeue();
                                brain.NextAnimation(currentCombo, _layer);

                                Dispatcher.OnAttackTriggered(currentCombo);
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
            currentCombo = currenLink.Combos.Dequeue();
            brain.NextAnimation(currentCombo, _layer);

            currenLink.HasFinished = false;

            Dispatcher.OnAttackTriggered(currentCombo);
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
            currentCombo = null;
            _ignoreInput = false;

            brain.UnRegisterCurrentCombo(this);
            if (chainQueue.Count != mainChain.Length)
            {
                Enumerable.All(mainChain, x => x.Reset());
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
            currentCombo = null;
            _ignoreInput = false;
        }
    }
}