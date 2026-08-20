using System;
using NukeLib.UI;
using NukeLib.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ThornClient.Managers;

namespace ThornClient.System.ClickGUIComponents;

internal class TabBarController : MonoBehaviour {
    private TextMeshProUGUI? _numberText;
    private TextMeshProUGUI? _amPmText;
    private Image? _iconImage;
    private GameObject? _battObj;
    private Image? _battIcon;
    private TextMeshProUGUI? _battNum;
    private Sprite _sunIcon;
    private Sprite _moonIcon;

    private void Awake() {
        _sunIcon = AssetManager.Get<Sprite>(ClickGUI.BundleKey, "sun");
        _moonIcon = AssetManager.Get<Sprite>(ClickGUI.BundleKey, "moon");
    }

    private void Start() {
        var versionText = gameObject.FindRecursive("InfoRow/InfoCol/Version")?.GetComponent<TextMeshProUGUI>();
        if (versionText != null) versionText.text = "v" + Plugin.PluginVersion;

        _numberText = gameObject.FindRecursive("SystemStatus/Clock/Time/Number")?.GetComponent<TextMeshProUGUI>();
        _amPmText = gameObject.FindRecursive("SystemStatus/Clock/Time/AmPm")?.GetComponent<TextMeshProUGUI>();
        _iconImage = gameObject.FindRecursive("SystemStatus/Clock/Icon")?.GetComponent<Image>();

        _battObj = gameObject.FindRecursive("SystemStatus/Battery");
        if (_battObj) {
            _battIcon = _battObj.FindRecursive("Icon")?.GetComponent<Image>();
            _battNum = _battObj.FindRecursive("Value/Number")?.GetComponent<TextMeshProUGUI>();
        }
    }

    private void Update() {
        // Don't update if menu not open... hmm is this necessary
        if (!gameObject.activeInHierarchy) return;
        UpdateTimeDisplay();
        UpdateBattery();
    }

    private void UpdateTimeDisplay() {

        var now = DateTime.Now;
        bool use24h = ThornModule.Instance?.TimeFormat.Value == ThornModule.TimeHourFormat.TwentyFour;

        string format = use24h ? "HH:mm" : "hh:mm";
        string newNumber = now.ToString(format);
        string newAmPm = use24h ? string.Empty : now.ToString("tt");

        var newIcon = (now.Hour is >= 6 and < 18) ? _sunIcon : _moonIcon;

        if (_numberText != null && _numberText.text != newNumber) {
            _numberText.text = newNumber;
        }

        if (_amPmText != null) {
            if (_amPmText.gameObject.activeSelf == use24h) {
                _amPmText.gameObject.SetActive(!use24h);
                gameObject.UnfuckLayoutHack();
                ExecutionUtils.RunNextFrame(() => { if (gameObject != null) gameObject.UnfuckLayoutHack(); });
            }

            if (_amPmText.text != newAmPm) {
                _amPmText.text = newAmPm;
            }
        }

        if (_iconImage != null && _iconImage.sprite != newIcon) {
            _iconImage.sprite = newIcon;
        }


    }

    private void UpdateBattery() {
        var status = SystemInfo.batteryStatus;
        if (status == BatteryStatus.Unknown || (SystemInfo.batteryLevel is < 0 or > 1) ) { // No battery
            if (_battObj != null && _battObj.activeSelf) _battObj.SetActive(false);
            return;
        }
        var charging = status == BatteryStatus.Charging || status == BatteryStatus.Full;
        var perc = SystemInfo.batteryLevel * 100; // Raw value in [0, 1]
        var newPercStr = $"{perc}";

        int iconIndex = Math.Clamp((int)Math.Round(perc / 25f), 0, 4);
        string iconLevel = iconIndex == 0 ? "low" : $"{iconIndex * 25}";
        string chargeStatTxt = charging ? "_charge" : "";

        string iconName = $"battery_{iconLevel}{chargeStatTxt}";

        if (_battNum != null && _battNum.text != newPercStr) _battNum.text = newPercStr;
        if (_battIcon != null && _battIcon.sprite?.name != iconName) _battIcon.sprite = AssetManager.Get<Sprite>(ClickGUI.BundleKey, iconName);
    }
}
