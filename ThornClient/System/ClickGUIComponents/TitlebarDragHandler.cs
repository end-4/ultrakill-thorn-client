using System;
using UnityEngine;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Component to add to a GameObject that's the direct child of the target body for dragging
/// </summary>
internal class TitlebarDragHandler : FreeMoveDragHandler {
    private void Start() {
        try {
            target = (RectTransform)transform.parent.transform;
        } catch (Exception e) {
            Plugin.Log.LogWarning("[TitlebarDragHandler] may have been added to a root element");
        }
    }
}
