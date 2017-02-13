using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TutorialAnimation))]
public class TutorialActionEditor : Editor
{
    private SerializedObject m_object;

    public void OnEnable()
    {
        m_object = new SerializedObject(target);
    }

    public override void OnInspectorGUI()
    {
        m_object.Update();

        TutorialAnimation tutorialAnimation = (TutorialAnimation)target;
        EditorGUILayout.PropertyField(m_object.FindProperty("Type"));

        EditorGUILayout.PropertyField(m_object.FindProperty("Visual"), true);

        if (tutorialAnimation.Type == TutorialAnimationActionType.Movemenet)
        {
            EditorGUILayout.PropertyField(m_object.FindProperty("MoveAction"), true);
        }
        else if (tutorialAnimation.Type == TutorialAnimationActionType.Static)
        {
            EditorGUILayout.PropertyField(m_object.FindProperty("StaticAction"), true);
        }

        m_object.ApplyModifiedProperties();
    }
}
