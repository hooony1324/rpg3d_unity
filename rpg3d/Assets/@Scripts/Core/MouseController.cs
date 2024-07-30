using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CursorType { Default, BlueArrow }
public class MouseController : MonoSingleton<MouseController>
{
    public delegate void ClickedHandler(Vector2 mousePosition);

    [System.Serializable]
    private struct CursorData
    {
        public CursorType type;
        public Texture2D texture;
    }

    [SerializeField]
    private CursorData[] cursorDatas;

    public event ClickedHandler onLeftClicked;
    public event ClickedHandler onRightClicked;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            onLeftClicked?.Invoke(Input.mousePosition);
        else if (Input.GetMouseButtonDown(1))
            onRightClicked?.Invoke(Input.mousePosition);
    }

    public void ChangeCursor(CursorType newType)
    {
        if (newType == CursorType.Default)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        else
        {
            var cursorTexture = cursorDatas.First(x => x.type == newType).texture;
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }
    }
}
