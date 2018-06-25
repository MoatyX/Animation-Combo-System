using UnityEngine;

public class Arm : StateMachineBehaviour
{
    [Range(0f, 1f)] public float time;
    [Range(0f, 1f)] public float unparentTime;

    private Transform axe;
    private Transform hand;

    private static Vector3 pos = new Vector3(0.0988f, -0.0468f, 0.148f);

    private void OnEnable()
    {
        axe = GameObject.FindGameObjectWithTag("Axe").transform;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (!axe) return;
        if (animatorStateInfo.normalizedTime >= time)
        {
            animator.SetLayerWeight(1, 0);
        }

        if (animatorStateInfo.normalizedTime >= unparentTime)
        {
            if (!hand) hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            axe.SetParent(hand);
            axe.localPosition = pos;
        }
    }
}
