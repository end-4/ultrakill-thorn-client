using NukeLib.UI;
using ThornClient.Core;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class EnabledButtonController : MonoBehaviour {
    public Configurable configurable;
    private TextMeshProUGUI? _textComp;
    private Image? _buttonImageComp;

    private void Start() {
        if (configurable == null) return;
        GetComponent<Button>().onClick.AddListener(ToggleIt);
        _textComp = gameObject.FindRecursive("Text").GetComponent<TextMeshProUGUI>();
        _buttonImageComp = gameObject.GetComponent<Image>();
        configurable.OnToggleStateChanged += UpdateAppearance;
        UpdateAppearance(configurable.IsEnabled);
    }

    private void OnDestroy() {
        GetComponent<Button>().onClick.RemoveListener(ToggleIt);
        configurable.OnToggleStateChanged -= UpdateAppearance;
        if (configurable == null) return;
    }

    private void ToggleIt() {
        configurable.Toggle();
    }

    private void UpdateAppearance(bool isEnabled) {
        if (_textComp != null) {
            _textComp.text = isEnabled ? "Enabled" : "Disabled";
            _textComp.color = isEnabled ? Color.black : Color.white;
        }

        if (_buttonImageComp != null) {
            _buttonImageComp.sprite = ClickGUI.Bundle
                .LoadAsset<Sprite>(isEnabled ? "Round_FillLarge" : "Round_BorderLarge");
        }
    }
}
