using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;


[CustomEditor(typeof(Test))]
public class TestAnim : Editor
{
    private string[] _targetStateNames;
    private string[] _layerNames;
    private Test Target { get { return target as Test;} }

    public int current;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpdateTargetStates();
        current = EditorGUILayout.Popup(current, _targetStateNames);

        Debug.Log(Animator.StringToHash(_targetStateNames[current]));
        Target.FullHash = Animator.StringToHash(_targetStateNames[current]);
    }

    static AnimatorController GetControllerFromAnimator(Animator animator)
    {
        if (animator == null)
            return null;
        var ovr = animator.runtimeAnimatorController as AnimatorOverrideController;
        if (ovr)
            return ovr.runtimeAnimatorController as AnimatorController;
        return animator.runtimeAnimatorController as AnimatorController;
    }

    private void UpdateTargetStates()
    {
        // Scrape the Animator Controller for states
        AnimatorController ac = GetControllerFromAnimator(Target.anim);
        MecanimExplorer collector = new MecanimExplorer(ac, Target.layer);

        _targetStateNames = collector.mStateNames.ToArray();

        if (ac == null)
            _layerNames = new string[0];
        else
        {
            _layerNames = new string[ac.layers.Length];
            for (int i = 0; i < ac.layers.Length; ++i)
                _layerNames[i] = ac.layers[i].name;
        }
    }
}
