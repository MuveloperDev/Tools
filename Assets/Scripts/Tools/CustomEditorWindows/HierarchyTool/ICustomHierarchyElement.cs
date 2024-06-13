#if UNITY_EDITOR
using UnityEngine;

// Ensure to apply this feature only to objects used in the editor.
public interface ICustomHierarchyElement
{
    GUIStyle CustomHierarchyElementStyle(ref GUIStyle InStyle);
    Color CustomHierarchyElementBackGroundColor();
}
#endif

