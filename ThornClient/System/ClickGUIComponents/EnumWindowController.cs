using System;
using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// The component that controls an enum selection window
/// </summary>
public class EnumWindowController : MonoBehaviour {
    private bool _doneSetup = false;

    /// <summary>
    /// The setting this component controls
    /// </summary>
    public Setting? TargetSetting;

    private void Start() {
        SetupStuff();
    }

    public void SetupStuff() {
        if (_doneSetup) return;
        _doneSetup = true;

        if (TargetSetting == null) return;

        // Header: icon, text, dragging behavior
        gameObject.FindRecursive("Header/TitleButton/TitleIcon")!.SetActive(false);
        var categoryText = gameObject.FindRecursive("Header/TitleName")!.GetComponent<TextMeshProUGUI>();

        categoryText.text = TargetSetting.Name;
        gameObject.FindRecursive("Header")?.AddComponent<TitlebarDragHandler>();
        var backBtn = gameObject.FindRecursive("Header/TitleButton")?.GetComponent<Button>();

        gameObject.FindRecursive("Header/TitleButton")!.GetComponent<Button>().interactable = true;
        gameObject.FindRecursive("Header/TitleButton/BackIcon")?.SetActive(true);
        if (backBtn != null) backBtn.onClick.AddListener(Back);

        // Body. Note we're reusing ModuleCategory prefab for this
        Transform listBody = gameObject.FindRecursive("Scroll View/Viewport/Content/Modules")!.transform;
        Populate(listBody, TargetSetting);
        gameObject.UnfuckLayoutHack();

        if (TargetSetting != null) TargetSetting.OnChanged += Back;
    }

    private void Back() {
        if (TargetSetting != null) TargetSetting.OnChanged -= Back;
        Destroy(gameObject);
    }

    private void Populate(Transform parent, Setting? setting) {
        if (parent == null || setting == null) return;
        var optionPrefab = AssetManager.Get<GameObject>(ClickGUI.BundleKey, "RadioChoiceButton");
        var enumType = setting.GetValue().GetType();
        var enumVals = Enum.GetValues(enumType);
        foreach (var e in enumVals) {
            var btn = Instantiate(optionPrefab, parent);
            var comp = btn.AddComponent<RadioChoiceEnumSettingController>();
            comp.TargetValue = e;
            comp.TargetSetting = setting;
        }
    }
}
