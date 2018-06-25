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

        public SequenceType type;
        public bool enableTimeLimit;
        public float timeLimit;
        public KeyCode[] sequence;

        private readonly Queue<KeyCode> queue = new Queue<KeyCode>();
        private float timer;


        /// <summary>
        /// Initialisation
        /// </summary>
        /// <param name="type"></param>
        public void Setup()
        {
            foreach (KeyCode k in sequence)
            {
                queue.Enqueue(k);
            }

            timer = Time.time;
        }

        /// <summary>
        /// Listen to the user input and process the key sequence
        /// </summary>
        /// <param name="ignoreThis">force the sequencer to ignore the input this frame</param>
        /// <returns>the sequence state</returns>
        public SequenceState Listen(bool ignoreThis = false)
        {
            if (ignoreThis)
            {
                return SequenceState.Neutrial;
            }

            if (timeLimit < Time.time - timer && enableTimeLimit)
            {
                //we ran out of time
                Reset();
                return SequenceState.Interupted;
            }

            if (Input.GetKeyDown(queue.Peek()))
            {
                queue.Dequeue();
                timer = Time.time;

                if (type == SequenceType.Partial)
                {
                    if (queue.Count > 0) return SequenceState.Success;

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

            if (queue.Count > 0) return SequenceState.Neutrial;

            Reset();
            return SequenceState.Completed;
        }

        /// <summary>
        /// Listen to all key input and put them in the buffer
        /// </summary>
        public SequenceState BufferedListening()
        {
            if (queue.Count == 0) return SequenceState.Neutrial;

            if (Input.GetKeyDown(queue.Peek()))
            {
                queue.Dequeue();
                return queue.Count > 0 ? SequenceState.Success : SequenceState.Completed;
            }

            if (!Input.anyKeyDown) return SequenceState.Neutrial;

            queue.Dequeue();
            return SequenceState.Interupted;
        }

        /// <summary>
        /// Reset the queue
        /// </summary>
        public void Reset()
        {
            //save us some performance if the queue didnt move
            if (queue.Count != sequence.Length)
            {
                queue.Clear();
                for (int i = 0; i < sequence.Length; i++)
                {
                    queue.Enqueue(sequence[i]);
                }
            }

            timer = Time.time;
        }

        /// <summary>
        /// Undo the last key stroke
        /// </summary>
        public void Undo()
        {
            if (sequence.Length == queue.Count) return;

            queue.Enqueue(sequence[sequence.Length - 1 - queue.Count]);
        }
    }
}