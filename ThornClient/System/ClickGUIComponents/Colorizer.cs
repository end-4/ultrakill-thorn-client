using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class Colorizer : MonoBehaviour {
    private TextMeshProUGUI? textComp;
    private Image? imageComp;

    private Color? _overrideColor;
    private Color? _overrideNormalColor;

    public Color HighlightColor {
        get => _overrideColor ?? ThornModule.Instance!.Accent.Value;
        set {
            if (imageComp == null) _overrideColor = value;
        }
    }

    public Color NormalColor {
        get => _overrideNormalColor ?? Color.white;
        set => _overrideNormalColor = value;
    }

    private void Start() {
        textComp = GetComponent<TextMeshProUGUI>();
        imageComp = GetComponent<Image>();
        if (_overrideColor == null) ThornModule.Instance!.Accent.OnChanged += UpdateHighlight;
        UpdateHighlight();
    }

    private void OnDestroy() {
        if (_overrideColor == null) ThornModule.Instance!.Accent.OnChanged -= UpdateHighlight;
    }

    private bool _lastHighlight = false;
    public void UpdateHighlight(bool highlighted) {
        _lastHighlight = highlighted;
        var targetColor = highlighted ? HighlightColor : NormalColor;
        if (textComp != null) {
            textComp.color = targetColor;
        } else if (imageComp != null) {
            imageComp.color = targetColor;
        }
    }

    public void UpdateHighlight() {
        UpdateHighlight(_lastHighlight);
    }
}
