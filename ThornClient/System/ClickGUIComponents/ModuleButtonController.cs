using System;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Module = ThornClient.Core.Module;

namespace ThornClient.System.ClickGUIComponents;

internal class ModuleButtonController : MonoBehaviour, IPointerClickHandler {
    public Module? TargetModule;
    private GameObject? _checkmark;
    private Colorizer? _iconColorizer;
    private Colorizer? _textColorizer;
    private Colorizer? _checkIconColorizer;

    private void Start() {
        if (TargetModule == null) {
            Plugin.Log.LogWarning("ModuleButtonController has no TargetModule assigned");
            return;
        }

        _checkmark = gameObject.FindRecursive("Checkbox/Mark");

        // Set icon
        var buttonIcon = gameObject.FindRecursive("Icon").GetComponent<Image>();
        try {
            buttonIcon.sprite = AssetManager.Get<Sprite>(ClickGUI.BundleKey, TargetModule.IconName);
        } catch (Exception e) {
            Plugin.Log.LogError($"Failed to load icon for module '{TargetModule.Name}': {e}");
        }

        // Set text
        var buttonText = gameObject.FindRecursive("Name").GetComponent<TextMeshProUGUI>();
        buttonText.text = TargetModule.Name;

        // Setup click
        var button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(() => { TargetModule.Toggle(); });

        // Setup hover
        var providerName = TargetModule.GetType().Assembly.GetName().Name;
        var tooltipComp = gameObject.AddComponent<ClickGUITooltipHandler>();
        // tooltipComp.text = $"{TargetModule.Description}<size=8>\n</size><size=10><color=#{ColorUtility.ToHtmlStringRGB(ThornModule.AccentColor)}>{providerName}</color></size>";
        tooltipComp.text = $"{TargetModule.Description}\n\n[{providerName}]";

        // Setup visuals
        _iconColorizer = buttonIcon.GetOrAddComponent<Colorizer>();
        _textColorizer = buttonText.GetOrAddComponent<Colorizer>();
        _checkmark.SetActive(true);
        _checkIconColorizer = _checkmark.GetOrAddComponent<Colorizer>();
        _checkmark.SetActive(false);
        TargetModule.OnToggleStateChanged += UpdateVisualState;
        UpdateVisualState(TargetModule.IsEnabled);
    }

    private void OnDestroy() {
        if (TargetModule == null) return;
        TargetModule.OnToggleStateChanged -= UpdateVisualState;
    }

    private void UpdateVisualState(bool isEnabled) {
        if (_checkmark == null) return;
        _checkmark.SetActive(isEnabled);
        if (_iconColorizer != null) _iconColorizer.UpdateHighlight(isEnabled);
        if (_textColorizer != null) _textColorizer.UpdateHighlight(isEnabled);
        if (_checkIconColorizer != null) _checkIconColorizer.UpdateHighlight(isEnabled);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            if (TargetModule == null) return;
            var panel = CreateConfigPanel(TargetModule);
            if (panel != null) ClickGUI.NestPanel(panel);
            ClickGUI.SurrenderTooltipText(TargetModule.Description);
        }
    }

    private static GameObject? CreateConfigPanel(Configurable config) {
        if (AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ModuleCategory") == null) return null;

        var configurableObj = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ModuleCategory"));
        if (configurableObj == null) {
            return null;
        }

        // Add controller
        var configController = configurableObj.GetOrAddComponent<ConfigurableWindowController>();
        configController.IsPopup = true;
        configController.TargetConfigurable = config;

        return configurableObj;
    }
}
