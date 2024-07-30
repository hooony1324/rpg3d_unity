using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Stat))]
public class StatEditor : IdentifiedObjectEditor
{
    private SerializedProperty isPercentTypeProperty;
    private SerializedProperty maxValueProperty;
    private SerializedProperty minValueProperty;
    private SerializedProperty defaultValueProperty;

    protected override void OnEnable()
    {
        base.OnEnable();

        isPercentTypeProperty = serializedObject.FindProperty("isPercentType");
        maxValueProperty = serializedObject.FindProperty("maxValue");
        minValueProperty = serializedObject.FindProperty("minValue");
        defaultValueProperty = serializedObject.FindProperty("defaultValue");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        if (DrawFoldoutTitle("Setting"))
        {
            EditorGUILayout.PropertyField(isPercentTypeProperty);
            EditorGUILayout.PropertyField(maxValueProperty);
            EditorGUILayout.PropertyField(minValueProperty);
            EditorGUILayout.PropertyField(defaultValueProperty);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
