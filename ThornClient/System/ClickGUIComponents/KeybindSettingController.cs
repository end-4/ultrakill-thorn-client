using System.Collections.Generic;
using Notiffy.API;
using NukeLib.Game;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class KeybindSettingController : MonoBehaviour {
    private const string PauseKey = "Thorn_ClickGUI_KeybindListener";
    private static readonly Color NormalColor = new Color(1, 1, 1);
    private static Color ListeningColor => ThornModule.Instance?.Accent.Value ?? new Color(1f, 0.4048f, 0f);

    private static KeybindSettingController? _listeningInstance = null;
    private Button? _activateButton;
    private Colorizer? _keyBorder;
    private TextMeshProUGUI _displayText;
    public Setting<Keybind> TargetSetting;

    private KeyCode _caughtModifier = KeyCode.None;
    private KeyCode _caughtKey = KeyCode.None;

    private static OptionsManager? opts => OptionsManager.Instance;

    private void Start() {
        _activateButton = gameObject.GetComponent<Button>();
        _keyBorder = gameObject.FindRecursive("ValueDisplay").GetOrAddComponent<Colorizer>();
        _displayText = gameObject.FindRecursive("ValueDisplay/Text").GetComponent<TextMeshProUGUI>();
        _activateButton.onClick.AddListener(ActivateKeyListen);
        UpdateKeyDisplay();
        TargetSetting.OnValueChanged += UpdateKeyDisplay;
    }

    private void SetBorderColor(bool highlighted) {
        if (_keyBorder == null) return;
        _keyBorder.UpdateHighlight(highlighted);
    }

    private void ActivateKeyListen() {
        ThornClient.Managers.InputManager.BlockInput = true;
        Pauser.Pause(true, PauseKey);
        if (_listeningInstance != null) {
            _listeningInstance.SetBorderColor(false);
        }

        _listeningInstance = this;
        SetBorderColor(true);
        _caughtModifier = KeyCode.None;
        _caughtKey = KeyCode.None;
    }

    private void DeactivateKeyListen() {
        ThornClient.Managers.InputManager.BlockInput = false;
        GameStateManager.Instance.PopState(PauseKey);
        if (!IsListening()) return;
        _listeningInstance = null;

        SetBorderColor(false);

        // Try to bind
        if (_caughtModifier != KeyCode.None && _caughtKey == KeyCode.None) {
            _caughtKey = _caughtModifier;
            _caughtModifier = KeyCode.None;
        }

        TargetSetting.Value = new Keybind(key: _caughtKey, modifier: _caughtModifier);
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

        // Special/nothing
        if (current.isKey && current.keyCode == KeyCode.None) return;
        if (current.keyCode == KeyCode.Escape) {
            DeactivateKeyListen();
            return;
        }

        // Normal
        if (current.type is EventType.KeyDown or EventType.MouseDown or EventType.ScrollWheel) {
            KeyCode currKey = current.keyCode;
            if (current.isMouse) currKey = KeyCode.Mouse0 + current.button;
            if (currKey.IsModifier()) {
                _caughtModifier = currKey;
            } else {
                _caughtKey = currKey;
            }

            UpdateKeyDisplay();
        }

        if (current.type is EventType.KeyUp or EventType.MouseUp or EventType.ScrollWheel) {
            DeactivateKeyListen();
        }

        return;

        // NotificationSystem.NotifySend("Keybind debug", $"Type {current.type.ToString()}, code {current.keyCode.ToString()}, modifiers {current.modifiers}");
    }
}
