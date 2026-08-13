using NukeLib.UI;
using ThornClient.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ThornClient.Managers;

namespace ThornClient.System.ClickGUIComponents;

internal class EnabledButtonController : MonoBehaviour {
    public Configurable? Configurable;
    private TextMeshProUGUI? _textComp;
    private Image? _buttonImageComp;
    private Colorizer? _colorizer;

    private void Start() {
        if (Configurable == null) return;
        GetComponent<Button>().onClick.AddListener(ToggleIt);
        _textComp = gameObject.FindRecursive("Text")?.GetComponent<TextMeshProUGUI>();
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
            _buttonImageComp.sprite = 
            AssetManager.Get<Sprite>(ClickGUI.BundleKey, isEnabled ? "Round_FillLarge" : "Round_BorderLarge");
        }

        if (_colorizer != null) {
            _colorizer.Highlighted = isEnabled;
        }
    }
}
