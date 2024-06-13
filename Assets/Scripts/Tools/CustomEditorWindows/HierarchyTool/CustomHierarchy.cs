// This script can move Editor folder.
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class CustomHierarchy
{
    private static GUIStyle style;
    static CustomHierarchy()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        InitializeGUIStyle();
    }
    private static void InitializeGUIStyle()
    {
        style = new GUIStyle
        {
            normal = { textColor = Color.black },
            alignment = TextAnchor.MiddleCenter
        };
    }
    private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null)
            return;

        ICustomHierarchyElement custom = go.GetComponent<ICustomHierarchyElement>();
        if (custom == null)
            return;

        style = custom.CustomHierarchyElementStyle(ref style);
        Color bgColor = custom.CustomHierarchyElementBackGroundColor();
        EditorGUI.DrawRect(selectionRect, bgColor);
        EditorGUI.DropShadowLabel(selectionRect, go.name, style);
    }
}

#endif