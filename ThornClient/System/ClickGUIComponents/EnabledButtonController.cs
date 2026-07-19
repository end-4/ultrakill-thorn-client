using NukeLib.UI;
using ThornClient.Core;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class EnabledButtonController : MonoBehaviour {
    public Configurable? Configurable;
    private TextMeshProUGUI? _textComp;
    private Image? _buttonImageComp;
    private Colorizer? _colorizer;

    private void Start() {
        if (Configurable == null) return;
        GetComponent<Button>().onClick.AddListener(ToggleIt);
        _textComp = gameObject.FindRecursive("Text").GetComponent<TextMeshProUGUI>();
        _buttonImageComp = gameObject.GetComponent<Image>();
        _colorizer = gameObject.GetOrAddComponent<Colorizer>();
        Configurable.OnToggleStateChanged += UpdateAppearance;
        UpdateAppearance(Configurable.IsEnabled);
    }

    private void OnDestroy() {
        if (Configurable == null) return;
        GetComponent<Button>().onClick.RemoveListener(ToggleIt);
        Configurable.OnToggleStateChanged -= UpdateAppearance;
    }

    private void ToggleIt() {
        Configurable?.Toggle();
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

        if (_colorizer != null) {
            _colorizer.UpdateHighlight(isEnabled);
        }
    }
}
