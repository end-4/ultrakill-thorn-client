using NukeLib.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Component to add to a GameObject that's the direct child of the target body for dragging
/// </summary>
public class TitlebarDragIgnoreLayoutHandler : TitlebarDragHandler {
    public override void OnDrag(PointerEventData eventData) {
        base.OnDrag(eventData);
        var layoutComp = target.GetOrAddComponent<LayoutElement>();
        layoutComp.ignoreLayout = true;
    }
}
