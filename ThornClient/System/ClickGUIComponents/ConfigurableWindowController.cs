using System;
using System.Collections.Generic;
using System.Linq;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.DataTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.System.ClickGUIComponents;

internal class ConfigurableWindowController : MonoBehaviour {
    private static readonly Dictionary<SettingType, Func<Setting, Transform, GameObject>> SettingUIFactories = new() {
        [SettingType.Bind] = (setting, parent) => {
            var go = Instantiate(ClickGUI.KeybindSettingPrefab, parent);
            go.AddComponent<KeybindSettingController>().TargetSetting = (Setting<Keybind>)setting;
            return go;
        },
        [SettingType.Bool] = (setting, parent) => {
            var go = Instantiate(ClickGUI.BoolSettingPrefab, parent);
            go.AddComponent<BoolSettingController>().TargetSetting = (Setting<bool>)setting;
            return go;
        },
        [SettingType.Color] = (setting, parent) => {
            var go = Instantiate(ClickGUI.ColorSettingPrefab, parent);
            go.AddComponent<ColorSettingController>().TargetSetting = (Setting<Color>)setting;
            return go;
        },
        [SettingType.EnemyList] = (setting, parent) => {
            var go = Instantiate(ClickGUI.EnemyListSettingPrefab, parent);
            go.AddComponent<EnemyListSettingController>().TargetSetting = (Setting<EnemyList>)setting;
            return go;
        },
        [SettingType.Enum] = (setting, parent) => {
            var enumType = setting.GetValue().GetType();
            int totalChars = Enum.GetNames(enumType).Sum(name => name.Length);
            Plugin.Log.LogInfo(
                $"setting {setting.Name} total chars: {totalChars} prefer connected {setting.Hints?.EnumPreferButtonGroup ?? false}");
            if (setting.Hints?.EnumPreferButtonGroup == true ||
                (totalChars <= 20 && setting.Hints?.EnumPreferButtonGroup != false)) {
                var go = Instantiate(ClickGUI.ConnectedButtonGroupSettingPrefab, parent);
                go.AddComponent<ConnectedButtonGroupSettingController>().TargetSetting = setting;
                return go;
            } else {
                var go = Instantiate(ClickGUI.DropdownSettingPrefab, parent);
                go.AddComponent<DropdownSettingController>().TargetSetting = setting;
                return go;
            }
        },
        [SettingType.Float] = (setting, parent) => {
            var go = Instantiate(ClickGUI.NumberSettingPrefab, parent);
            go.AddComponent<FloatSettingController>().TargetSetting = (Setting<float>)setting;
            return go;
        },
        [SettingType.Int] = (setting, parent) => {
            var go = Instantiate(ClickGUI.NumberSettingPrefab, parent);
            go.AddComponent<IntSettingController>().TargetSetting = (Setting<int>)setting;
            return go;
        },
        [SettingType.Text] = (setting, parent) => {
            var go = Instantiate(ClickGUI.TextSettingPrefab, parent);
            go.AddComponent<TextSettingController>().TargetSetting = (Setting<string>)setting;
            return go;
        }
    };

    private bool _doneSetup = false;
    public bool IsPopup = false;
    public Configurable? TargetConfigurable;

    private void Start() {
        SetupModules();
    }

    public void SetupModules() {
        if (_doneSetup) return;
        _doneSetup = true;

        // Header: icon, text, dragging behavior
        if (TargetConfigurable == null) return;
        var categoryIcon = gameObject.FindRecursive("Header/TitleButton/TitleIcon").GetComponent<Image>();
        var categoryText = gameObject.FindRecursive("Header/TitleName").GetComponent<TextMeshProUGUI>();
        if (TargetConfigurable is Module module) {
            categoryIcon.sprite = ClickGUI.Bundle.LoadAsset<Sprite>(module.IconName);
        }

        categoryText.text = TargetConfigurable.Name;
        gameObject.FindRecursive("Header").AddComponent<TitlebarDragHandler>();
        var backBtn = gameObject.FindRecursive("Header/TitleButton").GetComponent<Button>();
        var backBtnImg = gameObject.FindRecursive("Header/TitleButton").GetComponent<Image>();
        gameObject.FindRecursive("Header/TitleButton").GetComponent<Button>().interactable = IsPopup;
        gameObject.FindRecursive("Header/TitleButton/BackIcon").SetActive(IsPopup);
        if (IsPopup) backBtn.onClick.AddListener(ClickGUI.NavigateBack);

        // Populate with settings
        Transform
            listBody = gameObject.FindRecursive("Modules")
                .transform; // Note that we reuse ModuleCategory prefab for this
        var desc = Instantiate(ClickGUI.ModuleDescriptionPrefab!, listBody);
        desc.FindRecursive("DescText").GetComponent<TextMeshProUGUI>().text = TargetConfigurable.Description;
        if (TargetConfigurable is not SystemModule) {
            var enabledButton = Instantiate(ClickGUI.EnabledButtonPrefab, listBody);
            var enabledButtonComp = enabledButton.AddComponent<EnabledButtonController>();
            enabledButtonComp.Configurable = TargetConfigurable;
        }

        foreach (var element in TargetConfigurable.Settings) {
            if (element is not { } setting) continue;

            GameObject wrapper = Instantiate(ClickGUI.SettingRowWrapperPrefab, listBody);
            wrapper.AddComponent<SettingRowController>().TargetSetting = setting;

            if (SettingUIFactories.TryGetValue(setting.Type, out var createUI)) {
                GameObject go = createUI(setting, wrapper.transform);
                go.AddComponent<SettingDescriptionController>().TargetSetting = setting;
            }

            wrapper.UnfuckLayoutHack();
        }

        gameObject.UnfuckLayoutHack();
    }
}
