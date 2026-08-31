using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

// This is for the nested windows
// Quite the same as ConfigurableWindowController, maybe give each configurable a SettingGroup for deduplication?
internal class SettingGroupWindowController : MonoBehaviour {
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
        if (backBtn != null) backBtn.onClick.AddListener(() => Destroy(gameObject));

        // Body. Note we're reusing ModuleCategory prefab for this
        Transform listBody = gameObject.FindRecursive("Scroll View/Viewport/Content/Modules")!.transform;
        Populate(listBody, TargetGroup.Elements);
        gameObject.UnfuckLayoutHack();
    }

    private void Populate(Transform parent, IEnumerable<IConfigurableElement> elements) {
        foreach (var element in elements) {
            // TODO un-copypaste from ConfigurableWindowController maybe
            GameObject? wrapper = null;

            GameObject? obj = null;
            if (UICreatorManager.TryGetUICreator(element.GetType(), out var creator) && creator != null) {
                obj = creator.CreateUI(element);
            } else {
                continue;
            }

            if (element is Setting setting) {
                wrapper = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "SettingRowWrapper"), parent);
                wrapper!.AddComponent<SettingRowController>().TargetSetting = setting;
                obj?.transform.SetParent(wrapper.transform, false);
            } else {
                wrapper = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "SettingRowWrapper"), parent);
                obj?.transform.SetParent(wrapper!.transform, false);
            }

            if (element is not ConfigHeader && obj != null)
                obj.AddComponent<SettingDescriptionController>().TargetSetting = element;

            if (wrapper != null) {
                wrapper.UnfuckLayoutHack();
                if (element.Hints?.Hidden ?? false) {
                    wrapper.SetActive(false);
                }
            }
        }
    }
}
