using System;
using System.Linq;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Controller for Color Setting fields
/// </summary>
public class ColorSettingController : MonoBehaviour {
    public Setting<Color>? TargetSetting;

    private Image? _colorPreview;
    private TMP_InputField? _hexInput;
    private Slider?[] _sliders;
    private TMP_InputField?[] _valueInputs;

    private void Start() {
        if (TargetSetting == null) return;
        _colorPreview = gameObject.FindRecursive("PickerRow/PreviewColumn/ColorBorder/ColorPreview")
            ?.GetComponent<Image>();
        _hexInput = gameObject.FindRecursive("PickerRow/PreviewColumn/HexInput")?.GetComponent<TMP_InputField>();
        _sliders = [
            gameObject.FindRecursive("PickerRow/ValueColumn/Red/Slider")?.GetComponent<Slider>(),
            gameObject.FindRecursive("PickerRow/ValueColumn/Green/Slider")?.GetComponent<Slider>(),
            gameObject.FindRecursive("PickerRow/ValueColumn/Blue/Slider")?.GetComponent<Slider>(),
            gameObject.FindRecursive("PickerRow/ValueColumn/Alpha/Slider")?.GetComponent<Slider>(),
        ];
        _valueInputs = [
            gameObject.FindRecursive("PickerRow/ValueColumn/Red/Input")?.GetComponent<TMP_InputField>(),
            gameObject.FindRecursive("PickerRow/ValueColumn/Green/Input")?.GetComponent<TMP_InputField>(),
            gameObject.FindRecursive("PickerRow/ValueColumn/Blue/Input")?.GetComponent<TMP_InputField>(),
            gameObject.FindRecursive("PickerRow/ValueColumn/Alpha/Input")?.GetComponent<TMP_InputField>(),
        ];
        Plugin.Log.LogInfo($"Adding listeners");
        if (_hexInput != null) {
            _hexInput.onEndEdit.AddListener(TrySaveNewHex);
            _hexInput.GetOrAddComponent<InputFocusGrab>();
        }
        for (int i = 0; i < _sliders.Length; i++) {
            var slider = _sliders[i];
            if (slider != null) {
                slider.minValue = 0;
                slider.maxValue = 1;
                var i1 = i;
                slider.onValueChanged.AddListener((float value) => { TrySaveNewValue(value, i1); });
            }
        }

        for (int i = 0; i < _valueInputs.Length; i++) {
            var input = _valueInputs[i];
            input.GetOrAddComponent<InputFocusGrab>();
            if (input != null) {
                var i1 = i;
                input.onValueChanged.AddListener((string s) => {
                    if (int.TryParse(s, out var n)) {
                        TrySaveNewValue((float)n / 255f, i1);
                    }
                });
            }
        }

        TargetSetting.OnValueChanged += UpdateDisplay;
        UpdateDisplay(TargetSetting.Value);
    }

    private void TrySaveNewHex(string hex) {
        if (TargetSetting == null) return;
        if (!hex.StartsWith("#")) hex = "#" + hex;
        if (ColorUtility.TryParseHtmlString(hex, out Color color)) {
            TargetSetting.Value = new Color(color.r, color.g, color.b, TargetSetting.Value.a);
        } else {
            // Reset if invalid
            TargetSetting.Value = new Color(TargetSetting.Value.r, TargetSetting.Value.g, TargetSetting.Value.b,
                TargetSetting.Value.a);
        }

        UpdateDisplay(TargetSetting.Value);
    }

    private void TrySaveNewValue(float value, int index) {
        if (TargetSetting == null) return;
        float[] colVals = [0, 1, 2, 3];
        colVals = colVals.Select(i => ((int)i == index ? value : TargetSetting.Value.GetValues()[(int)i])).ToArray();
        TargetSetting.Value = new Color(colVals[0], colVals[1], colVals[2], colVals[3]);
    }

    private void OnDestroy() {
        if (TargetSetting == null) return;
        TargetSetting.OnValueChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(Color col) {
        if (TargetSetting == null) return;
        if (_colorPreview != null) _colorPreview.color = TargetSetting.Value;
        if (_hexInput != null) _hexInput.text = ColorUtility.ToHtmlStringRGB(col);
        float[] vals = [col.r, col.g, col.b, col.a];
        for (int i = 0; i < _sliders.Length; i++) {
            Slider? slider = _sliders[i];
            if (slider != null) slider.value = vals[i];
        }

        for (int i = 0; i < _valueInputs.Length; i++) {
            TMP_InputField? input = _valueInputs[i];
            if (input != null) input.text = $"{(int)(255f * vals[i])}";
        }

        gameObject.UnfuckLayoutHack();
    }
}
