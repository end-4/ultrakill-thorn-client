using System;
using System.Linq;
using Newtonsoft.Json;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Controller for an EnhancedColor's setting window
/// Note: Sliders range [0, 1], inputs range [0, 255]
/// </summary>
internal class EnhancedColorWindowController : MonoBehaviour {
    /// <summary>
    /// The setting that the controller handles
    /// </summary>
    public Setting<EnhancedColor>? TargetSetting;

    private const float InputMax = 255f;
    private const int NumSliders = 4;

    private GameObject? _body;
    private Slider?[] _sliders;
    private TMP_InputField?[] _valueInputs;
    private Image? _preview;
    private TMP_InputField? _hexInput;
    private GameObject?[] _modeButtons;
    private Image?[] _modeBorders;
    private Image?[] _modeIcons;
    private TextMeshProUGUI?[] _modeTexts;

    private readonly string[] SliderPaths = [
        "ActualColorSettings/ValueColumn/Red/Slider",
        "ActualColorSettings/ValueColumn/Green/Slider",
        "ActualColorSettings/ValueColumn/Blue/Slider",
        "ActualColorSettings/ValueColumn/Alpha/Slider",
    ];

    private readonly string[] InputPaths = [
        "ActualColorSettings/ValueColumn/Red/Input",
        "ActualColorSettings/ValueColumn/Green/Input",
        "ActualColorSettings/ValueColumn/Blue/Input",
        "ActualColorSettings/ValueColumn/Alpha/Input",
    ];

    private readonly string[] ModeButtonPaths = [
        "ActualColorSettings/ModeRow/Static",
        "ActualColorSettings/ModeRow/HueShift",
    ];

    private readonly EnhancedColorMode[] EnhancedColorModes = [
        EnhancedColorMode.Static, EnhancedColorMode.HuePulse
    ];

    private static JsonSerializerSettings _serializerSettings = new JsonSerializerSettings {
        Converters = { new EnhancedColorJsonConverter() },
        Formatting = Formatting.Indented
    };

    private void Start() {
        if (TargetSetting == null) return;
        _body = gameObject.FindRecursive("Scroll View/Viewport/Content/Modules");
        if (_body == null) return;

        // Header: Dragging, text, back btn
        gameObject.FindRecursive("Header")?.AddComponent<TitlebarDragHandler>();

        var title = gameObject.FindRecursive("Header/TitleName")?.GetComponent<TextMeshProUGUI>();
        title?.SetText(TargetSetting.Name);

        var backBtn = gameObject.FindRecursive("Header/TitleButton")?.GetComponent<Button>();
        if (backBtn != null) {
            backBtn.interactable = true;
            backBtn.onClick.AddListener(() => Destroy(gameObject));
        }

        // Body: Value axes
        _sliders = SliderPaths.Select(path => _body.FindRecursive(path)?.GetComponent<Slider>()).ToArray();
        _valueInputs = InputPaths.Select(path => _body.FindRecursive(path)?.GetComponent<TMP_InputField>()).ToArray();
        for (int i = 0; i < NumSliders; i++) {
            var slider = _sliders[i];
            if (slider != null) {
                slider.minValue = 0;
                slider.maxValue = 1;
                var i1 = i;
                slider.onValueChanged.AddListener(value => { TrySaveNewValue(value, i1); });
            }
        }

        for (int i = 0; i < NumSliders; i++) {
            var input = _valueInputs[i];
            if (input != null) {
                input.GetOrAddComponent<InputFocusGrab>();
                var i1 = i;
                input.onEndEdit.AddListener(str => {
                    if (int.TryParse(str, out var n)) {
                        TrySaveNewValue(n / InputMax, i1);
                    }
                });
            }
        }

        // Body: Preview
        _hexInput = _body.FindRecursive("ActualColorSettings/ModeRow/PreviewColumn/HexInput")
            ?.GetComponent<TMP_InputField>();
        _preview = _body.FindRecursive("ActualColorSettings/ModeRow/PreviewColumn/ColorBorder/ColorPreview")
            ?.GetComponent<Image>();
        if (_hexInput != null) {
            _hexInput.GetOrAddComponent<InputFocusGrab>();
            _hexInput.onEndEdit.AddListener(TrySaveNewHex);
        }

        // Body: mode selection
        _modeButtons = ModeButtonPaths.Select(path => _body.FindRecursive(path)).ToArray();
        _modeBorders = _modeButtons.Select(gob => gob?.GetComponent<Image>()).ToArray();
        _modeIcons = _modeButtons.Select(gob => gob?.FindRecursive("Icon")?.GetComponent<Image>()).ToArray();
        _modeTexts = _modeButtons.Select(gob => gob?.FindRecursive("Text")?.GetComponent<TextMeshProUGUI>()).ToArray();
        for (int i = 0; i < EnhancedColorModes.Length; i++) {
            var mode = EnhancedColorModes[i];
            var btn = _modeButtons[i]?.GetComponent<Button>();
            btn?.onClick.AddListener(() =>
                TargetSetting.Value = new EnhancedColor(TargetSetting.Value.BaseColor, mode));
        }

        // Body: action buttons
        var resetButton = _body.FindRecursive("ActionsRow/Reset")?.GetComponent<Button>();
        var copyButton = _body.FindRecursive("ActionsRow/Copy")?.GetComponent<Button>();
        var pasteButton = _body.FindRecursive("ActionsRow/Paste")?.GetComponent<Button>();
        resetButton?.onClick.AddListener(() => TargetSetting?.Reset());
        copyButton?.onClick.AddListener(ClipboardCopy);
        pasteButton?.onClick.AddListener(ClipboardPaste);

        // Hooks
        TargetSetting.OnChanged += UpdateAll;
        TargetSetting.OnEffectiveValueChanged += UpdatePreview;
        ThornModule.Instance!.Accent.OnChanged += UpdateModeSelection;
        ThornModule.Instance!.Accent.OnEffectiveValueChanged += UpdateModeSelection;

        // Init updates
        UpdateAll();
    }

    private void OnDestroy() {
        if (TargetSetting != null) {
            TargetSetting.OnChanged -= UpdateAll;
            TargetSetting.OnEffectiveValueChanged -= UpdatePreview;
        }
        ThornModule.Instance!.Accent.OnChanged -= UpdateModeSelection;
        ThornModule.Instance!.Accent.OnEffectiveValueChanged -= UpdateModeSelection;
    }

    private static float GetSafeColorValue(float value) {
        return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
    }

    private void UpdateAll() {
        if (TargetSetting == null) return;

        // Slider values
        var currVal = TargetSetting.Value;
        float[] rgba = [
            currVal.r,
            currVal.g,
            currVal.b,
            currVal.a
        ];
        for (int i = 0; i < NumSliders; i++) {
            var slider = _sliders[i];
            if (slider != null) slider.SetValueWithoutNotify(rgba[i]);
            var input = _valueInputs[i];
            if (input != null) input.SetTextWithoutNotify($"{Mathf.RoundToInt(rgba[i] * InputMax)}");
        }

        // Preview
        UpdatePreview();
        if (_hexInput != null) _hexInput.SetTextWithoutNotify($"#{ColorUtility.ToHtmlStringRGBA(TargetSetting.Value.BaseColor)}");

        // Mode selection
        UpdateModeSelection();
    }

    private void UpdatePreview() {
        if (TargetSetting == null || _preview == null) return;
        _preview.color = TargetSetting.Value.GetCurrentColor();
    }

    private void UpdateModeSelection() {
        for (int i = 0; i < EnhancedColorModes.Length; i++) {
            var border = _modeBorders[i];
            var icon = _modeIcons[i];
            var text = _modeTexts[i];
            var color = EnhancedColorModes[i] == TargetSetting.Value.Mode ? ThornModule.Instance!.Accent.Value.GetCurrentColor() : Color.white;
            if (border == null || icon == null || text == null) continue;
            border.color = color;
            icon.color = color;
            text.color = color;
        }
    }

    private void TrySaveNewHex(string hex) {
        if (TargetSetting == null) return;
        if (!hex.StartsWith("#")) hex = "#" + hex;
        var currVal = TargetSetting.Value;
        var currMode = currVal.Mode;
        var currA = currVal.a;
        if (ColorUtility.TryParseHtmlString(hex, out Color color)) {
            if (hex.Length == 8) {
                // Transparency specified
                TargetSetting.Value = new EnhancedColor(color, currMode);
            } else {
                TargetSetting.Value = new EnhancedColor(color.r, color.g, color.b, currA, currMode);
            }
        } else {
            // Reset if invalid
            TargetSetting.Value = new EnhancedColor(
                currVal.r, currVal.g, currVal.b,
                currA, currMode
            );
        }

        UpdateAll();
    }

    private void ClipboardCopy() {
        if (TargetSetting == null) return;
        GUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(TargetSetting.Value, _serializerSettings);
    }

    private void ClipboardPaste() {
        if (TargetSetting == null) return;
        try {
            var newVal = JsonConvert.DeserializeObject<EnhancedColor>(GUIUtility.systemCopyBuffer,
                new EnhancedColorJsonConverter());
            if (newVal != null) TargetSetting.Value = newVal;
        } catch (Exception e) {
            // Just not paste if clipboard content isn't correct data
        }
    }

    private void TrySaveNewValue(float value, int index) {
        if (TargetSetting == null || index > 3) return;
        if (float.IsNaN(value) || float.IsInfinity(value)) return;

        var currVal = TargetSetting.Value;
        float[] rgba = [
            currVal.r,
            currVal.g,
            currVal.b,
            currVal.a,
        ];
        rgba[index] = value;
        var newColor = new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
        TargetSetting.Value = new EnhancedColor(newColor, currVal.Mode);
    }
}
