using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class ComboSequencer
{
    public SequenceType type;

    [SerializeField] int animsLayer;
    [SerializeField] string defaultAnim;
    [SerializeField] Animator anim;
    [SerializeField] KeySequencer keys;
    [SerializeField] AnimCombo[] animations;

    private Queue<ChainLink> chainQueue = new Queue<ChainLink>();
    private ChainLink[] mainChain;
    private AnimCombo currentCombo;
    private ChainLink currenLink;

    private bool initiated;
    private bool ignoreInput;

    /// <summary>
    /// Setup the Combo sequencer
    /// </summary>
    public bool Setup()
    {
        if (keys.sequence.Length > animations.Length) type = SequenceType.Full;
        int chainLength = type == SequenceType.Full ? 1 : keys.sequence.Length;

        mainChain = new ChainLink[chainLength];

        for (int i = 0; i < chainLength; i++)
        {
            ChainLink link;

            //put the rest of the anims inside the last chain link
            if (i + 1 == chainLength && animations.Length >= 1)
            {
                AnimCombo[] rest = new AnimCombo[animations.Length - i];
                Array.Copy(animations, i, rest, 0, rest.Length);
                
                link = new ChainLink(rest);
                mainChain[i] = link;
                break;
            }

            link = new ChainLink(animations[i]);
            mainChain[i] = link;
        }

        chainQueue = new Queue<ChainLink>(mainChain);

        keys.Setup(type);

        return initiated = anim;
    }

    /// <summary>
    /// Update the Sequencer
    /// </summary>
    public void Update()
    {
        if (!initiated)
        {
            Debug.LogError("Make sure to call the Setup() function at the beginning to initiate the combo");
            return;
        }

        switch (type)
        {
            case SequenceType.Full:
                FullSequencer();
                break;
            case SequenceType.Partial:
                PartialSequencer();
                break;
            default:
                Debug.Log("Achievement unlocked: you have discovered a bug !");
                break;
        }
    }

    /// <summary>
    /// Execute a Full Sequqence
    /// </summary>
    private void FullSequencer()
    {
        KeySequencer.SequenceState inputState = keys.Listen();
        currenLink = chainQueue.Peek();

        if (inputState == KeySequencer.SequenceState.Completed && currenLink.hasFinished)
        {
            //Debug.Log("Executing combo chain");

            currentCombo = currenLink.combos.Dequeue();
            anim.CrossFadeInFixedTime(currentCombo.animName, 0.1f, animsLayer);

            currenLink.hasFinished = false;
        }

        //move automatically to the next combo in the chain
        if (currentCombo != null && IsCurrentlyPlaying(currentCombo.animName, currentCombo.linkBegin) && currenLink.combos.Count > 0)
        {
            currentCombo = currenLink.combos.Dequeue();
            anim.CrossFadeInFixedTime(currentCombo.animName, 0.1f, animsLayer);
        }

        if (currenLink.combos.Count <= 0 && currentCombo != null && IsCurrentlyPlaying(currentCombo.animName, currentCombo.linkBegin))
        {
            //Debug.Log("Finished");
            ResetFullSequence();
        }
    }

    /// <summary>
    /// Execute a partial Sequence
    /// </summary>
    private void PartialSequencer()
    {
        KeySequencer.SequenceState inputState = keys.Listen(ignoreInput);

        switch (inputState)
        {
            case KeySequencer.SequenceState.Success:

                currenLink = chainQueue.Dequeue();
                currentCombo = currenLink.combos.Dequeue();
                anim.CrossFadeInFixedTime(currentCombo.animName, 0.1f, animsLayer);

                currenLink.hasFinished = false;
                break;
            case KeySequencer.SequenceState.Completed:
                currenLink = chainQueue.Dequeue();
                currentCombo = currenLink.combos.Dequeue();
                anim.CrossFadeInFixedTime(currentCombo.animName, 0.1f, animsLayer);

                currenLink.hasFinished = false;

                Debug.Log(chainQueue.Count);
                break;
            case KeySequencer.SequenceState.Interupted:
                ResetPartialSequence();
                break;
            case KeySequencer.SequenceState.Neutrial:
                if (currentCombo != null && currenLink != null)
                {
                    currenLink.hasFinished = IsCurrentlyPlaying(currentCombo.animName, currentCombo.linkEnd);
                    if (currenLink.hasFinished)
                    {
                        Debug.Log("No key stroke was input, resetting");
                        ResetPartialSequence();
                        break;
                    }

                    if (IsCurrentlyPlaying(currentCombo.animName, currentCombo.linkBegin))
                    {
                        ignoreInput = false;
                    }
                    else
                    {
                        ignoreInput = true;
                        Debug.Log("Ignoring input");
                    }
                }
                else
                {
                    ignoreInput = false;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// is the current state playing == stateName ?
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="layer">Which animator layer</param>
    /// <param name="minDuration">min normalised duration to consider</param>
    /// <param name="maxDuration">max normalised duration to consider</param>
    /// <returns></returns>
    public bool IsCurrentlyPlaying(string stateName, float minDuration = 0f, float maxDuration = 1f)
    {
        float currentNormalisedTime = Mathf.Clamp01(anim.GetCurrentAnimatorStateInfo(animsLayer).normalizedTime);
        bool condition1 = anim.GetCurrentAnimatorStateInfo(animsLayer).IsName(stateName);
        bool condition2 = !condition1 || Utilities.InRange(currentNormalisedTime, minDuration, maxDuration);

        return condition1 && condition2;
    }

    private void ResetFullSequence()
    {
        chainQueue.Peek().Reset();
        currentCombo = null;
        anim.CrossFade(defaultAnim, 0.1f, animsLayer);
    }

    private void ResetPartialSequence()
    {
        currenLink = null;
        currentCombo = null;
        ignoreInput = false;


        if (chainQueue.Count != mainChain.Length)
        {
            mainChain.All(x => x.Reset());
            chainQueue = new Queue<ChainLink>(mainChain);
        }

        keys.Reset();

        anim.CrossFade(defaultAnim, 0.1f, animsLayer);
    }
}
