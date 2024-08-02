using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SkillSystemWindow : EditorWindow
{
    // 현재 보고있는 Database tab인덱스 (category, skill, effect, stat, ...)
    private static int toolbarIndex = 0;
    // 현재 Database List의 ScrollPosition저장용
    private static Dictionary<Type, Vector2> scrollPositionsByType = new();
    // 현재 보여주고 있는 data의 ScrollPosition
    private static Vector2 drawingEditorScrollPosition;
    // 현재 선택한 Data
    private static Dictionary<Type, IdentifiedObject> selectedObjectsByType = new();

    // Type별 Database
    private readonly Dictionary<Type, IODatabase> databasesByType = new();
    private Type[] databaseTypes;
    private string[] databaseTypeNames;

    // 현재 보여주고 있는 data의 Editor클래스
    private Editor cachedEditor;

    // Database List선택 시 배경
    private Texture2D selectedBoxTexture;
    private GUIStyle selectedBoxStyle;

    [MenuItem("Tools/Skill System")]
    private static void OpenWindow()
    {
        var window = GetWindow<SkillSystemWindow>("Skill System");
        window.minSize = new Vector2(800, 700);
        window.Show();
    }

    private void SetupStyle()
    {
        selectedBoxTexture = new Texture2D(1, 1);
        selectedBoxTexture.SetPixel(0, 0, new Color(0.31f, 0.40f, 0.50f));
        // 위에서 설정한 Color값을 실제로 적용함
        selectedBoxTexture.Apply();
        // 이 Texture는 Window에서 관리할 것이기 때문에 Unity에서 자동 관리하지말라(DontSave) Flag를 설정해줌
        // 이 flag가 없다면 Editor에서 Play를 누른채로 SetupStyle 함수가 실행되면
        // texture가 Play 상태에 종속되어 Play를 중지하면 texture가 자동 Destroy되버림
        selectedBoxTexture.hideFlags = HideFlags.DontSave;

        selectedBoxStyle = new GUIStyle();
        // Normal상태의 Backgorund Texture를 위 Texture로 설정해줌으로써 이 Style을 쓰는 GUI는 Background가 청색으로 나옴
        selectedBoxStyle.normal.background = selectedBoxTexture;
    }

    private void SetupDatabases(Type[] dataTypes)
    {
        if (databasesByType.Count == 0)
        {
            // Resources Folder에 Database Folder가 있는지 확인
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Database"))
            {
                // 없다면 Database Folder를 만들어줌
                AssetDatabase.CreateFolder("Assets/Resources", "Database");
            }
        }

        foreach (var type in dataTypes)
        {
            var database = AssetDatabase.LoadAssetAtPath<IODatabase>($"Assets/Resources/Database/{type.Name}Database.asset");

            if (database == null)
            {
                database = CreateInstance<IODatabase>();
                AssetDatabase.CreateAsset(database, $"Assets/Resources/Database/{type.Name}Database.asset");
                // 지정한 주소의 하위 Folder를 생성, 이 Folder는 Window에 의해 생성된 IdentifiedObject가 저장될 장소임
                
                if (!AssetDatabase.IsValidFolder($"Assets/Resources/{type.Name}"))
                {
                    AssetDatabase.CreateFolder("Asset/Resources", type.Name);
                }
            }

            // 불러온 Database세팅
            databasesByType[type] = database;
            scrollPositionsByType[type] = Vector2.zero;
            selectedObjectsByType[type] = null;
        }

        databaseTypeNames = dataTypes.Select(x => x.Name).ToArray();
        databaseTypes = dataTypes;
    }

    private void OnEnable()
    {
        SetupStyle();
        SetupDatabases(new[] { typeof(Category), typeof(Stat), typeof(Effect),/*typeof(Skill), typeof(SkillTree) */});
    }

    private void OnDisable()
    {
        DestroyImmediate(cachedEditor);
        DestroyImmediate(selectedBoxTexture);
    }

    private void OnGUI()
    {
        // Database들이 관리 중인 IdentifiedObject들의 Type Name으로 Toolbar를 그려줌
        toolbarIndex = GUILayout.Toolbar(toolbarIndex, databaseTypeNames);
        EditorGUILayout.Space(4f);
        CustomEditorUtility.DrawUnderline();
        EditorGUILayout.Space(4f);

        DrawDatabase(databaseTypes[toolbarIndex]);
    }

    void DrawDatabase(Type dataType)
    {
        var database = databasesByType[dataType];

        // Editor에 Caching되는 PreviewTexture의 수를 최소 32개, 최대 Database의 Count까지 늘림
        // Window에서 보여지는 Icon이 늘어나면 안보이는 문제 발생할 수 있음
        AssetPreview.SetPreviewTextureCacheSize(Mathf.Max(32, 32 + database.Count));

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300f));
            {
                /**
                 * Button - [New DataType]
                 */
                GUI.color = Color.green;
                if (GUILayout.Button($"New {dataType.Name}"))
                {
                    var guid = Guid.NewGuid();
                    var newData = CreateInstance(dataType) as IdentifiedObject;

                    // Reflection을 이용해 codeName Field를 찾아와서 newData의 codeName을 임시 codeName인 guid로 set
                    dataType.BaseType.GetField("codeName", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(newData, guid.ToString());

                    // Asset폴더에 newData저장(ScriptableObject)
                    AssetDatabase.CreateAsset(newData, $"Assets/Resources/{dataType.Name}/{dataType.Name.ToUpper()}_{guid}.asset");

                    database.Add(newData);

                    // IODatabase의 멤버 중 Serialize된 변수인 'datas'에 변화를 알림
                    EditorUtility.SetDirty(database);
                    // Dirty Flag대상을 저장
                    AssetDatabase.SaveAssets();


                    selectedObjectsByType[dataType] = newData;
                }

                /**
                 * Button - [Remove Last DataType]
                 */
                GUI.color = Color.red;
                if (GUILayout.Button($"Remove Last {dataType.Name}"))
                {
                    var lastData = database.Count > 0 ? database.Datas.Last() : null;
                    if (lastData)
                    {
                        database.Remove(lastData);

                        // Data의 Asset폴더 내 위치 가져와서 삭제
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(lastData));
                        EditorUtility.SetDirty(database);
                        AssetDatabase.SaveAssets();
                    }
                }

                /**
                 * Button - [Sort By Name]
                 */
                GUI.color = Color.cyan;
                if (GUILayout.Button("Sort By Name"))
                {
                    database.SortByCodeName();
                    EditorUtility.SetDirty(database);
                    AssetDatabase.SaveAssets();
                }

                /**
                 * ScrollView - Databases
                 */
                GUI.color = Color.white;
                EditorGUILayout.Space(2f);
                CustomEditorUtility.DrawUnderline();
                EditorGUILayout.Space(4f);

                scrollPositionsByType[dataType] = EditorGUILayout.BeginScrollView(scrollPositionsByType[dataType], false, true,
    GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
                {
                    foreach (var data in database.Datas)
                    {
                        // CodeName Indent정도, 아이콘 있으면 넓게
                        float labelWidth = data.Icon != null ? 200f : 245f;

                        // 선택된 데이터는 파란색 배경
                        var style = selectedObjectsByType[dataType] == data ? selectedBoxStyle : GUIStyle.none;

                        EditorGUILayout.BeginHorizontal(style, GUILayout.Height(40f));
                        {
                            /** 
                             * Preview - [Icon]
                             */
                            if (data.Icon)
                            {
                                // 한번 가져온 Texture는 Unity내부에 Caching됨
                                // Cache한계를 넘어가면 오래된 Texture부터 지워짐
                                var preview = AssetPreview.GetAssetPreview(data.Icon);
                                GUILayout.Label(preview, GUILayout.Height(40f), GUILayout.Width(40f));
                            }

                            /**
                             * Label - [CodeName]
                             */
                            EditorGUILayout.LabelField(data.CodeName, GUILayout.Width(labelWidth), GUILayout.Height(40f));

                            /**
                             * Button - [X]
                             */
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.Space(10f);
                                GUI.color = Color.red;
                                if (GUILayout.Button("X", GUILayout.Width(20f)))
                                {
                                    database.Remove(data);
                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(data));
                                    EditorUtility.SetDirty(database);
                                    AssetDatabase.SaveAssets();
                                }
                            }
                            EditorGUILayout.EndVertical();

                            GUI.color = Color.white;
                        }
                        EditorGUILayout.EndHorizontal();

                        // data가 삭제되었으면 즉시 Database목록을 그리는걸 멈추고 빠져나옴
                        if (data == null)
                            break;

                        // 마지막으로 그린 Layout, 현재시점 BeginHorizontal 부분
                        var lastRect = GUILayoutUtility.GetLastRect();
                        // MouseEvent가 lastRect내부에 있다면 Database를 선택한 것
                        if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
                        {
                            selectedObjectsByType[dataType] = data;
                            drawingEditorScrollPosition = Vector2.zero;
                            // Event에 대한 처리를 했다고 Unity에 알림
                            Event.current.Use();
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();

            // 선택된 Data가 존재한다면 Editor의 메인화면에 그려줌
            if (selectedObjectsByType[dataType])
            {
                drawingEditorScrollPosition = EditorGUILayout.BeginScrollView(drawingEditorScrollPosition);
                {
                    // selectedObjectsByType[dataType]: Editor를 만들 Target
                    // null: Target의 타입, null이면 Target의 기본Type
                    // cachedEditor: 내부에서 만들어진 Editor를 담는 변수
                    Editor.CreateCachedEditor(selectedObjectsByType[dataType], null, ref cachedEditor);
                    cachedEditor.OnInspectorGUI();
                }
                EditorGUILayout.EndScrollView();
            }
            
        }
        EditorGUILayout.EndHorizontal();
    }
}
