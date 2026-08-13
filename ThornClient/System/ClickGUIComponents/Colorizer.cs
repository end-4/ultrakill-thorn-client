using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Component that colors stuff based on active/inactive state
/// </summary>
public class Colorizer : MonoBehaviour {
    private TextMeshProUGUI? _textComp;
    private Image? _imageComp;

    private Color? _overrideColor;
    private Color? _overrideNormalColor;

    /// <summary>
    /// The color to use when highlighted
    /// </summary>
    public Color HighlightColor {
        get => _overrideColor ?? ThornModule.Instance!.Accent.Value;
        set {
            if (_imageComp == null) _overrideColor = value;
        }
    }

    /// <summary>
    /// The color to use when not highlighted
    /// </summary>
    public Color NormalColor {
        get => _overrideNormalColor ?? Color.white;
        set => _overrideNormalColor = value;
    }

    private void Start() {
        _textComp = GetComponent<TextMeshProUGUI>();
        _imageComp = GetComponent<Image>();
        if (_overrideColor == null) ThornModule.Instance!.Accent.OnChanged += UpdateHighlight;
        UpdateHighlight();
    }

    private void OnDestroy() {
        if (_overrideColor == null) ThornModule.Instance!.Accent.OnChanged -= UpdateHighlight;
    }

    private bool _highlighted = false;

    /// <summary>
    /// Whether the current element should be highlighted
    /// </summary>
    public bool Highlighted {
        get => _highlighted;
        set {
            _highlighted = value;
            var targetColor = value ? HighlightColor : NormalColor;
            if (_textComp != null) {
                _textComp.color = targetColor;
            } else if (_imageComp != null) {
                _imageComp.color = targetColor;
            }
        }
    }

    /// <summary>
    /// Forces color update
    /// </summary>
    private void UpdateHighlight() {
        Highlighted = _highlighted;
    }
}
