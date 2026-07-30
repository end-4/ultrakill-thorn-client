using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.UI;
using ThornClient.HUD.HUDComponents;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

// Quite the same as ConfigurableWindowController, maybe give each configurable a SettingGroup for deduplication?
public class SettingGroupWindowController : MonoBehaviour {
    private bool _doneSetup = false;
    public SettingGroup TargetGroup;

    private void Start() {
        SetupStuff();
    }

    public void SetupStuff() {
        if (_doneSetup) return;
        _doneSetup = true;

        if (TargetGroup == null) return;

        // Header: icon, text, dragging behavior
        gameObject.FindRecursive("Header/TitleButton/TitleIcon")!.SetActive(false); // No need icon for subsection
        var categoryText = gameObject.FindRecursive("Header/TitleName")!.GetComponent<TextMeshProUGUI>();

        categoryText.text = TargetGroup.Name;
        gameObject.FindRecursive("Header")?.AddComponent<TitlebarDragHandler>();
        var backBtn = gameObject.FindRecursive("Header/TitleButton")?.GetComponent<Button>();

        gameObject.FindRecursive("Header/TitleButton")!.GetComponent<Button>().interactable = true;
        gameObject.FindRecursive("Header/TitleButton/BackIcon")?.SetActive(true);
        backBtn!.onClick.AddListener(() => Destroy(gameObject));

        // Body. Note we're reusing ModuleCategory prefab for this
        Transform listBody = gameObject.FindRecursive("Modules")!.transform;
        Populate(listBody, TargetGroup.Elements);
        gameObject.UnfuckLayoutHack();
    }

    private void Populate(Transform parent, IEnumerable<IConfigurableElement> elements) {
        foreach (var element in elements) {
            GameObject? wrapper = null;

            if (element is Setting setting) {
                wrapper = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "SettingRowWrapper"), parent);
                wrapper.AddComponent<SettingRowController>().TargetSetting = setting;

                if (ConfigurableElementUICreators.SettingUICreators.TryGetValue(setting.Type, out var createUI)) {
                    GameObject go = createUI(setting, wrapper.transform);
                    go.AddComponent<SettingDescriptionController>().TargetSetting = setting;
                }
            } else if (element is SettingGroup || element is ConfigButtonRow) {
                // TODO
                wrapper = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "SettingRowWrapper"), parent);
                if (ConfigurableElementUICreators.MenuUICreators.TryGetValue(element.GetType(), out var createUI)) {
                    var go = createUI(element, wrapper.transform);
                    go.AddComponent<SettingDescriptionController>().TargetSetting = element;
                }
            }

            if (wrapper != null) {
                wrapper.UnfuckLayoutHack();
                if (element.Hints?.Hidden ?? false) {
                    wrapper.SetActive(false);
                }
            }
        }
    }
}
