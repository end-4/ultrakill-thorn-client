using System;
using NukeLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

internal class TabBarController : MonoBehaviour {
    private TextMeshProUGUI _numberText;
    private TextMeshProUGUI _amPmText;
    private Image _iconImage;
    private Sprite _sunIcon = ClickGUI.Bundle.LoadAsset<Sprite>("sun");
    private Sprite _moonIcon = ClickGUI.Bundle.LoadAsset<Sprite>("moon");

    private void Start() {
        var versionText = gameObject.FindRecursive("InfoCol/Version").GetComponent<TextMeshProUGUI>();
        versionText.text = "v" + Plugin.PluginVersion;

        _numberText = gameObject.FindRecursive("Clock/Time/Number").GetComponent<TextMeshProUGUI>();
        _amPmText = gameObject.FindRecursive("Clock/Time/AmPm").GetComponent<TextMeshProUGUI>();
        _iconImage = gameObject.FindRecursive("Clock/Icon").GetComponent<Image>();
    }

    private void Update() {
        if (!gameObject.activeInHierarchy) return;
        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay() {
        var now = DateTime.Now;
        string newNumber = now.ToString("hh:mm");
        string newAmPm = now.ToString("tt");
        var newIcon = (now.Hour is >= 6 and < 18) ? _sunIcon : _moonIcon;

        if (_numberText != null && _numberText.text != newNumber) {
            _numberText.text = newNumber;
        }

        if (_amPmText != null && _amPmText.text != newAmPm) {
            _amPmText.text = newAmPm;
        }

        if (_iconImage != null && _iconImage.sprite != newIcon) {
            _iconImage.sprite = newIcon;
        }
    }
}
