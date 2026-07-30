using NukeLib.UI;
using ThornClient.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class SettingDescriptionController : MonoBehaviour {
    public IConfigurableElement TargetSetting;

    private void Start() {
        var targetObj = gameObject.FindRecursive("Name", warnings: false);
        if (targetObj == null) targetObj = gameObject.FindRecursive("TopRow/Name", warnings: false);
        if (targetObj == null) targetObj = gameObject.FindRecursive("MainField/Name");
        if (targetObj == null) return;
        var targetComp = targetObj.GetComponent<TextMeshProUGUI>();
        targetComp.text = TargetSetting.Name;
        var hoverComp = gameObject.GetOrAddComponent<ClickGUITooltipHandler>();
        hoverComp.text = TargetSetting.Description;
    }
}
