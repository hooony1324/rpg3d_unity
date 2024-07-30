using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(IdentifiedObject), true)]
public class IdentifiedObjectEditor : Editor
{
    // [SerialzieField]로 Serialize된 변수들에 접근
    private SerializedProperty categoriesProperty;
    private SerializedProperty iconProperty;
    private SerializedProperty idProperty;
    private SerializedProperty codeNameProperty;
    private SerializedProperty displayNameProperty;
    private SerializedProperty descriptionProperty;

    private ReorderableList categories;

    private GUIStyle textAreaStyle;

    // Title별로 Foldout여부 관리
    private readonly Dictionary<string, bool> isFoldoutExpandedesByTitle = new();

    protected virtual void OnEnable()
    {
        // Inspector1 -> Inspector2로 넘어가는 경우
        // 포커스가 풀리지 않는 부분을 풀어주는 용도
        GUIUtility.keyboardControl = 0;

        categoriesProperty = serializedObject.FindProperty("categories");
        iconProperty = serializedObject.FindProperty("icon");
        idProperty = serializedObject.FindProperty("id");
        codeNameProperty = serializedObject.FindProperty("codeName");
        displayNameProperty = serializedObject.FindProperty("displayName");
        descriptionProperty = serializedObject.FindProperty("description");

        // target: Editor에서 현재 보고있는 IdentifiedObject객체를 가져옴
        // var identifiedObject = target as IdentifiedObject
        // var identifiedObject = serializedObject.targetObject as IdentifiedObject;

        categories = new(serializedObject, categoriesProperty);

        categories.drawHeaderCallback = rect => EditorGUI.LabelField(rect, categoriesProperty.displayName);

        categories.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            rect = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rect, categoriesProperty.GetArrayElementAtIndex(index), GUIContent.none);
        };
    }
    protected bool DrawFoldoutTitle(string text)
    => CustomEditorUtility.DrawFoldoutTitle(isFoldoutExpandedesByTitle, text);

    private void StyleSetup()
    {
        if (textAreaStyle == null)
        {
            textAreaStyle = new(EditorStyles.textArea);
            textAreaStyle.wordWrap = true;
        }
    }

    public override void OnInspectorGUI()
    {
        StyleSetup();

        // 객체의 Serialize 변수들의 값을 업데이트함
        serializedObject.Update();

        categories.DoLayoutList();

        if (DrawFoldoutTitle("Information"))
        {
            EditorGUILayout.BeginHorizontal("HelpBox");
            {
                iconProperty.objectReferenceValue = EditorGUILayout.ObjectField(GUIContent.none, iconProperty.objectReferenceValue, typeof(Sprite), false, GUILayout.Width(65));

                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        // 변수 편집 Disable, ID는 자동설정 함
                        GUI.enabled = false;
                        // 변수의 선행 명칭(prefix) 지정
                        EditorGUILayout.PrefixLabel("ID");
                        // id변수는 그리되 prefix는 그리지 않음(=GUIContgent.none)
                        EditorGUILayout.PropertyField(idProperty, GUIContent.none);

                        GUI.enabled = true;
                    }

                    EditorGUILayout.EndHorizontal();

                    // 엔터 누르면 에셋이름 리셋
                    EditorGUI.BeginChangeCheck();
                    var prevCodeName = codeNameProperty.stringValue;

                    EditorGUILayout.DelayedTextField(codeNameProperty);
                    if (EditorGUI.EndChangeCheck())
                    {
                        var assetPath = AssetDatabase.GetAssetPath(target);
                        var newName = $"{target.GetType().Name.ToUpper()}_{codeNameProperty.stringValue}";

                        // serialized값의 변화를 적용(저장), 하지 않으면 이전 값으로 돌아감
                        serializedObject.ApplyModifiedProperties();

                        // project view에서 보이는 이름을 수정함, 같은 이름 가진 객체 있으면 실패
                        var message = AssetDatabase.RenameAsset(assetPath, newName);

                        // 외부 이름, 내부 이름 일치시켜 주어야 함
                        if (string.IsNullOrEmpty(message))
                            target.name = newName;
                        else
                            codeNameProperty.stringValue = prevCodeName;
                    }

                    // displayName 변수 그려줌
                    EditorGUILayout.PropertyField(displayNameProperty);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("HelpBox");
            {
                EditorGUILayout.LabelField("Description");

                descriptionProperty.stringValue = EditorGUILayout.TextArea(
                    descriptionProperty.stringValue,
                    textAreaStyle, GUILayout.Height(60));
            }
            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
