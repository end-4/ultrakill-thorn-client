using UnityEngine;
using UnityEngine.EventSystems;

namespace ThornClient.System.ClickGUIComponents;

internal class ClickGUITooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    /// <summary>
    /// The text to set on the tooltip
    /// </summary>
    public string Text {
        get;
        set {
            field = value;
            if (_lastSetText != "") SetText();
        }
    }

    private string _lastSetText = "";

    /// <summary>
    /// Fired natively by Unity when the mouse pointer begins hovering over this GameObject's RectTransform boundary.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData) {
        if (Text.Length == 0) return;
        SetText();
    }

    /// <summary>
    /// Fired natively by Unity when the mouse pointer leaves this GameObject's UI bounds.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData) {
        UnsetText();
    }

    private void OnDisable() {
        UnsetText();
    }

    private void SetText() {
        _lastSetText = Text;
        ClickGUI.SetTooltipText(_lastSetText);
    }

    private void UnsetText() {
        ClickGUI.SurrenderTooltipText(_lastSetText);
        _lastSetText = "";
    }
}
