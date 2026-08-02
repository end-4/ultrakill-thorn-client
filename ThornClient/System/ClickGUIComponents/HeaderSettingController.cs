using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class HeaderSettingController : MonoBehaviour {
    public ConfigHeader? TargetElement;

    private void Start() {
        if (TargetElement == null) return;
        var category = gameObject.FindRecursive("Name", warnings: false)?.GetComponent<TextMeshProUGUI>();
        if (category != null) {
            category.text = TargetElement.Name;
            category.fontSize = TargetElement.FontSize;
        }
        var desc = gameObject.FindRecursive("DescLayout/Description", warnings: false)?.GetComponent<TextMeshProUGUI>();
        if (desc != null) {
            desc.text = TargetElement.Description;
        }

        var resetButton = transform.parent.gameObject.FindRecursive("RevertButtonWrapper");
        if (resetButton != null) resetButton.SetActive(false);
    }
}
