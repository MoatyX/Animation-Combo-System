using UnityEngine;
using System.Collections.Generic;

namespace Generics.Utilities
{
    /// <summary>
    /// the Input Sequencer System
    /// </summary>
    [System.Serializable]
    public class KeySequencer
    {
        public enum SequenceState
        {
            Success,
            Interupted,
            Neutrial,
            Completed
        }

        public SequenceType Type;
        public bool EnableTimeLimit = true;
        public float TimeLimit = 2f;
        public KeyCode[] Sequence;

        private readonly Queue<KeyCode> _queue = new Queue<KeyCode>();
        private float _timer;


        /// <summary>
        /// Initialisation
        /// </summary>
        public void Setup()
        {
            foreach (KeyCode k in Sequence)
            {
                _queue.Enqueue(k);
            }

            _timer = Time.time;
        }

        /// <summary>
        /// Listen to the user input and process the key sequence
        /// </summary>
        /// <param name="ignoreThisPass">force the sequencer to ignore the input this frame</param>
        /// <returns>the sequence state</returns>
        public SequenceState Listen(bool ignoreThisPass = false)
        {
            if (ignoreThisPass)
            {
                return SequenceState.Neutrial;
            }

            if (TimeLimit < Time.time - _timer && EnableTimeLimit)
            {
                //we ran out of time
                Reset();
                return SequenceState.Interupted;
            }

            if (Input.GetKeyDown(_queue.Peek()))
            {
                _queue.Dequeue();
                _timer = Time.time;

                if (Type == SequenceType.Partial)
                {
                    if (_queue.Count > 0) return SequenceState.Success;

                    Reset();
                    return SequenceState.Completed;
                }
            }
            else if (Input.anyKeyDown)
            {
                //incorrect stroke
                Reset();
                return SequenceState.Interupted;
            }

            if (_queue.Count > 0) return SequenceState.Neutrial;

            Reset();
            return SequenceState.Completed;
        }

        /// <summary>
        /// Listen to all key input and put them in the buffer
        /// </summary>
        public SequenceState BufferedListening()
        {
            if (_queue.Count == 0) return SequenceState.Neutrial;

            if (Input.GetKeyDown(_queue.Peek()))
            {
                _queue.Dequeue();
                return _queue.Count > 0 ? SequenceState.Success : SequenceState.Completed;
            }

            if (!Input.anyKeyDown) return SequenceState.Neutrial;

            _queue.Dequeue();
            return SequenceState.Interupted;
        }

        /// <summary>
        /// Reset the queue
        /// </summary>
        public void Reset()
        {
            //save us some performance if the queue didnt move
            if (_queue.Count != Sequence.Length)
            {
                _queue.Clear();
                for (int i = 0; i < Sequence.Length; i++)
                {
                    _queue.Enqueue(Sequence[i]);
                }
            }

            _timer = Time.time;
        }

        /// <summary>
        /// Undo the last key stroke
        /// </summary>
        public void Undo()
        {
            if (Sequence.Length == _queue.Count) return;

            _queue.Enqueue(Sequence[Sequence.Length - 1 - _queue.Count]);
        }
    }
}