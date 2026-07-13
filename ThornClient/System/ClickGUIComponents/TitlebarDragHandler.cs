using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Component to add to a GameObject that's the direct child of the target body for dragging
/// </summary>
public class TitlebarDragHandler : FreeMoveDragHandler {
    private void Start() {
        try {
            target = (RectTransform)transform.parent.transform;
        } catch (Exception e) {
            Plugin.Log.LogWarning("[TitlebarDragHandler] may have been added to a root element");
        }
    }
}
