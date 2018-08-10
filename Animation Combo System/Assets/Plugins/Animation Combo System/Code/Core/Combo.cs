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
        public string LayerName;

        [Tooltip("The combo animation")] public AttackAnim[] Animations;

        [Tooltip("Setup how the Input triggers the Combo")]
        public KeySequencer InputSequencer;

        private ChainLink _currenLink;
        private AttackAnim _currentAttk;

        private ComboSequencer _brain;
        private ChainLink[] _mainChain;
        private Queue<ChainLink> _chainQueue;
        private Queue<Action> _actions;

        private bool _ignoreInput;
        private int _layer;

        /// <summary>
        /// Setup and prepare
        /// </summary>
        /// <returns></returns>
        protected internal bool Initialise(ComboSequencer brain)
        {
            if (Animations.Length == 0)
            {
                Debug.LogWarning(string.Format("No Animations were set in the Combo: {0}", this.name));
                return false;
            }

            if (InputSequencer.Sequence.Length == 0)
            {
                Debug.LogWarning(string.Format("No InputSequence was set in the Combo: {0}", this.name));
                return false;
            }

            if (string.IsNullOrEmpty(LayerName))
            {
                Debug.LogWarning(string.Format("No LayerName set in the Combo: {0}", this.name));
                return false;
            }

            _layer = brain.Animator.GetLayerIndex(LayerName);

            if (_layer == -1)
            {
                Debug.LogWarning(string.Format("Invalid LayerName in the Combo: {0}", this.name));
                return false;
            }

            if (InputSequencer.Sequence.Length > Animations.Length) InputSequencer.Type = SequenceType.Full;
            int chainLength = InputSequencer.Type == SequenceType.Full ? 1 : InputSequencer.Sequence.Length;

            _mainChain = new ChainLink[chainLength];
            _ignoreInput = false;
            this._brain = brain;

            for (int i = 0; i < chainLength; i++)
            {
                ChainLink link;

                //put the rest of the anims inside the last chain link
                if (i + 1 == chainLength && Animations.Length >= 1)
                {
                    AttackAnim[] rest = new AttackAnim[Animations.Length - i];
                    Array.Copy(Animations, i, rest, 0, rest.Length);

                    link = new ChainLink(rest);
                    _mainChain[i] = link;
                    break;
                }

                link = new ChainLink(Animations[i]);
                _mainChain[i] = link;
            }

            _chainQueue = new Queue<ChainLink>(_mainChain);
            _actions = new Queue<Action>();
            InputSequencer.Setup();
            return true;
        }

        /// <summary>
        /// process
        /// </summary>
        protected internal void Update()
        {
            switch (InputSequencer.Type)
            {
                case SequenceType.Full:
                    FullSequencer();
                    break;
                case SequenceType.Partial:
                    TimedPartialSequencer();
                    break;
                case SequenceType.PartialAppending:
                    ScheduledSequencer();
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
            KeySequencer.SequenceState inputState = InputSequencer.Listen(_ignoreInput);

            switch (inputState)
            {
                case KeySequencer.SequenceState.Interupted:
                    ResetAll();
                    break;
                case KeySequencer.SequenceState.Neutrial:
                    if (_currenLink == null) break;

                    //move automatically to the next combo in the chain
                    if (_brain.IsBeginningLink(_currentAttk, _layer) && _currenLink.Combos.Count > 0)
                    {
                        //play next
                        _currentAttk.Reset();
                        _currentAttk = _currenLink.Combos.Dequeue();
                        _brain.NextAnimation(_currentAttk, _layer);
                        Dispatcher.OnAttackTriggered(_currentAttk);
                    }

                    //be ready to trigger evenets
                    _brain.ScanTimeline(_currentAttk, _layer);

                    //finished playing the whole chain
                    if (_currenLink.Combos.Count <= 0 && _brain.IsEndingLink(_currentAttk, _layer))
                    {
                        ResetAll();
                        Dispatcher.OnComboCompleted(this);
                    }

                    break;
                case KeySequencer.SequenceState.Completed:
                    _currenLink = _chainQueue.Peek();

                    //start the execution
                    _brain.RegisterCurrentCombo(this);
                    _ignoreInput = true;
                    _currentAttk = _currenLink.Combos.Dequeue();
                    _brain.NextAnimation(_currentAttk, _layer);

                    Dispatcher.OnAttackTriggered(_currentAttk);
                    break;
            }
        }

        /// <summary>
        /// The Timed Partial Sequencer logic
        /// </summary>
        private void TimedPartialSequencer()
        {
            KeySequencer.SequenceState inputState = InputSequencer.Listen(_ignoreInput);
            switch (inputState)
            {
                case KeySequencer.SequenceState.Success:
                    Link();
                    break;
                case KeySequencer.SequenceState.Interupted:
                    ResetAll();
                    break;
                case KeySequencer.SequenceState.Neutrial:
                    if (_currenLink == null) break;

                    /* Duties:
                     * scan the playing animation's timeline for trigger points
                     * ignore input as long as the current playing anim is not within the linking phase
                     * Excute the long final chain link (if available) automatically
                     * reset the sequencer if the animation finishes playing and no input was received
                     */

                    //scan for trigger points
                    _brain.ScanTimeline(_currentAttk, _layer);

                    //determain timing
                    _ignoreInput = !_brain.WithinLink(_currentAttk, _layer);

                    //check for a long combo link and execute it
                    if (_currenLink.Combos.Count > 0 && _brain.IsEndingLink(_currentAttk, _layer))
                    {
                        _ignoreInput = true;
                        _currentAttk.Reset();
                        _currentAttk = _currenLink.Combos.Dequeue();
                        _brain.NextAnimation(_currentAttk, _layer);

                        Dispatcher.OnAttackTriggered(_currentAttk);
                    }

                    //reset if no input and the animation has finished playing
                    _currenLink.HasFinished = _currenLink.Combos.Count == 0 && _brain.IsExisting(_currentAttk, _layer);
                    if (_currenLink.HasFinished)
                    {
                        if (_currenLink.Combos.Count == 0 && _chainQueue.Count == 0)
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
        /// Buffered Seuquencer
        /// </summary>
        private void ScheduledSequencer()
        {
            KeySequencer.SequenceState inputState = InputSequencer.BufferedListening();
            switch (inputState)
            {
                case KeySequencer.SequenceState.Success:
                    _actions.Enqueue(Link);
                    break;
                case KeySequencer.SequenceState.Interupted:
                    _actions.Enqueue(ResetAll);
                    break;
                case KeySequencer.SequenceState.Neutrial:

                    /* Duties:
                     * activate actions (no matter which) at the LinkEnd of Attacks (if non playing, activate directly)
                     * if anim finishes and No input -> reset
                     */

                    if (_currenLink == null && _actions.Count > 0) //start
                    {
                        _actions.Dequeue().Invoke();
                    }

                    if (_currenLink != null)
                    {
                        if (_brain.IsEndingLink(_currentAttk, _layer))
                        {
                            if (_actions.Count > 0) //transition
                            {
                                _actions.Dequeue().Invoke();
                            }
                            else if (_currenLink.Combos.Count > 0) //finish off
                            {
                                _currentAttk.Reset();
                                _currentAttk = _currenLink.Combos.Dequeue();
                                _brain.NextAnimation(_currentAttk, _layer);

                                Dispatcher.OnAttackTriggered(_currentAttk);
                            }
                        }

                        if (_currenLink != null)
                        {
                            _currenLink.HasFinished = _brain.IsExisting(_currentAttk, _layer);
                            if (_currenLink.HasFinished)
                            {
                                if (_currenLink.Combos.Count == 0 && _chainQueue.Count == 0)
                                {
                                    Dispatcher.OnComboCompleted(this);
                                }

                                //no key strokes and the anim has finished playing. reset !
                                ResetAll();
                            }
                        }
                    }

                    break;
                case KeySequencer.SequenceState.Completed:
                    _actions.Enqueue(Link);
                    break;
            }
        }

        /// <summary>
        /// Link a new Chain Link combo to the sequencer
        /// </summary>
        private void Link()
        {
            if (_chainQueue.Count <= 0) return;
            _brain.RegisterCurrentCombo(this);

            _currenLink = _chainQueue.Dequeue();
            if (_currentAttk) _currentAttk.Reset(); //reset the last combo first
            _currentAttk = _currenLink.Combos.Dequeue();
            _brain.NextAnimation(_currentAttk, _layer);

            _currenLink.HasFinished = false;

            Dispatcher.OnAttackTriggered(_currentAttk);
        }

        /// <summary>
        /// Universal reset switch
        /// </summary>
        public void ResetAll()
        {
            switch (InputSequencer.Type)
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
            _currenLink = null;
            _currentAttk = null;
            _ignoreInput = false;

            _brain.UnRegisterCurrentCombo(this);
            if (_chainQueue.Count != _mainChain.Length)
            {
                //Enumerable.All(mainChain, x => x.Reset());
                for (int i = 0; i < _mainChain.Length; i++)
                {
                    _mainChain[i].Reset();
                }

                _chainQueue = new Queue<ChainLink>(_mainChain);
            }

            InputSequencer.Reset();
        }

        /// <summary>
        /// Reset the full-input sequence
        /// </summary>
        private void ResetFullSequence()
        {
            _brain.UnRegisterCurrentCombo(this);

            _chainQueue.Peek().Reset();
            _currentAttk = null;
            _ignoreInput = false;
        }
    }
}