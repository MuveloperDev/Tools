#if UNITY_EDITOR
using UnityEngine;

public class CustomHierarchyElement : MonoBehaviour, ICustomHierarchyElement
{
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
#endif
