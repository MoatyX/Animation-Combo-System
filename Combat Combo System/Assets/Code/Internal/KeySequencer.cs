using System.Collections.Generic;
using UnityEngine;

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

    public float timeLimit;
    public KeyCode[] sequence;

    public Queue<KeyCode> queue = new Queue<KeyCode>();
    private SequenceType type;
    private float timer;


    /// <summary>
    /// Initialisation
    /// </summary>
    /// <param name="type"></param>
    public void Setup(SequenceType type)
    {
        foreach (KeyCode k in sequence)
        {
            queue.Enqueue(k);
        }

        timer = Time.time;
        this.type = type;
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

        if (timeLimit < Time.time - timer)
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
        if(sequence.Length == queue.Count) return;

        queue.Enqueue(sequence[sequence.Length - 1 - queue.Count]);
    }
}
