using NukeLib.UI;
using TMPro;
using UnityEngine;

namespace ThornClient.System.ClickGUIComponents;

internal class TabBarController : MonoBehaviour {
    private void Start() {
        var versionText = gameObject.FindRecursive("InfoCol/Version").GetComponent<TextMeshProUGUI>();
        versionText.text = "v" + Plugin.PluginVersion;
    }
}
