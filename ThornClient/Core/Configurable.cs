using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ThornClient.Core.DataTypes;
using ThornClient.Core.UI;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Core;

public abstract class Configurable {
    [JsonIgnore] public string GUID;
    [JsonIgnore] public string Name { get; }
    [JsonIgnore] public string Description { get; }
    [JsonIgnore] public List<IConfigurableElement> Elements { get; } = [];

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

            // Plugin.Log.LogInfo($"[ConfigManager] Toggled {Name} to {value}, saving");
            ConfigManager.SaveConfig(this);
        }
    }

    protected Configurable(
        string guid,
        string name,
        string description,
        KeyCode defaultKey = KeyCode.None,
        KeyCode defaultModifier = KeyCode.None,
        bool defaultToggleOnRelease = false, bool hasToggling = true) {
        GUID = guid;
        Name = name;
        Description = description;

        if (hasToggling) {
            var defaultBind = new Keybind(defaultKey, modifier: defaultModifier);
            ToggleKeybind = RegisterSetting("toggleKeybind", "Toggle Keybind",
                "The key combo used to turn this feature on and off", defaultBind);

            ToggleOnRelease = RegisterSetting(
                "toggleOnRelease",
                "Toggle On Release",
                "Acts as a temporary hold-to-(de)activate when enabled",
                defaultToggleOnRelease
            );
            ToggleOnRelease.InternalOnValueChanged += UpdateToggleCallbacks;
            ToggleKeybind.InternalOnValueChanged += UpdateToggleCallbacks;
        }

        UpdateToggleCallbacks();
    }

    private void UpdateToggleCallbacks() {
        if (ToggleKeybind == null) return;

        // Clear existing to avoid dupes
        ToggleKeybind.OnPress -= HandlePress;
        ToggleKeybind.OnRelease -= HandleRelease;

        if (ToggleOnRelease.Value) {
            ToggleKeybind.OnPress += HandlePress;
            ToggleKeybind.OnRelease += HandleRelease;
        } else {
            ToggleKeybind.OnPress += HandlePress;
        }
    }

    private void HandlePress() => IsEnabled = !IsEnabled;
    private void HandleRelease() => IsEnabled = !IsEnabled;

    protected void RegisterElement(IConfigurableElement element, SettingGroup? parent = null) {
        if (parent == null) {
            Elements.Add(element);
        } else {
            parent.Elements.Add(element);
        }
    }

    protected SettingGroup CreateGroup(string guid, string name, string description, SettingGroup? parent = null) {
        var group = new SettingGroup(guid, name, description);
        RegisterElement(group, parent);
        return group;
    }

    protected Setting<T> RegisterSetting<T>(string guid, string name, string description, T defaultValue, SettingGroup? parent = null) {
        var setting = new Setting<T>(guid, name, description, defaultValue);

        setting.InternalOnValueChanged += () => { ConfigManager.SaveConfig(this); };

        if (setting is Setting<Keybind> keybindSetting) {
            Managers.InputManager.RegisterKeybindSetting(keybindSetting);
        }

        RegisterElement(setting, parent);
        return setting;
    }

    protected ConfigButtonRow RegisterButtonRow(string guid, string name, string description, string[] texts, SettingGroup? parent = null) {
        var button = new ConfigButtonRow(guid, name, description, texts);
        RegisterElement(button, parent);
        return button;
    }

    public void Toggle() {
        IsEnabled = !IsEnabled;
    }

    /// <summary>
    /// Stuff to run when the module is enabled
    /// </summary>
    protected virtual void OnEnable() {
    }

    /// <summary>
    /// Stuff to run when the module is disabled
    /// </summary>
    protected virtual void OnDisable() {
    }
}
