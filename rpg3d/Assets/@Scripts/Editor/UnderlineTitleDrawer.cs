using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UnderlineTitleAttribute))]
public class UnderlineTitleDrawer : DecoratorDrawer
{
    public override void OnGUI(Rect position)
    {
        var attributeAsUnderlineTitle = attribute as UnderlineTitleAttribute;

        position = EditorGUI.IndentedRect(position);
        position.height = EditorGUIUtility.singleLineHeight;
        position.y += attributeAsUnderlineTitle.Space;

        GUI.Label(position, attributeAsUnderlineTitle.Title, EditorStyles.boldLabel);

        // 한줄 이동
        position.y += EditorGUIUtility.singleLineHeight;
        // 두께는 1, 회색 선
        position.height = 1f;
        EditorGUI.DrawRect(position, Color.gray);
    }

    // GUI의 총 높이를 반환하는 함수
    public override float GetHeight()
    {
        var attributeAsUnderlineTitle = attribute as UnderlineTitleAttribute;
        // 기본 GUI 높이 + (기본 GUI 간격 * 2) + 설정한 Attribute Space 
        return attributeAsUnderlineTitle.Space + EditorGUIUtility.singleLineHeight + (EditorGUIUtility.standardVerticalSpacing * 2);
    }
}
