#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public class GridDebugEditorWindow : EditorWindow
{
    private WindowGridBrush script;
    private int[,] grid;
    private int rows;
    private int columns;
    private Vector2 scrollPosition; // ��ũ�Ѻ� ������
    public static void Open(WindowGridBrush script)
    {
        GridDebugEditorWindow window = GetWindow<GridDebugEditorWindow>("Grid Editor");

        window.script = script;
        window.grid = script.grid;
        window.rows = script.grid.GetLength(0);
        window.columns = script.grid.GetLength(1);

        window.Show();
    }

    private void OnGUI()
    {
        // ������ ũ�⿡ ���� �� ũ�� �������� ���
        float availableWidth = position.width - 40;
        float availableHeight = position.height - 100;
        float cellSize = Mathf.FloorToInt(Mathf.Min(availableWidth / columns, availableHeight / rows));

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - 50));

        // Display grid values in editor
        if (script.displayGrid && script.grid != null)
        {
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);

            for (int y = 0; y < rows; y++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < columns; x++)
                {
                    script.grid[x, y] = EditorGUILayout.IntField(script.grid[x, y], GUILayout.Width(cellSize));
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
    }
}
public class GridEditorWindow : EditorWindow
{
    enum ToolType
    {
        Brush,
        Eraser
    }
    private WindowGridBrush script;
    private WindowGridBrushCustomEditor customEditor;
    private int[,] grid;
    private int rows;
    private int columns;

    // tool ���� property
    private static int toolSize = 1; // �ʱ� �� ũ��
    private static ToolType toolType = ToolType.Brush;

    private int cellSize; // �������� ���� �׸��� �� ũ��
    private bool isDragging = false;
    private Vector2 startDragPosition;
    private Vector2 scrollPosition; // ��ũ�Ѻ� ������
    float toolbarHeight = 60f;
    public static void Open(WindowGridBrush script, WindowGridBrushCustomEditor customEditor)
    {
        GridEditorWindow window = GetWindow<GridEditorWindow>("Grid Editor");

        window.script = script;
        window.customEditor = customEditor;
        window.grid = script.grid;
        window.rows = script.grid.GetLength(0);
        window.columns = script.grid.GetLength(1);
        window.Initialize();

        window.Show();
    }

    private void Initialize()
    {
        toolType = ToolType.Brush;
    }

    private void OnGUI()
    {
        // ��� ���� �� �׸���
        GUILayout.BeginArea(new Rect(0, 0, position.width, 60), GUI.skin.box);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // ��� ���м�
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Tool type :", GUILayout.Width(100)); // �� �߰�
        if (GUILayout.Button(toolType.ToString(), GUILayout.Width(100)))
        {
            switch (toolType)
            {
                case ToolType.Brush:
                    toolType = ToolType.Eraser;
                    break;
                case ToolType.Eraser:
                    toolType = ToolType.Brush;
                    break;
            }
        }
        GUILayout.FlexibleSpace();

        EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(4)); // ���� ���м�
        GUILayout.Label("Brush Size:", GUILayout.Width(80));
        toolSize = EditorGUILayout.IntSlider(toolSize, 1, Mathf.Min(rows, columns), GUILayout.Width(200));

        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(4)); // ���� ���м�

        GUILayout.Label($"Grid Size [{columns},{rows}]", GUILayout.Width(150)); // �� �߰�

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // �ϴ� ���м�
        GUILayout.EndArea();

        // ��ũ�Ѻ� ����
        float toolbarHeight = 60f;
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - toolbarHeight * 2));

        // ������ ũ�⿡ ���� �� ũ�� �������� ���
        float availableWidth = position.width - 40;
        float availableHeight = position.height - 100 - toolbarHeight * 2; // ���� �� ���� ���̸� ��
        cellSize = Mathf.FloorToInt(Mathf.Min(availableWidth / columns, availableHeight / rows));

        // �׸����� �߾� ��ġ
        float gridWidth = columns * cellSize;
        float gridHeight = rows * cellSize;
        float startX = (position.width - gridWidth) / 2;
        float startY = (position.height - gridHeight) / 2 + 20; // +20 for top padding

        // �׸��� �׸���
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        DrawGrid(new Vector2(startX, startY));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // ��ũ�Ѻ� ��
        EditorGUILayout.EndScrollView();

        // �巡�� �̺�Ʈ ó��
        HandleDragEvents(startX, startY);

        // ���̵� ǥ��
        HandleGuide(startX, startY);

        // �ϴ� ���� �� �׸���
        GUILayout.BeginArea(new Rect(0, position.height - toolbarHeight, position.width, toolbarHeight), GUI.skin.box);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // ��� ���м�
        EditorGUILayout.BeginHorizontal();

        // clear ��ư
        EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(4)); // ���� ���м�
        if (GUILayout.Button("Grid Clear", GUILayout.Width(100)))
        {
            for (int i = 0; i < rows; i++)
            {
                for (int k = 0; k < columns; k++)
                {
                    grid[i, k] = 0;
                }
            }
        }
        GUILayout.FlexibleSpace();
        // tool Clear
        EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(4)); // ���� ���м�
        if (GUILayout.Button("Tool Reset", GUILayout.Width(100)))
        {
            toolSize = 1; // �ʱ� �� ũ��
            toolType = ToolType.Brush;
        }
        GUILayout.FlexibleSpace();

        // Apply ��ư
        EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(4)); // ���� ���м�
        if (GUILayout.Button("Apply", GUILayout.Width(100)))
        {
            customEditor.OnGridApplyEvent();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // �ϴ� ���м�
        GUILayout.EndArea();
    }

    void DrawGrid(Vector2 startCoord)
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Rect cellRect = new Rect(startCoord.x + x * cellSize, startCoord.y + y * cellSize, cellSize, cellSize);

                Color originalBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = grid[x, y] == 1 ? Color.cyan : Color.gray;

                EditorGUI.DrawRect(cellRect, GUI.backgroundColor);
                Handles.color = Color.black;
                Handles.DrawLine(new Vector3(cellRect.xMin, cellRect.yMin), new Vector3(cellRect.xMax, cellRect.yMin));
                Handles.DrawLine(new Vector3(cellRect.xMin, cellRect.yMin), new Vector3(cellRect.xMin, cellRect.yMax));
                Handles.DrawLine(new Vector3(cellRect.xMax, cellRect.yMin), new Vector3(cellRect.xMax, cellRect.yMax));
                Handles.DrawLine(new Vector3(cellRect.xMin, cellRect.yMax), new Vector3(cellRect.xMax, cellRect.yMax));

                GUI.backgroundColor = originalBackgroundColor;

                // Ŭ�� �̺�Ʈ ó��
                ClickEvent(new Vector2Int(x, y), cellRect);
            }
        }
    }

    private void ClickEvent(Vector2Int coord, Rect cellRect)
    {
        // Ŭ�� �̺�Ʈ ó��
        if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
        {
            isDragging = true;
            ApplyBrush(coord.x, coord.y);
            startDragPosition = Event.current.mousePosition;
            Event.current.Use();
        }
    }

    private void HandleGuide(float startX, float startY)
    {
        Event e = Event.current;
        if (e.pointerType == PointerType.Mouse)
        {
            Vector2 mousePos = e.mousePosition;
            int x = Mathf.FloorToInt((mousePos.x - startX) / cellSize);
            int y = Mathf.FloorToInt((mousePos.y - startY) / cellSize);

            if (x >= 0 && x < columns && y >= 0 && y < rows)
            {
                Handles.BeginGUI();
                Color originalColor = Handles.color;
                Handles.color = new Color(0, 255, 0, 0.5f); // ������ ���
                Handles.DrawSolidRectangleWithOutline(GetBrushRect(x, y, startX, startY), new Color(1, 1, 1, 0.2f), Color.white);
                Handles.color = originalColor;
                Handles.EndGUI();
                Repaint();
            }
        }

    }

    private void HandleDragEvents(float startX, float startY)
    {
        Event e = Event.current;

        if (isDragging && e.isMouse)
        {
            Vector2 mousePos = e.mousePosition;
            int x = Mathf.FloorToInt((mousePos.x - startX) / cellSize);
            int y = Mathf.FloorToInt((mousePos.y - startY) / cellSize);

            if (e.type == EventType.MouseDrag)
            {
                if (x >= 0 && x < columns && y >= 0 && y < rows)
                {
                    if (Vector2.Distance(startDragPosition, e.mousePosition) > cellSize / 2)
                    {
                        ApplyBrush(x, y);
                        startDragPosition = e.mousePosition; // ������Ʈ�� ���������� ����
                    }
                    Repaint();
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                isDragging = false;
                e.Use();
            }
        }
    }

    private void ApplyBrush(int centerX, int centerY)
    {
        int halfBrushSize = toolSize / 2;

        for (int y = -halfBrushSize; y <= halfBrushSize; y++)
        {
            for (int x = -halfBrushSize; x <= halfBrushSize; x++)
            {
                int targetX = centerX + x;
                int targetY = centerY + y;

                if (targetX >= 0 && targetX < columns && targetY >= 0 && targetY < rows)
                {
                    int value = 0;
                    switch (toolType)
                    {
                        case ToolType.Brush:
                            value = 1;
                            break;
                        case ToolType.Eraser:
                            value = 0;
                            break;
                    }
                    grid[targetX, targetY] = value;
                    script.grid[targetX, targetY] = value;
                }
            }
        }
    }

    private Rect GetBrushRect(int centerX, int centerY, float startX, float startY)
    {
        int halfBrushSize = toolSize / 2;
        float left = startX + (centerX - halfBrushSize) * cellSize;
        float top = startY + (centerY - halfBrushSize) * cellSize;
        float size = toolSize * cellSize;
        return new Rect(left, top, size, size);
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }
}
#endif