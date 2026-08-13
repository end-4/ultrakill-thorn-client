using UnityEngine;
using UnityEngine.EventSystems;

namespace ThornClient.System.ClickGUIComponents;

internal class ClickGUITooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public string Text = "";

    /// <summary>
    /// Fired natively by Unity when the mouse pointer begins hovering over this GameObject's RectTransform boundary.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData) {
        ClickGUI.SetTooltipText(Text);
    }

    /// <summary>
    /// Fired natively by Unity when the mouse pointer leaves this GameObject's UI bounds.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData) {
        ClickGUI.SurrenderTooltipText(Text);
    }
}
