using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Drawing;
using FontStyle = UnityEngine.FontStyle;
using PlasticPipe.PlasticProtocol.Messages;
using Color = UnityEngine.Color;

public static class CustomEditorUtility
{
    private readonly static GUIStyle titleStyle;

    static CustomEditorUtility()
    {
        titleStyle = new GUIStyle("ShurikenModuleTitle")
        {
            // 유니티 Default Label의 font를 가져옴
            font = new GUIStyle(EditorStyles.label).font,
            fontStyle = FontStyle.Bold,
            fontSize = 14,
            // title을 그릴 상하좌우 공간에 여유를 줌
            border = new RectOffset(15, 7, 4, 4),
            // 높이는 26
            fixedHeight = 26f,
            // 내부 Text의 위치를 조절함
            contentOffset = new Vector2(20f, -2f)
        };
    }

    public static bool DrawFoldoutTitle(string title, bool isExpanded, float space = 15f)
    {
        EditorGUILayout.Space(space);

        // FoldoutTitle박스 titleSytle대로 그림
        var rect = GUILayoutUtility.GetRect(16f, titleStyle.fixedHeight, titleStyle);
        GUI.Box(rect, title, titleStyle);

        // 현재 Editor의 Event
        // - 마우스 입력, GUI Repaint, 키보드 입력 등
        var currentEvent = Event.current;
        var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);

        if (currentEvent.type == EventType.Repaint)
            EditorStyles.foldout.Draw(toggleRect, false, false, isExpanded, false);
        else if (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition))
        {
            // 토글
            isExpanded = !isExpanded;
            // event처리 했음을 Unity에 알려줌
            currentEvent.Use();
        }

        return isExpanded;
    }

    public static bool DrawFoldoutTitle(IDictionary<string, bool> isFoldoutExpandedesByTitle, string title, float space = 15f)
    {
        if (!isFoldoutExpandedesByTitle.ContainsKey(title))
            isFoldoutExpandedesByTitle[title] = true;

        isFoldoutExpandedesByTitle[title] = DrawFoldoutTitle(title, isFoldoutExpandedesByTitle[title], space);
        return isFoldoutExpandedesByTitle[title];
    }

    public static void DrawUnderline(float height = 1f)
    {
        // 마지막으로 그린 GUI의 위치와 크기 정보를 가진 Rect 구조체를 가져옴
        var lastRect = GUILayoutUtility.GetLastRect();
        // Rect 구조체를 indent(=들여쓰기)가 적용된 값으로 변환함
        lastRect = EditorGUI.IndentedRect(lastRect);
        // rect의 y값을 이전 GUI의 높이만큼 내림(=즉, y값은 이전 GUI 바로 아래에 위치하게 됨)
        lastRect.y += lastRect.height;
        lastRect.height = height;
        // rect 값을 이용해서 지정된 위치에 height크기의 Box를 그림
        // height가 1이라면 이전 GUI 바로 아래에 크기가 1인 Box, 즉 Line이 그려지게됨
        EditorGUI.DrawRect(lastRect, Color.gray);
    }
}
