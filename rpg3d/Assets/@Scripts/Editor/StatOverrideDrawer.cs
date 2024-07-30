using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StatOverride))]
public class StatOverrideDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var statProperty = property.FindPropertyRelative("stat");

        var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        string labelName = statProperty.objectReferenceValue?.name.Replace("STAT_", "") ?? label.text;

        property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, labelName);
        if (property.isExpanded)
        {
            var boxRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width,
                GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight);
            EditorGUI.HelpBox(boxRect, "", MessageType.None);

            var propertyRect = new Rect(boxRect.x + 4f, boxRect.y + 2f, boxRect.width - 8f, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(propertyRect, property.FindPropertyRelative("stat"));

            propertyRect.y += EditorGUIUtility.singleLineHeight;
            var isUseOverrideProperty = property.FindPropertyRelative("isUseOverride");
            EditorGUI.PropertyField(propertyRect, isUseOverrideProperty);

            if (isUseOverrideProperty.boolValue)
            {
                propertyRect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(propertyRect, property.FindPropertyRelative("overrideDefaultValue"));
            }
        }

        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;
        else
        {
            bool isUseOverride = property.FindPropertyRelative("isUseOverride").boolValue;
            int propertyLine = isUseOverride ? 4 : 3;
            return (EditorGUIUtility.singleLineHeight * propertyLine) + propertyLine;
        }
    }

}
