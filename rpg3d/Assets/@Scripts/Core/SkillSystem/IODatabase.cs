using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "IODatabase")]
public class IODatabase : ScriptableObject
{
    [SerializeField]
    private List<IdentifiedObject> datas = new();

    public IReadOnlyList<IdentifiedObject> Datas => datas;
    public int Count => datas.Count;

    public IdentifiedObject this[int index] => datas[index];


    private void SetID(IdentifiedObject target, int id)
    {
        // BindingFlags
        // NonPublic: 한정자 Public이면 안됨, Instance: static type이 아니여야 함
        var field = typeof(IdentifiedObject).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);

        field.SetValue(target, id);

#if UNITY_EDITOR
        // 코드로 Serialize변수를 수정 시, 수정되었음을 Unity에 알림
        // 당장 저장되지는 않고, EditorCode에서 ApplyModifiedProperties 혹은 AssetDatabase.SaveAssets가 호출되야 저장됨
        EditorUtility.SetDirty(target);
#endif
    }

    private void ReorderDatas()
    {
        var field = typeof(IdentifiedObject).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i = 0; i < datas.Count; i++)
        {
            field.SetValue(datas[i], i);
#if UNITY_EDITOR
            EditorUtility.SetDirty(datas[i]);
#endif
        }
    }

    public void Add(IdentifiedObject newData)
    {
        datas.Add(newData);
        SetID(newData, datas.Count - 1);
    }
    public void Remove(IdentifiedObject data)
    {
        datas.Remove(data);
        ReorderDatas();
    }

    public IdentifiedObject GetDataByID(int id) => datas[id];
    public T GetDataByID<T>(int id) where T : IdentifiedObject => GetDataByID(id) as T;

    public IdentifiedObject GetDataCodeName(string codeName) => datas.Find(item => item.CodeName == codeName);
    public T GetDataCodeName<T>(string codeName) where T : IdentifiedObject => GetDataCodeName(codeName) as T;

    public bool Contains(IdentifiedObject item) => datas.Contains(item);

    public void SortByCodeName()
    {
        datas.Sort((x, y) => x.CodeName.CompareTo(y.CodeName));
        ReorderDatas();
    }
}
