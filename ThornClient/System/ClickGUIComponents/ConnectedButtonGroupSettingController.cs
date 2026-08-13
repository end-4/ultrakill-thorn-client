using System;
using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using ThornClient.Managers;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// For enums
/// Inspiration: https://m3.material.io/components/button-groups/specs
/// Keyword for pattern-mining code readers: EnumSettingController
/// </summary>
internal class ConnectedButtonGroupSettingController : MonoBehaviour {
    public Setting? TargetSetting { get; set; }
    private Type? _enumType;
    private GameObject? _btnRow;
    private string[] _enumNames = [];

    private readonly List<(Image image, TMP_Text text)> _cachedButtons = new();

    private void Start() {
        if (TargetSetting == null) return;
        var currentValue = TargetSetting.GetValue();
        _enumType = currentValue.GetType();

        if (!_enumType.IsEnum) {
            Plugin.Log.LogError($"[EnumSettingController] Setting {TargetSetting.Name} is not an enum!");
            return;
        }

        TargetSetting.OnChanged += UpdateDisplay;

        _btnRow = gameObject.FindRecursive("ButtonRow");
        // Plugin.Log.LogInfo($"curr {gameObject.name}, btn row {_btnRow}");
        if (_btnRow != null) {
            _enumNames = Enum.GetNames(_enumType);
            _cachedButtons.Clear();

            for (int i = 0; i < _enumNames.Length; i++) {
                var btn = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ConnectedChoiceButton"), _btnRow.transform);

                // Note: Fixed text lookup consistency across Start and UpdateDisplay
                var textComp = btn.FindRecursive("Text").GetComponent<TMP_Text>();
                var imgComp = btn.GetComponent<Image>();

                textComp.text = _enumNames[i];

                // Cache components
                _cachedButtons.Add((imgComp, textComp));

                var i1 = i;
                btn.GetComponent<Button>().onClick.AddListener(() => {
                    TargetSetting.SetValue(Enum.Parse(_enumType, _enumNames[i1]));
                });
            }
        }

        UpdateDisplay();
    }

    private void UpdateDisplay() {
        if (TargetSetting == null || _enumType == null || _cachedButtons.Count == 0) return;

        string currentSettingName = TargetSetting.GetValue().ToString();
        int total = _cachedButtons.Count;

        for (int i = 0; i < total; i++) {
            var (imgComp, textComp) = _cachedButtons[i];

            bool selected = _enumNames[i] == currentSettingName;
            bool left = i == 0;
            bool right = i == total - 1;

            Color targetColor = selected ? ThornModule.AccentColor : Color.white;
            Color targetTextColor = selected ? Color.black : Color.white;

            imgComp.sprite = GetSprite(left, right, selected);

            if (imgComp.color != targetColor) imgComp.color = targetColor;
            if (textComp.color != targetTextColor) textComp.color = targetTextColor;
        }
    }

    private void OnDestroy() {
        if (TargetSetting != null) TargetSetting.OnChanged -= UpdateDisplay;
    }

    public static Sprite GetSprite(bool atLeft, bool atRight, bool active) {
        string assetName =
            $"{(atLeft || atRight ? "Round_" : "")}{(active ? "Fill" : "Border")}Large{(atLeft ? "Left" : atRight ? "Right" : "")}";
        return AssetManager.Get<Sprite>(ClickGUI.BundleKey, assetName);
    }
}
