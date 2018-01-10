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

    public void Setup(SequenceType type)
    {
        for (int i = 0; i < sequence.Length; i++)
        {
            queue.Enqueue(sequence[i]);
        }

        timer = Time.time;
        this.type = type;
    }

    public SequenceState Listen(bool ignoreThis = false)
    {
        if (ignoreThis)
        {
            return SequenceState.Neutrial;
        }

        if (timeLimit < Time.time - timer)
        {
            //Debug.Log("We ran out of time");

            Reset();
            return SequenceState.Interupted;
        }

        if (Input.GetKeyDown(queue.Peek()))
        {
            //Debug.Log("Dequeued " + queue.Peek());

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
            Reset();
            //Debug.Log("Incorrect stroke");
            return SequenceState.Interupted;
        }

        if (queue.Count <= 0)
        {
            Reset();
            //Debug.Log("Sequence complete");

            return SequenceState.Completed;
        }

        return SequenceState.Neutrial;
    }

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
