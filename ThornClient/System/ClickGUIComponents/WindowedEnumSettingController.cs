using System;
using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Controller for an enum option that opens up a new window
/// </summary>
public class WindowedEnumSettingController : MonoBehaviour {
    /// <summary>
    /// The setting this controller handles
    /// </summary>
    public Setting? TargetSetting { get; set; }

    private TextMeshProUGUI? _selected;
    private Button? _btn;
    private GameObject? _panel;

    private void Start() {
        _btn = GetComponent<Button>();
        _selected = gameObject.FindRecursive("Dropdown/Label")?.GetComponent<TextMeshProUGUI>();
        if (_btn != null) _btn.onClick.AddListener(OpenWindow);
        if (TargetSetting != null) TargetSetting.OnChanged += UpdateSelection;
        UpdateSelection();
    }

    private void OnDestroy() {
        if (TargetSetting != null) TargetSetting.OnChanged -= UpdateSelection;
        if (_btn != null) _btn.onClick.RemoveListener(OpenWindow);
    }

    private void UpdateSelection() {
        if (_selected == null || TargetSetting == null) return;
        var currSel = TargetSetting.GetValue();
        var str = currSel.ToString();
        var displayText = TargetSetting?.Hints?.EnumSubstitutions.GetValueOrDefault(str, str);
        _selected.SetText($"{displayText}");
    }

    private void OpenWindow() {
        if (_panel != null) return;
        _panel = CreateSelectionPanel(TargetSetting);
        if (_panel == null) return;
        ClickGUI.SpawnContent(_panel);
        ClickGUI.SurrenderTooltipText(TargetSetting?.Description ?? "");
    }

    private GameObject? CreateSelectionPanel(Setting? setting) {
        var prefab = AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ModuleCategory");
        if (prefab == null || setting == null) return null;

        var obj = Instantiate(prefab);
        if (obj == null) return null;

        var ctl = obj.GetOrAddComponent<EnumWindowController>();
        ctl.TargetSetting = setting;

        return obj;
    }
}
