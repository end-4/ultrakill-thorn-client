using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class ButtonRowSettingController : MonoBehaviour {
    public ConfigButtonRow? TargetButtonRow;

    private void Start() {
        if (TargetButtonRow == null) return;
        Populate();
    }

    private void Populate() {
        if (TargetButtonRow == null) return;
        for (int i = 0; i < TargetButtonRow.Texts.Count; i++) {
            string curr = TargetButtonRow.Texts[i];
            var btn = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ButtonRowButton"), transform);
            var txtComp = btn.FindRecursive("Text");
            if (txtComp != null) txtComp.GetComponent<TextMeshProUGUI>().text = curr;
            var iCopied = i;
            btn.GetComponent<Button>().onClick.AddListener(() => Trigger(iCopied));
        }
    }

    private void Trigger(int idx) {
        if (TargetButtonRow == null) return;
        TargetButtonRow.OnClick?.Invoke(idx);
    }
}
