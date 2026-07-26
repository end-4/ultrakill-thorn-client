using System;
using System.Collections.Generic;
using System.Linq;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.DataTypes;
using ThornClient.HUD.HUDComponents;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.System.ClickGUIComponents;

internal class ConfigurableWindowController : MonoBehaviour {
    private static readonly Dictionary<SettingType, Func<Setting, Transform, GameObject>> SettingUIFactories = new() {
        [SettingType.Bind] = (setting, parent) => {
            var go = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "KeybindSetting"), parent);
            go.AddComponent<KeybindSettingController>().TargetSetting = (Setting<Keybind>)setting;
            return go;
        },
        [SettingType.Bool] = (setting, parent) => {
            var go = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "BoolSetting"), parent);
            go.AddComponent<BoolSettingController>().TargetSetting = (Setting<bool>)setting;
            return go;
        },
        [SettingType.Color] = (setting, parent) => {
            var go = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ColorSetting"), parent);
            go.AddComponent<ColorSettingController>().TargetSetting = (Setting<Color>)setting;
            return go;
        },
        [SettingType.EnemyList] = (setting, parent) => {
            var go = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "EnemyListSetting"), parent);
            go.AddComponent<EnemyListSettingController>().TargetSetting = (Setting<EnemyList>)setting;
            return go;
        },
        [SettingType.Enum] = (setting, parent) => {
            var enumType = setting.GetValue().GetType();
            int totalChars = Enum.GetNames(enumType).Sum(name => name.Length);
            if (setting.Hints?.EnumPreferButtonGroup == true ||
                (totalChars <= 20 && setting.Hints?.EnumPreferButtonGroup != false)) {
                var go = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ConnectedButtonGroupSetting"), parent);
                go.AddComponent<ConnectedButtonGroupSettingController>().TargetSetting = setting;
                return go;
            } else {
                var go = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "DropdownSetting"), parent);
                go.AddComponent<DropdownSettingController>().TargetSetting = setting;
                return go;
            }
        },
        [SettingType.Float] = (setting, parent) => {
            var go = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "NumberSetting"), parent);
            go.AddComponent<FloatSettingController>().TargetSetting = (Setting<float>)setting;
            return go;
        },
        [SettingType.Int] = (setting, parent) => {
            var go = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "NumberSetting"), parent);
            go.AddComponent<IntSettingController>().TargetSetting = (Setting<int>)setting;
            return go;
        },
        [SettingType.Text] = (setting, parent) => {
            var go = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "TextSetting"), parent);
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
            categoryIcon.sprite = AssetManager.Get<Sprite>(ClickGUI.BundleKey, module.IconName);
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
        var desc = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ModuleDescription"), listBody);
        desc.FindRecursive("DescText").GetComponent<TextMeshProUGUI>().text = TargetConfigurable.Description;
        if (TargetConfigurable is not SystemModule) {
            var enabledButton = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "EnabledButton"), listBody);
            var enabledButtonComp = enabledButton.AddComponent<EnabledButtonController>();
            enabledButtonComp.Configurable = TargetConfigurable;
        }

        if (TargetConfigurable is HudModule hudModule) {
            var dragArea = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "RemoteDrag"), listBody);
            var dragComp = dragArea.AddComponent<HudElementPositioningController>();
            dragComp.TargetModule = hudModule;
            if (hudModule.UIElement != null) dragComp.target = hudModule.UIElement.transform as RectTransform;
        }

        foreach (var element in TargetConfigurable.Settings) {
            if (element is not { } setting) continue;

            GameObject wrapper = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "SettingRowWrapper"), listBody);
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
