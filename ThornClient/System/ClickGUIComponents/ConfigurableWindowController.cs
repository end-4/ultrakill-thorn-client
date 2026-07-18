using NukeLib.UI;
using ThornClient.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

internal class ConfigurableWindowController : MonoBehaviour {
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
        if (IsPopup) backBtn.onClick.AddListener(ClickGUI.CloseConfig);

        // Populate with settings
        Transform listBody = gameObject.FindRecursive("Modules").transform; // Note that we reuse ModuleCategory prefab for this
        var desc = Instantiate(ClickGUI.ModuleDescriptionPrefab!, listBody);
        desc.FindRecursive("DescText").GetComponent<TextMeshProUGUI>().text = TargetConfigurable.Description;
        if (TargetConfigurable is not SystemModule) {
            var enabledButton = Instantiate(ClickGUI.EnabledButtonPrefab, listBody);
            var enabledButtonComp = enabledButton.AddComponent<EnabledButtonController>();
            enabledButtonComp.configurable = TargetConfigurable;
        }

        foreach (var setting in TargetConfigurable.Settings) {
            GameObject wrapper = Object.Instantiate(ClickGUI.SettingRowWrapperPrefab, listBody);
            var wrapperComp = wrapper.AddComponent<SettingRowController>();
            wrapperComp.TargetSetting = setting;
            GameObject go = null;
            switch (setting.Type) {
                case SettingType.Bool:
                    go = Instantiate(ClickGUI.BoolSettingPrefab, wrapper.transform);
                    var boolComp = go.AddComponent<BoolSettingController>();
                    boolComp.TargetSetting = (Setting<bool>)setting;
                    break;
                case SettingType.Float:
                    go = Instantiate(ClickGUI.NumberSettingPrefab, wrapper.transform);
                    var floatComp = go.AddComponent<FloatSettingController>();
                    floatComp.TargetSetting = (Setting<float>)setting;
                    break;
                case SettingType.Int:
                    go = Instantiate(ClickGUI.NumberSettingPrefab, wrapper.transform);
                    var intComp = go.AddComponent<IntSettingController>();
                    intComp.TargetSetting = (Setting<int>)setting;
                    break;
                case SettingType.Bind:
                    go = Instantiate(ClickGUI.KeybindSettingPrefab, wrapper.transform);
                    var keyComp = go.AddComponent<KeybindSettingController>();
                    keyComp.TargetSetting = (Setting<Keybind>)setting;
                    break;
                case SettingType.Text:
                    go = Instantiate(ClickGUI.TextSettingPrefab, wrapper.transform);
                    var strComp = go.AddComponent<TextSettingController>();
                    strComp.TargetSetting = (Setting<string>)setting;
                    break;
                case SettingType.Color:
                    go = Instantiate(ClickGUI.ColorSettingPrefab, wrapper.transform);
                    var colComp = go.AddComponent<ColorSettingController>();
                    colComp.TargetSetting = (Setting<Color>)setting;
                    break;
            }

            if (go != null) {
                var descCon = go.AddComponent<SettingDescriptionController>();
                descCon.TargetSetting = setting;
            }

            wrapper.UnfuckLayoutHack();
        }

        gameObject.UnfuckLayoutHack();
    }
}
