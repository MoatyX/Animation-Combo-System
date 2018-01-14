using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class ComboSequencer
{
    public SequenceType type;

    [Header("Default Properties")]
    [SerializeField] int targetLayer;
    [SerializeField] string defaultAnim;
    [SerializeField] private float transitionDuration = 0.1f;

    [Header("Combo Setup")]
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
        #region Error checking
        if (!anim)
        {
            Debug.LogError("Animation Component was not assigned");
            return false;
        }

        if (animations.Length <= 0)
        {
            Debug.LogError("No Animations were setup");
            return false;
        }

        for (int i = 0; i < animations.Length; i++)
        {
            if (!string.IsNullOrEmpty(animations[i].animName)) continue;

            Debug.LogError("the animation of index " + i + " has an empty name string");
            return false;
        }
        #endregion


        if (keys.sequence.Length > animations.Length) type = SequenceType.Full;
        int chainLength = type == SequenceType.Full ? 1 : keys.sequence.Length;

        mainChain = new ChainLink[chainLength];
        animations.All(x => x.Setup());

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

        return initiated = true;
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
                Debug.Log("Achievement unlocked: you have broken the system !");
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
            //start the execution

            currentCombo = currenLink.combos.Dequeue();
            anim.CrossFadeInFixedTime(currentCombo.animHash, currentCombo.transitionDuration, targetLayer);

            currenLink.hasFinished = false;
        }

        //move automatically to the next combo in the chain
        if (currentCombo != null && IsCurrentlyPlaying(currentCombo.animHash, currentCombo.linkBegin) && currenLink.combos.Count > 0)
        {
            currentCombo = currenLink.combos.Dequeue();
            anim.CrossFadeInFixedTime(currentCombo.animHash, currentCombo.transitionDuration, targetLayer);
        }

        //finish
        if (currenLink.combos.Count <= 0 && currentCombo != null && IsCurrentlyPlaying(currentCombo.animHash, currentCombo.linkBegin))
        {
            ResetFullSequence();
        }
    }

    /// <summary>
    /// Execute a partial Sequence
    /// </summary>
    private void PartialSequencer()
    {
        KeySequencer.SequenceState inputState = keys.Listen(ignoreInput);

        Action Link = () =>
        {
            if(chainQueue.Count <= 0) return;

            currenLink = chainQueue.Dequeue();
            currentCombo = currenLink.combos.Dequeue();
            anim.CrossFadeInFixedTime(currentCombo.animHash, currentCombo.transitionDuration, targetLayer);

            currenLink.hasFinished = false;
        };

        switch (inputState)
        {
            case KeySequencer.SequenceState.Success:
                Link();
                break;
            case KeySequencer.SequenceState.Completed:
                Link();
                break;
            case KeySequencer.SequenceState.Interupted:
                ResetPartialSequence();
                break;
            case KeySequencer.SequenceState.Neutrial:
                if (currentCombo != null && currenLink != null)
                {
                    currenLink.hasFinished = IsCurrentlyPlaying(currentCombo.animHash, currentCombo.linkEnd);
                    if (currenLink.hasFinished)
                    {
                        //no key strokes, reset
                        ResetPartialSequence();
                        break;
                    }

                    //execute links with long combos automatically
                    if (IsCurrentlyPlaying(currentCombo.animHash, currentCombo.linkBegin) && currenLink.combos.Count > 0)
                    {
                        currentCombo = currenLink.combos.Dequeue();
                        anim.CrossFadeInFixedTime(currentCombo.animHash, currentCombo.transitionDuration, targetLayer);
                        ignoreInput = true;
                    }

                    ignoreInput = !IsCurrentlyPlaying(currentCombo.animHash, currentCombo.linkBegin);
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
    /// Reset the combo chain for full sequences
    /// </summary>
    private void ResetFullSequence()
    {
        chainQueue.Peek().Reset();
        currentCombo = null;
        anim.CrossFade(defaultAnim, transitionDuration, targetLayer);
    }

    /// <summary>
    /// reset the partial chain for partial sequences
    /// </summary>
    private void ResetPartialSequence()
    {
        currenLink = null;
        currentCombo = null;
        ignoreInput = false;

        if (chainQueue.Count != mainChain.Length)
        {
            Enumerable.All(mainChain, x => x.Reset());
            chainQueue = new Queue<ChainLink>(mainChain);
        }

        keys.Reset();

        //anim.CrossFade(defaultAnim, transitionDuration, targetLayer);
    }

    /// <summary>
    /// Check if the current state playing is our targetState and within time constrains
    /// </summary>
    /// <param name="stateHash">target state</param>
    /// <param name="minDuration">min normalised duration to consider</param>
    /// <param name="maxDuration">max normalised duration to consider</param>
    /// <returns></returns>
    public bool IsCurrentlyPlaying(int stateHash, float minDuration = 0f, float maxDuration = 1f)
    {
        float currentNormalisedTime = Mathf.Clamp01(anim.GetCurrentAnimatorStateInfo(targetLayer).normalizedTime);
        bool condition1 = anim.GetCurrentAnimatorStateInfo(targetLayer).shortNameHash == stateHash;
        bool condition2 = !condition1 || Utilities.InRange(currentNormalisedTime, minDuration, maxDuration);

        return condition1 && condition2;
    }
}
