using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class SettingRowController : MonoBehaviour {
    public Setting? TargetSetting;
    private GameObject? _resetButton;
    private Image? _resetButtonIconImage;
    private Button _resetButtonComp;

    private void Start() {
        if (TargetSetting == null) return;
        _resetButton = gameObject.FindRecursive("RevertButtonWrapper/RevertButton");
        _resetButtonIconImage = _resetButton.FindRecursive("Icon").GetComponent<Image>();
        if (_resetButton == null) return;
        _resetButtonComp = _resetButton.GetComponent<Button>();
        UpdateResetVisibility();
        _resetButtonComp.onClick.AddListener(ResetSetting);
        TargetSetting.OnChanged += UpdateResetVisibility;
    }

    private void OnDestroy() {
        if (TargetSetting != null) TargetSetting.OnChanged -= UpdateResetVisibility;
        if (_resetButtonComp != null) _resetButtonComp.onClick.RemoveListener(ResetSetting);
    }

    private void UpdateResetVisibility() {
        if (TargetSetting == null || _resetButton == null || _resetButtonIconImage == null) return;
        bool revertable = !TargetSetting.IsDefault;
        _resetButtonIconImage.sprite = AssetManager.Get<Sprite>(ClickGUI.BundleKey, revertable ? "revert" : "dot");
        _resetButtonComp.interactable = revertable;
    }

    private void ResetSetting() {
        if (TargetSetting == null) return;
        TargetSetting.Reset();
    }
}
