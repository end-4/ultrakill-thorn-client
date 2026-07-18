using UnityEngine;
using UnityEngine.EventSystems;
using ThornClient.Core;

namespace ThornClient.System.ClickGUIComponents;

public class ClickGUITooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public string text = "";

    /// <summary>
    /// Fired natively by Unity when the mouse pointer begins hovering over this GameObject's RectTransform boundary.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData) {
        ClickGUI.SetTooltipText(text);
    }

    /// <summary>
    /// Fired natively by Unity when the mouse pointer leaves this GameObject's UI bounds.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData) {
        ClickGUI.SurrenderTooltipText(text);
    }
}
