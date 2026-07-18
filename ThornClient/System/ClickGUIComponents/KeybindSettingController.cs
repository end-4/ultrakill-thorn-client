using System.Collections.Generic;
using Notiffy.API;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class KeybindSettingController : MonoBehaviour {
    private static readonly Color NormalColor = new Color(1, 1, 1);
    private static readonly Color ListeningColor = new Color(1f, 0.4048f, 0f);

    private static KeybindSettingController _listeningInstance = null;
    private Button? _activateButton;
    private Image? _keyBorder;
    private TextMeshProUGUI _displayText;
    public Setting<Keybind> TargetSetting;

    private KeyCode _caughtModifier = KeyCode.None;
    private KeyCode _caughtKey = KeyCode.None;

    private static OptionsManager? opts => OptionsManager.Instance;

    private void Start() {
        _activateButton = gameObject.GetComponent<Button>();
        _keyBorder = gameObject.FindRecursive("ValueDisplay").GetComponent<Image>();
        _displayText = gameObject.FindRecursive("ValueDisplay/Text").GetComponent<TextMeshProUGUI>();
        _activateButton.onClick.AddListener(ActivateKeyListen);
        UpdateKeyDisplay();
        TargetSetting.OnValueChanged += UpdateKeyDisplay;
    }

    internal void SetBorderColor(Color color) {
        if (_keyBorder == null) return;
        _keyBorder.color = color;
    }

    private void ActivateKeyListen() {
        if (_listeningInstance != null) {
            _listeningInstance.SetBorderColor(NormalColor);
        }

        _listeningInstance = this;
        SetBorderColor(ListeningColor);
        _caughtModifier = KeyCode.None;
        _caughtKey = KeyCode.None;
    }

    private void DeactivateKeyListen() {
        if (IsListening()) {
            _listeningInstance = null;
        }

        SetBorderColor(NormalColor);

        // Try to bind
        if (_caughtModifier != KeyCode.None && _caughtKey == KeyCode.None) {
            _caughtKey = _caughtModifier;
            _caughtModifier = KeyCode.None;
        }
        TargetSetting.Value.Key = _caughtKey;
        TargetSetting.Value.Modifier = _caughtModifier;
        // Set to itself to trigger change + save
        TargetSetting.Value = TargetSetting.Value;
        UpdateKeyDisplay();
    }

    private bool IsListening() {
        return _listeningInstance == this;
    }

    private void UpdateKeyDisplay() {
        string newText = "";
        bool isListening = IsListening();
        var modKey = isListening ? _caughtModifier : TargetSetting.Value.Modifier;
        var key = isListening ? _caughtKey : TargetSetting.Value.Key;
        if (modKey != KeyCode.None) {
            newText += modKey.ToString();
        }
        if (key != KeyCode.None) {
            if (newText.Length > 0) newText += "+";
            newText += key.ToString();
        }

        if (newText.Length == 0) newText = "--";

        if (_displayText.text != newText) {
            _displayText.text = newText;
        }
        gameObject.UnfuckLayoutHack();
    }

    private void UpdateKeyDisplay(Keybind _) => UpdateKeyDisplay();

    private void OnDestroy() {
        DeactivateKeyListen();
        TargetSetting.OnValueChanged -= UpdateKeyDisplay;
    }

    private void OnGUI() {
        if (!IsListening()) return;

        Event current = Event.current;
        switch (current.keyCode)
        {
            case KeyCode.None:
                return;
            case KeyCode.Escape:
                DeactivateKeyListen();
                return;
            default:
                if (current.type == EventType.KeyDown) {
                    if (current.keyCode.IsModifier()) {
                        _caughtModifier = current.keyCode;
                    } else {
                        _caughtKey = current.keyCode;
                    }
                    UpdateKeyDisplay();
                } else if (current.type == EventType.KeyUp) {
                    DeactivateKeyListen();
                }

                return;
        }

        // NotificationSystem.NotifySend("Keybind debug", $"Type {current.type.ToString()}, code {current.keyCode.ToString()}, modifiers {current.modifiers}");
    }
}
