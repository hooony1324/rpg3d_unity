using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Effect))]
public class EffectEditor : IdentifiedObjectEditor
{

    private SerializedProperty typeProperty;
    private SerializedProperty isAllowDuplicateProperty;
    private SerializedProperty removeDuplicateTargetOptionProperty;

    private SerializedProperty isShowInUIProperty;

    private SerializedProperty isAllowLevelExceedDatasProperty;
    private SerializedProperty maxLevelProperty;
    private SerializedProperty effectDatasProperty;

    protected override void OnEnable()
    {
        base.OnEnable();

        typeProperty = serializedObject.FindProperty("type");
        isAllowDuplicateProperty = serializedObject.FindProperty("isAllowDuplicate");
        removeDuplicateTargetOptionProperty = serializedObject.FindProperty("removeDuplicateTargetOption");

        isShowInUIProperty = serializedObject.FindProperty("isShowInUI");

        isAllowLevelExceedDatasProperty = serializedObject.FindProperty("isAllowLevelExceedDatas");

        maxLevelProperty = serializedObject.FindProperty("maxLevel");
        effectDatasProperty = serializedObject.FindProperty("effectDatas");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        float prevLevelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 175f;

        DrawSettings();
        DrawOptions();
        DrawEffectDatas();

        EditorGUIUtility.labelWidth = prevLevelWidth;

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSettings()
    {
        if (!DrawFoldoutTitle("Setting"))
            return;

        // Enum을 Toolbar 형태로 그려줌
        CustomEditorUtility.DrawEnumToolbar(typeProperty);

        EditorGUILayout.Space();
        CustomEditorUtility.DrawUnderline();
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(isAllowDuplicateProperty);
        // 중복 적용 허용 Option이 true라면 중복 Effect를 지울 필요가 없으므로
        // removeDuplicateTargetOption 변수를 그리지 않음
        if (!isAllowDuplicateProperty.boolValue)
            CustomEditorUtility.DrawEnumToolbar(removeDuplicateTargetOptionProperty);
    }
    private void DrawOptions()
    {
        if (!DrawFoldoutTitle("Option"))
            return;

        EditorGUILayout.PropertyField(isShowInUIProperty);
    }

    private void DrawEffectDatas()
    {
        // Effect의 Data가 아무것도 존재하지 않으면 1개를 자동적으로 만들어줌
        if (effectDatasProperty.arraySize == 0)
        {
            // 배열 길이를 늘려서 새로운 Element를 생성
            effectDatasProperty.arraySize++;
            // 추가한 Data의 Level을 1로 설정
            effectDatasProperty.GetArrayElementAtIndex(0).FindPropertyRelative("level").intValue = 1;
        }

        if (!DrawFoldoutTitle("Data"))
            return;

        EditorGUILayout.PropertyField(isAllowLevelExceedDatasProperty);

        // Level상한 제한 있으면 설정한 값으로
        if (isAllowLevelExceedDatasProperty.boolValue)
            EditorGUILayout.PropertyField(maxLevelProperty);
        // 없으면 MaxLevel을 고정값으로
        else
        {
            // maxLevelProperty는 수정 못하게
            GUI.enabled = false;
            var lastEffectData = effectDatasProperty.GetArrayElementAtIndex(effectDatasProperty.arraySize - 1);

            // 고정값은 maxLevel을 마지막 Data의 Level
            maxLevelProperty.intValue = lastEffectData.FindPropertyRelative("level").intValue;

            EditorGUILayout.PropertyField(maxLevelProperty);
            GUI.enabled = true;
        }

        // effectDatas를 돌면서 GUI그림
        for (int i = 0; i < effectDatasProperty.arraySize; i++)
        {
            var property = effectDatasProperty.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("HelpBox");
            {
                // 첫 번째 데이터(지워지면 안됨)
                if (DrawRemovableLevelFoldout(effectDatasProperty, property, i, i != 0))
                {
                    EditorGUILayout.EndVertical();
                    break;
                }

                if (property.isExpanded)
                {
                    EditorGUI.indentLevel += 1;

                    var levelProperty = property.FindPropertyRelative("level");

                    // level프로퍼티 그림, level수정되면 EffectDatas오름차순 정렬
                    DrawSortedPropertiesByLevel(effectDatasProperty, levelProperty, i, i != 0);

                    var maxStackProperty = property.FindPropertyRelative("maxStack");
                    EditorGUILayout.PropertyField(maxStackProperty);
                    // maxStack은 1이 최소값 고정
                    maxStackProperty.intValue = Mathf.Max(maxStackProperty.intValue, 1);

                    // List +/- 누를 시 size 변경됨
                    var stackActionsProperty = property.FindPropertyRelative("stackActions");
                    var prevStackActionsSize = stackActionsProperty.arraySize;

                    EditorGUILayout.PropertyField(stackActionsProperty);

                    // stackActions에서 +눌러서 Action추가한 상황
                    // 1스택 Action 있는 상황, 2스택 Action을 추가했을 때 일단은 1스택 정보를 받아와서 편집하기 위함
                    if (stackActionsProperty.arraySize > prevStackActionsSize)
                    {
                        // 새로 추가된 Element가져옴
                        var lastStackActionProperty = stackActionsProperty.GetArrayElementAtIndex(prevStackActionsSize);

                        var actionProperty = lastStackActionProperty.FindPropertyRelative("action");
                        // Deep Copy
                        CustomEditorUtility.DeepCopySerializeReference(actionProperty);
                    }

                    // StackAction들의 stack 변수에 입력 가능한 최대 값을 MaxStack 값으로 제한
                    for (int stackActionIndex = 0; stackActionIndex < stackActionsProperty.arraySize; stackActionIndex++)
                    {
                        var stackActionProperty = stackActionsProperty.GetArrayElementAtIndex(stackActionIndex);

                        var stackProperty = stackActionProperty.FindPropertyRelative("stack");
                        // 1~MaxStack으로 값 제한
                        stackProperty.intValue = Mathf.Clamp(stackProperty.intValue, 1, maxStackProperty.intValue);
                    }

                    EditorGUILayout.PropertyField(property.FindPropertyRelative("action"));

                    EditorGUILayout.PropertyField(property.FindPropertyRelative("runningFinishOption"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("duration"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("applyCount"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("applyCycle"));

                    EditorGUILayout.PropertyField(property.FindPropertyRelative("customActions"));

                    // 들여쓰기 종료
                    EditorGUI.indentLevel -= 1;
                }

            }
            EditorGUILayout.EndVertical();
        }

        // EffectDatas에 새로운 Data를 추가하는 Button
        if (GUILayout.Button("Add New Level"))
        {
            // 이전 Element를 가져와서 새 Element에 정보 입력
            var lastArraySize = effectDatasProperty.arraySize++;
            var prevElementProperty = effectDatasProperty.GetArrayElementAtIndex(lastArraySize - 1);
            var newElementProperty = effectDatasProperty.GetArrayElementAtIndex(lastArraySize);
            var newElementLevel = prevElementProperty.FindPropertyRelative("level").intValue + 1;
            newElementProperty.FindPropertyRelative("level").intValue = newElementLevel;

            CustomEditorUtility.DeepCopySerializeReferenceArray(newElementProperty.FindPropertyRelative("stackActions"), "action");

            CustomEditorUtility.DeepCopySerializeReference(newElementProperty.FindPropertyRelative("action"));

            CustomEditorUtility.DeepCopySerializeReferenceArray(newElementProperty.FindPropertyRelative("customActions"));
        }
    }
}
