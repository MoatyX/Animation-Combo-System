using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Generics.Utilities
{
    [CustomPropertyDrawer(typeof(AnimState))]
    public class AnimStateDrawer : PropertyDrawer
    {
        private int _current;
        private AnimatorController _ac;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            //current = EditorGUILayout.Popup(current, new[] {"11", "22"});
            _ac = EditorGUILayout.ObjectField(new GUIContent("Controller"), _ac, typeof(AnimatorController), false) as AnimatorController;
        }

        private void UpdateAnimatorStates()
        {
           
        }
    }
}
