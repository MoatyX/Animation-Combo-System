using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines a link in the ComboChain which could have more than 1 actual animation
/// </summary>
public class ChainLink
{
    public Queue<AnimCombo> combos = new Queue<AnimCombo>();
    public bool hasFinished;

    public ChainLink(IEnumerable<AnimCombo> combos)
    {
        IEnumerator<AnimCombo> combo = combos.GetEnumerator();
        while (combo.MoveNext())
        {
            this.combos.Enqueue(combo.Current);
        }

        combo.Dispose();
        hasFinished = true;
    }

    public ChainLink(AnimCombo combo)
    {
        combos.Enqueue(combo);
        hasFinished = true;
    }

    public void Reset(AnimCombo combo)
    {
        combos.Enqueue(combo);
    }

    public void Reset(AnimCombo[] combos)
    {
        for (int i = 0; i < combos.Length; i++)
        {
            this.combos.Enqueue(combos[i]);
        }

        hasFinished = true;
    }
}
