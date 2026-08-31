using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.HUD.HUDComponents;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

internal class ConfigurableWindowController : MonoBehaviour {
    private bool _doneSetup = false;
    public bool IsPopup = false;
    public Configurable? TargetConfigurable;

    private void Start() {
        SetupStuff();
    }

    public void SetupStuff() {
        if (_doneSetup) return;
        _doneSetup = true;

        if (TargetConfigurable == null) return;

        // Header: icon, text, dragging behavior
        var categoryIcon = gameObject.FindRecursive("Header/TitleButton/TitleIcon")?.GetComponent<Image>();
        var categoryText = gameObject.FindRecursive("Header/TitleName")?.GetComponent<TextMeshProUGUI>();
        if (TargetConfigurable is Module module && categoryIcon != null) {
            categoryIcon.sprite = module.Icon;
        }

        categoryText.text = TargetConfigurable.Name;
        gameObject.FindRecursive("Header")?.AddComponent<TitlebarDragHandler>();
        var backBtn = gameObject.FindRecursive("Header/TitleButton")?.GetComponent<Button>();
        gameObject.FindRecursive("Header/TitleButton").GetComponent<Button>().interactable = IsPopup;
        gameObject.FindRecursive("Header/TitleButton/BackIcon")?.SetActive(IsPopup);
        if (IsPopup) backBtn.onClick.AddListener(ClickGUI.NavigateBack);

        // Body
        Transform listBody =
            gameObject.FindRecursive("Scroll View/Viewport/Content/Modules")
                .transform; // Note we reuse ModuleCategory prefab for this

        var desc = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ModuleDescription"), listBody);
        desc.FindRecursive("DescText").GetComponent<TextMeshProUGUI>().text = TargetConfigurable.Description;

        if (TargetConfigurable.HasToggling) {
            var enabledButton =
                Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "EnabledButton"), listBody);
            var enabledButtonComp = enabledButton.AddComponent<EnabledButtonController>();
            enabledButtonComp.Configurable = TargetConfigurable;
        }

        if (TargetConfigurable is HudModule hudModule) {
            var dragArea = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "RemoteDrag"), listBody);
            var dragComp = dragArea.AddComponent<HudElementPositioningController>();
            dragComp.TargetModule = hudModule;
            if (hudModule.UIElement != null) dragComp.target = hudModule.UIElement.transform as RectTransform;
        }

        Populate(listBody, TargetConfigurable.Elements);

        gameObject.UnfuckLayoutHack();
    }

    internal static void Populate(Transform parent, IEnumerable<IConfigurableElement> elements) {
        foreach (var element in elements) {
            if (!UICreatorManager.TryGetUICreator(element.GetType(), out var creator) || creator == null) {
                continue;
            }

            var obj = creator.CreateUI(element);
            var wrapper = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "SettingRowWrapper"), parent);

            if (element is Setting setting) {
                wrapper.AddComponent<SettingRowController>().TargetSetting = setting;
            }

            obj?.transform.SetParent(wrapper.transform, false);

            if (element is not ConfigHeader && obj != null) {
                obj.AddComponent<SettingDescriptionController>().TargetSetting = element;
            }

            wrapper.UnfuckLayoutHack();
            if (element.Hints?.Hidden ?? false) {
                wrapper.SetActive(false);
            }
        }
    }
}
