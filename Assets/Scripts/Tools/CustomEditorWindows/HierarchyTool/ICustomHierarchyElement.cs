#if UNITY_EDITOR
using UnityEngine;

public interface ICustomHierarchyElement
{
    GUIStyle CustomHierarchyElementStyle(ref GUIStyle InStyle);
    Color CustomHierarchyElementBackGroundColor();
}
#endif

