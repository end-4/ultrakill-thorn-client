using System.Collections.Generic;
using Newtonsoft.Json;
using ThornClient.Managers;
using ThornClient.Settings;
using UnityEngine;

namespace ThornClient.Core;

public abstract class Configurable {
    [JsonIgnore] public string Name { get; }
    [JsonIgnore] public string Description { get; }
    [JsonIgnore] public List<Setting> Settings { get; } = [];

    public bool IsEnabled {
        get;
        set {
            if (field == value) return;
            field = value;

            if (field) OnEnable();
            else OnDisable();

            // Auto-save on change
            ConfigManager.SaveConfig(this);
        }
    }

    [JsonIgnore] public Setting<KeyCode> KeybindModifier { get; }
    [JsonIgnore] public Setting<KeyCode> Keybind { get; }
    [JsonIgnore] public Setting<bool> ToggleOnRelease { get; }

    protected Configurable(string name, string description, KeyCode defaultKey = KeyCode.None, KeyCode defaultModifier = KeyCode.None, bool defaultToggleOnRelease = false) {
        Name = name;
        Description = description;

        KeybindModifier = RegisterSetting("Modifier", "Key that must be held with the keybind to toggle", defaultModifier);
        Keybind = RegisterSetting("Keybind", "The key to toggle this feature", defaultKey);
        ToggleOnRelease = RegisterSetting("Toggle On Release", "Acts as a temporary hold-to-activate when enabled", defaultToggleOnRelease);
    }

    protected Setting<T> RegisterSetting<T>(string name, string description, T defaultValue) {
        var setting = new Setting<T>(name, description, defaultValue);

        setting.InternalOnValueChanged += () => {
            ConfigManager.SaveConfig(this);
        };

        Settings.Add(setting);
        return setting;
    }

    public void Toggle() {
        IsEnabled = !IsEnabled;
        if (IsEnabled) OnEnable();
        else OnDisable();
    }

    /// <summary>
    /// Stuff to run when the module is enabled
    /// </summary>
    protected virtual void OnEnable() { }

    /// <summary>
    /// Stuff to run when the module is disabled
    /// </summary>
    protected virtual void OnDisable() { }
}
