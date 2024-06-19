#if UNITY_EDITOR
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
[ExecuteInEditMode]
public class WindowGridBrush : MonoBehaviour, ICustomHierarchyElement
{
    public Transform ground;
    public Vector2Int mapSize = Vector2Int.zero;

    public int[,] map;
    public int[,] grid = new int[0,0];

    public bool displayGrid;

    private void OnDrawGizmos()
    {
        //DrawGridInScene();
    }
    private void DrawGridInScene()
    {
        if (grid == null || mapSize == Vector2Int.zero) return;

        float cellSize = 1.0f; // 한 셀의 크기

        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                // 셀의 좌표 계산
                Vector3 cellPosition = transform.position + new Vector3(x * cellSize, 0, y * cellSize);
                Gizmos.color = grid[x, y] == 1 ? new Color(0,0,0,0.5f): new Color(1, 1, 1, 0.5f);

                // 셀을 그리기
                Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
            }
        }
    }
    public Color CustomHierarchyElementBackGroundColor()
    {
        return Color.black;
    }

    public GUIStyle CustomHierarchyElementStyle(ref GUIStyle InStyle)
    {
        InStyle.normal.textColor = Color.cyan;
        InStyle.alignment = TextAnchor.MiddleCenter;
        return InStyle;
    }
}
[CustomEditor(typeof(WindowGridBrush))]
public class WindowGridBrushCustomEditor : Editor
{
    private SerializedProperty ground;
    private SerializedProperty mapSize;
    private SerializedProperty grid;
    private SerializedProperty displayGrid;
    private WindowGridBrush script;

    void OnEnable()
    {
        script = (WindowGridBrush)target;
        ground = serializedObject.FindProperty("ground");
        mapSize = serializedObject.FindProperty("mapSize");
        grid = serializedObject.FindProperty("grid");
        displayGrid = serializedObject.FindProperty("displayGrid");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        if (GUILayout.Button("Open Grid Editor"))
        {
            if (Vector2Int.zero == mapSize.vector2IntValue)
            {
                mapSize.vector2IntValue = new Vector2Int(61, 61);
                script.map = new int[mapSize.vector2IntValue.x, mapSize.vector2IntValue.y];
                if (0 == script.grid.GetLength(0) && 0 == script.grid.GetLength(1))
                {
                    script.grid = new int[mapSize.vector2IntValue.x, mapSize.vector2IntValue.y];
                }
            }
            else
            {
                script.map = new int[mapSize.vector2IntValue.x, mapSize.vector2IntValue.y];
                if (0 == script.grid.GetLength(0) && 0 == script.grid.GetLength(1))
                {
                    script.grid = new int[mapSize.vector2IntValue.x, mapSize.vector2IntValue.y];
                }
            }
            GridEditorWindow.Open(script, this);
        }

        if (true == displayGrid.boolValue)
        {
            if (GUILayout.Button("Open Grid Editor For Debug"))
            {
                script.grid = new int[mapSize.vector2IntValue.x, mapSize.vector2IntValue.y];
                GridDebugEditorWindow.Open(script);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    public void OnGridApplyEvent()
    {
        // 그리드 크기에 맞춰서 텍스쳐 교체
        Debug.Log($"[OnGridApplyEvent] - ");
    }
}
#endif
