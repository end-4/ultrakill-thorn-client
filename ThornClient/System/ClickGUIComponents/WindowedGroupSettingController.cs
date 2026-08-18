using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// This is for the button that opens up a new window, not the window itself
/// </summary>
internal class WindowedGroupSettingController : MonoBehaviour {
    public SettingGroup? TargetGroup;
    private TextMeshProUGUI? text;
    private GameObject? _panel;

    private void Start() {
        if (TargetGroup == null) return;
        GetComponent<Button>().onClick.AddListener(OpenWindow);
        text = gameObject.FindRecursive("Name")?.GetComponent<TextMeshProUGUI>();
        if (text != null) text.text = TargetGroup.Name;
    }

    private void OnDestroy() {
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.RemoveListener(OpenWindow);
    }

    private void OpenWindow() {
        if (_panel != null) return;
        _panel = CreateConfigPanel(TargetGroup);
        if (_panel != null) ClickGUI.SpawnContent(_panel);
        ClickGUI.SurrenderTooltipText(TargetGroup?.Description ?? "");
    }

    private GameObject? CreateConfigPanel(SettingGroup? settingGroup) {
        GameObject? go = AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ModuleCategory");
        if (go == null || settingGroup == null) return null;

        var obj = Instantiate(go);
        if (obj == null) return null;

        var ctl = obj.GetOrAddComponent<SettingGroupWindowController>();
        ctl.TargetGroup = settingGroup;

        return obj;
    }
}
