using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CustomEditorChangeScene : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> scenePaths = new List<string>();
    private bool isDragging = false;
    private int draggingIndex = -1;
    private string draggingScenePath;
    private Vector2 dragStartPos;
    private Vector2 dragOffset;

    [MenuItem("Window/Scenes Window [ Custom Editor ]")]
    public static void ShowWindow()
    {
        GetWindow<CustomEditorChangeScene>("Scenes Window");
    }

    private void OnEnable()
    {
        // Initialize the scene paths
        scenePaths.Clear();
        string[] paths = Directory.GetFiles("Assets/Scenes", "*.unity", SearchOption.AllDirectories);
        scenePaths.AddRange(paths);
    }

    private void OnGUI()
    {
        DrawTitle();
        DrawSceneButtons();
        HandleDragging();
    }

    private void DrawTitle()
        => GUILayout.Label("Scene Management", EditorStyles.boldLabel);

    private void DrawSceneButtons()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(50));
        GUILayout.BeginHorizontal();

        for (int i = 0; i < scenePaths.Count; i++)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenePaths[i]);
            Rect buttonRect = GUILayoutUtility.GetRect(100, 30, GUILayout.Width(100), GUILayout.Height(30));

            if (Event.current.type == EventType.Repaint)
            {
                GUI.Button(buttonRect, sceneName);
            }

            // Handle mouse down event
            if (Event.current.type == EventType.MouseDown && buttonRect.Contains(Event.current.mousePosition) && !isDragging)
            {
                isDragging = true;
                draggingIndex = i;
                draggingScenePath = scenePaths[i];
                dragStartPos = Event.current.mousePosition;
                dragOffset = dragStartPos - new Vector2(buttonRect.x, buttonRect.y);
                Event.current.Use();
            }

            // Highlight the dragging item
            if (isDragging && draggingIndex == i)
            {
                GUI.Box(buttonRect, "", "box");
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();

        // Draw the dragging element
        if (isDragging)
        {
            Vector2 currentMousePos = Event.current.mousePosition;
            Rect draggingRect = new Rect(currentMousePos.x - dragOffset.x, scrollPosition.y, 100, 30);
            GUI.Button(draggingRect, Path.GetFileNameWithoutExtension(draggingScenePath));
            Repaint();
        }
    }

    private void HandleDragging()
    {
        if (isDragging)
        {
            if (Event.current.type == EventType.MouseDrag)
            {
                Vector2 currentMousePos = Event.current.mousePosition;
                int targetIndex = (int)((currentMousePos.x - scrollPosition.x) / 100f);
                targetIndex = Mathf.Clamp(targetIndex, 0, scenePaths.Count - 1);

                if (targetIndex != draggingIndex)
                {
                    scenePaths.RemoveAt(draggingIndex);
                    scenePaths.Insert(targetIndex, draggingScenePath);
                    draggingIndex = targetIndex;
                }

                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                isDragging = false;
                draggingIndex = -1;
                draggingScenePath = null;
                Event.current.Use();
            }
        }
    }
}
