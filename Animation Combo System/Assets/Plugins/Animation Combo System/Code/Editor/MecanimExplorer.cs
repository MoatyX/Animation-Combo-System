using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class MecanimExplorer
{
    public List<int> mStates;
    public List<string> mStateNames;
    public Dictionary<int, int> mStateIndexLookup;
    public Dictionary<int, int> mStateParentLookup;

    public MecanimExplorer(AnimatorController animator, ushort layer)
    {
        mStates = new List<int>();
        mStateNames = new List<string>();
        mStateIndexLookup = new Dictionary<int, int>();
        mStateParentLookup = new Dictionary<int, int>();

        mStateIndexLookup[0] = mStates.Count;
        mStateNames.Add("(default)");
        mStates.Add(0);

        if (animator != null && layer < animator.layers.Length)
        {
            AnimatorStateMachine fsm = animator.layers[layer].stateMachine;
            string name = fsm.name;
            int hash = Animator.StringToHash(name);

            CollectStatesFromFSM(fsm, name + ".", hash, string.Empty);
        }
    }

    int AddState(string hashName, int parentHash, string displayName)
    {
        int hash = Animator.StringToHash(hashName);
        if (parentHash != 0)
            mStateParentLookup[hash] = parentHash;
        mStateIndexLookup[hash] = mStates.Count;
        mStateNames.Add(displayName);
        mStates.Add(hash);
        return hash;
    }

    List<string> CollectClipNames(Motion motion)
    {
        List<string> names = new List<string>();
        AnimationClip clip = motion as AnimationClip;
        if (clip != null)
            names.Add(clip.name);
        BlendTree tree = motion as BlendTree;
        if (tree != null)
        {
            ChildMotion[] children = tree.children;
            foreach (var child in children)
                names.AddRange(CollectClipNames(child.motion));
        }
        return names;
    }

    private void CollectStatesFromFSM(AnimatorStateMachine fsm, string hashPrefix, int parentHash, string displayPrefix)
    {
        ChildAnimatorState[] states = fsm.states;
        for (int i = 0; i < states.Length; i++)
        {
            AnimatorState state = states[i].state;
            int hash = AddState(hashPrefix + state.name, parentHash, displayPrefix + state.name);

            // Also process clips as pseudo-states, if more than 1 is present.
            // Since they don't have hashes, we can manufacture some.
            List<string> clips = CollectClipNames(state.motion);
            if (clips.Count > 1)
            {
                string substatePrefix = displayPrefix + state.name + ".";
                foreach (string name in clips)
                    AddState(
                        CreateFakeHashName(hash, name),
                        hash, substatePrefix + name);
            }
        }
    }

    public static string CreateFakeHashName(int parentHash, string stateName)
    { return parentHash + "_" + stateName; }
}
