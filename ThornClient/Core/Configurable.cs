using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Core;

public abstract class Configurable {
    [JsonIgnore] public string GUID;
    [JsonIgnore] public string Name { get; }
    [JsonIgnore] public string Description { get; }
    [JsonIgnore] public List<Setting> Settings { get; } = [];

    [JsonIgnore] public Setting<Keybind> ToggleKeybind { get; }
    [JsonIgnore] public Setting<bool> ToggleOnRelease { get; }

    public event Action<bool>? OnToggleStateChanged;

    public virtual bool IsEnabled {
        get;
        set {
            if (field == value) return;
            field = value;

            OnToggleStateChanged?.Invoke(field);

            if (field) OnEnable();
            else OnDisable();

            Plugin.Log.LogInfo($"[ConfigManager] Toggled {Name} to {value}, saving");
            ConfigManager.SaveConfig(this);
        }
    }

    protected Configurable(
        string guid,
        string name,
        string description,
        KeyCode defaultKey = KeyCode.None,
        KeyCode defaultModifier = KeyCode.None,
        bool defaultToggleOnRelease = false)
    {
        GUID = guid;
        Name = name;
        Description = description;

        ToggleOnRelease = RegisterSetting(
            "toggleOnRelease",
            "Toggle On Release",
            "Acts as a temporary hold-to-(de)activate when enabled",
            defaultToggleOnRelease
        );

        var defaultBind = new Keybind(defaultKey, modifier: defaultModifier);
        ToggleKeybind = RegisterSetting("toggleKeybind", "Toggle Keybind", "The key combo used to turn this feature on and off", defaultBind);

        UpdateToggleCallbacks();
        ToggleOnRelease.InternalOnValueChanged += UpdateToggleCallbacks;
        ToggleKeybind.InternalOnValueChanged += UpdateToggleCallbacks;
    }

    /// <summary>
    /// Swaps the Keybind's behavior loops depending on ToggleOnRelease
    /// </summary>
    private void UpdateToggleCallbacks() {
        var keybind = ToggleKeybind?.Value;
        if (keybind == null) return;

        if (ToggleOnRelease.Value) {
            keybind.OnPress = () => IsEnabled = !IsEnabled;
            keybind.OnRelease = () => IsEnabled = !IsEnabled;
        } else {
            keybind.OnPress = Toggle;
            keybind.OnRelease = null;
        }
    }

    protected Setting<T> RegisterSetting<T>(string guid, string name, string description, T defaultValue) {
        var setting = new Setting<T>(guid, name, description, defaultValue);

        setting.InternalOnValueChanged += () => {
            ConfigManager.SaveConfig(this);
        };

        Settings.Add(setting);
        return setting;
    }

    public void Toggle() {
        IsEnabled = !IsEnabled;
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
