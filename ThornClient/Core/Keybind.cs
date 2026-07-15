using System;
using GameConsole.pcon;
using Newtonsoft.Json;
using UnityEngine;

namespace ThornClient.Core;

[JsonConverter(typeof(KeybindJsonConverter))]
public class Keybind {
    public KeyCode Key { get; set; }
    public KeyCode Modifier { get; set; }

    [JsonIgnore] public Action? OnPress { get; set; }
    [JsonIgnore] public Action? OnRelease { get; set; }

    public Keybind(
        KeyCode key,
        Action? onPress = null,
        Action? onRelease = null,
        KeyCode modifier = KeyCode.None
    ) {
        Key = key;
        Modifier = modifier;
        OnPress = onPress;
        OnRelease = onRelease;
    }

    /// <summary>
    /// Checks for key events and invokes actions accordingly. To be called by the Input Manager.
    /// </summary>
    internal void CheckInput() {
        if (Key == KeyCode.None) return;

        bool modifierSatisfied = (Modifier == KeyCode.None) || Input.GetKey(Modifier);
        if (!modifierSatisfied) return;

        if (Input.GetKeyDown(Key)) {
            Plugin.Log.LogInfo($"Pressed {Key}");
            OnPress?.Invoke();
        }

        if (Input.GetKeyUp(Key)) {
            OnRelease?.Invoke();
        }
    }
}
