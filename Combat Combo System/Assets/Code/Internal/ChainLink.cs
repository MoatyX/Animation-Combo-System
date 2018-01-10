using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Defines a link in the ComboChain which could have more than 1 actual animation
/// </summary>
public class ChainLink
{
    public Queue<AnimCombo> combos = new Queue<AnimCombo>();
    public bool hasFinished;

    private List<AnimCombo> mainCombos = new List<AnimCombo>();

    public ChainLink(IEnumerable<AnimCombo> combos)
    {
        mainCombos = combos.ToList();

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
        mainCombos.Add(combo);
        combos.Enqueue(combo);
        hasFinished = true;
    }

    public bool Reset()
    {
        combos = new Queue<AnimCombo>(mainCombos);
        hasFinished = true;

        return true;
    }
}
