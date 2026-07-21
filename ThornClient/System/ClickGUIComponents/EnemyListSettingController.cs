using Notiffy.API;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.DataTypes;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class EnemyListSettingController : MonoBehaviour {
    public Setting<EnemyList>? TargetSetting;
    private TextMeshProUGUI? text;

    private void Start() {
        if (TargetSetting == null) return;
        GetComponent<Button>().onClick.AddListener(OpenList);
        text = gameObject.FindRecursive("Name").GetComponent<TextMeshProUGUI>();
        TargetSetting.OnValueChanged += UpdateText;
        UpdateText(TargetSetting.Value);
    }

    private void UpdateText(EnemyList elist) {
        if (text != null && TargetSetting != null) text.text = $"{TargetSetting.Name} ({elist.Count()})";
    }

    private void OnDestroy() {
        if (TargetSetting != null) TargetSetting.OnValueChanged -= UpdateText;
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.RemoveListener(OpenList);
    }

    private void OpenList() {
        var panel = CreateConfigPanel(TargetSetting);
        if (panel != null) ClickGUI.NestPanel(panel);
        ClickGUI.SurrenderTooltipText(TargetSetting?.Description ?? "");
    }

    private GameObject? CreateConfigPanel(Setting<EnemyList>? setting) {
        if (ClickGUI.EnemyListPrefab == null || setting == null) return null;

        var obj = Instantiate(ClickGUI.EnemyListPrefab);
        if (obj == null) return null;

        var ctl = obj.GetOrAddComponent<EnemyListController>();
        ctl.IsPopup = true;
        ctl.TargetList = setting;

        return obj;
    }
}
