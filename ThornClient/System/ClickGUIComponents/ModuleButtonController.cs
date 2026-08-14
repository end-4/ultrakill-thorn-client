using System;
using NukeLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Module = ThornClient.Core.Module;

namespace ThornClient.System.ClickGUIComponents;

internal class ModuleButtonController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Module? TargetModule;
    private Colorizer? _iconColorizer;
    private Colorizer? _textColorizer;
    private GameObject? _settingIcon;

    private void Start() {
        if (TargetModule == null) {
            Plugin.Log.LogWarning("[ModuleButtonController] TargetModule not assigned!");
            return;
        }

        // Set icon
        var buttonIcon = gameObject.FindRecursive("Icon")?.GetComponent<Image>();
        try {
            if (buttonIcon != null) buttonIcon.sprite = TargetModule.Icon;
        } catch (Exception e) {
            Plugin.Log.LogError($"Failed to load icon for module '{TargetModule.Name}': {e}");
        }

        // Set text
        var buttonText = gameObject.FindRecursive("Name")?.GetComponent<TextMeshProUGUI>();
        buttonText?.SetText(TargetModule.Name);

        // Setup clicks
        var button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(() => { TargetModule.Toggle(); });
        _settingIcon = gameObject.FindRecursive("ConfigButton/Icon");
        _settingIcon?.SetActive(false);
        var settingButtonComp = gameObject.FindRecursive("ConfigButton")?.GetComponent<Button>();
        if (settingButtonComp != null) settingButtonComp.onClick.AddListener(() => { ClickGUI.NestConfigPanel(TargetModule); });

        // Setup hover
        var providerName = TargetModule.GetType().Assembly.GetName().Name;
        var tooltipComp = gameObject.AddComponent<ClickGUITooltipHandler>();
        tooltipComp.Text = $"{TargetModule.Description}<size=8>\n\n</size><size=10>[<color=#{ColorUtility.ToHtmlStringRGB(ThornModule.AccentColor)}>{providerName}</color>]</size>";
        // tooltipComp.text = $"{TargetModule.Description}\n\n[{providerName}.dll]";

        // Setup visuals
        var settingIcon = gameObject.FindRecursive("ConfigButton/Icon");
        _iconColorizer = buttonIcon.GetOrAddComponent<Colorizer>();
        _textColorizer = buttonText.GetOrAddComponent<Colorizer>();

        TargetModule.OnToggleStateChanged += UpdateVisualState;
        UpdateVisualState(TargetModule.IsEnabled);
    }

    private void OnDestroy() {
        if (TargetModule == null) return;
        TargetModule.OnToggleStateChanged -= UpdateVisualState;
    }

    private void UpdateVisualState(bool isEnabled) {
        if (_iconColorizer != null) _iconColorizer.Highlighted = isEnabled;
        if (_textColorizer != null) _textColorizer.Highlighted = isEnabled;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            ClickGUI.NestConfigPanel(TargetModule);
            _settingIcon?.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _settingIcon?.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        _settingIcon?.SetActive(false);
    }
}
