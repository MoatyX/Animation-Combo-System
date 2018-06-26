using UnityEngine;

public class Disarm : StateMachineBehaviour
{
    [Range(0f, 1f)] public float time;
    [Range(0f, 1f)] public float unparentTime;
    private Transform axe;
    private Transform spine;

    private void OnEnable()
    {
        axe = GameObject.FindGameObjectWithTag("Axe").transform;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (!axe) return;
        if (animatorStateInfo.normalizedTime >= time)
        {
            animator.SetLayerWeight(1, 1);
        }

        if (animatorStateInfo.normalizedTime >= unparentTime)
        {
            if (!spine) spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            axe.SetParent(spine);
        }

    }
}
