using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Core;

/// <summary>
/// A item that can be configured.
/// </summary>
public abstract class Configurable {
    /// <summary>
    /// The unique identifier. It is recommended to follow a PROVIDER.NAME format, such as thorn.fpsDisplay or ultravoice.generalConfig
    /// </summary>
    [JsonIgnore] public string GUID;

    /// <summary>
    /// The friendly name
    /// </summary>
    [JsonIgnore]
    public string Name { get; }

    /// <summary>
    /// The description
    /// </summary>
    [JsonIgnore]
    public string Description { get; }

    /// <summary>
    /// Elements, including settings and other UI elements
    /// </summary>
    [JsonIgnore]
    public List<IConfigurableElement> Elements { get; } = [];

    /// <summary>
    /// The keybind to toggle this configurable item
    /// </summary>
    [JsonIgnore]
    public Setting<Keybind> ToggleKeybind { get; }

    /// <summary>
    /// Whether the toggling keybind toggles on release
    /// </summary>
    [JsonIgnore]
    public Setting<bool> ToggleOnRelease { get; }

    /// <summary>
    /// Emitted when the toggle state changes
    /// </summary>
    public event Action<bool>? OnToggleStateChanged;

    /// <summary>
    /// Whether this configurable item is enabled
    /// </summary>
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

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guid">The unique identifier</param>
    /// <param name="name">The friendly name</param>
    /// <param name="description">The description</param>
    /// <param name="defaultKey">The default key for the toggle keybind. It is recommended to leave this out to avoid conflicts. You can nudge the user to set it via Notiffy.</param>
    /// <param name="defaultModifier">The default modifier for the toggle keybind</param>
    /// <param name="defaultToggleOnRelease">Whether the toggle keybind toggles on release</param>
    /// <param name="hasToggling">Whether this configurable item has toggling</param>
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

    /// <summary>
    /// Registers a configurable element to the Configurable.
    /// </summary>
    /// <param name="element">The element to register</param>
    /// <param name="parent">The parent group to register the element under. If null, the element will be registered at the root level.</param>
    protected void RegisterElement(IConfigurableElement element, SettingGroup? parent = null) {
        if (parent == null) {
            Elements.Add(element);
        } else {
            parent.Elements.Add(element);
        }
    }

    /// <summary>
    /// Creates a new setting group.
    /// </summary>
    /// <param name="guid">The identifier, unimportant</param>
    /// <param name="name">The friendly name</param>
    /// <param name="description">The description</param>
    /// <param name="parent">The parent group to register the group under. If null, the group will be registered at the root level.</param>
    /// <returns></returns>
    protected SettingGroup CreateGroup(string guid, string name, string description, SettingGroup? parent = null) {
        var group = new SettingGroup(guid, name, description);
        RegisterElement(group, parent);
        return group;
    }

    /// <summary>
    /// Registers a new setting.
    /// </summary>
    /// <typeparam name="T">The type of the setting</typeparam>
    /// <param name="guid">The unique identifier within the configurable</param>
    /// <param name="name">The friendly display name</param>
    /// <param name="description">The description that will show when hovered</param>
    /// <param name="defaultValue">The default value</param>
    /// <param name="parent">The parent group to register the setting under. If null, the setting will be registered at the root level.</param>
    /// <returns></returns>
    protected Setting<T> RegisterSetting<T>(string guid, string name, string description, T defaultValue,
        SettingGroup? parent = null) {
        var setting = new Setting<T>(guid, name, description, defaultValue);

        setting.InternalOnValueChanged += () => { ConfigManager.SaveConfig(this); };

        if (setting is Setting<Keybind> keybindSetting) {
            Managers.InputManager.RegisterKeybindSetting(keybindSetting);
        }

        RegisterElement(setting, parent);
        return setting;
    }


    /// <summary>
    /// Creates a button row.
    /// </summary>
    /// <param name="guid">The unique identifier within the configurable</param>
    /// <param name="name">The friendly display name</param>
    /// <param name="description">The description that will show when hovered</param>
    /// <param name="texts">The texts on the buttons</param>
    /// <param name="parent">The parent group to register the setting under. If null, the setting will be registered at the root level.</param>
    /// <returns></returns>
    protected ConfigButtonRow RegisterButtonRow(string guid, string name, string description, string[] texts, SettingGroup? parent = null) {
        var button = new ConfigButtonRow(guid, name, description, texts);
        RegisterElement(button, parent);
        return button;
    }

    /// <summary>
    /// Creates a header.
    /// </summary>
    /// <param name="guid">The identifier, unimportant for header</param>
    /// <param name="name">The friendly display name</param>
    /// <param name="description">The description that will show when hovered</param>
    /// <param name="headerType">The type, as in size, like H1 or H2</param>
    /// <param name="parent">The parent group to register the setting under. If null, the setting will be registered at the root level.</param>
    /// <returns></returns>
    protected ConfigHeader RegisterHeader(string guid, string name, string description = "", HeaderType headerType = HeaderType.H1, SettingGroup? parent = null) {
        var header = new ConfigHeader(guid, name, description) {
            FontSize = headerType switch {
                HeaderType.H1 => 16,
                HeaderType.H2 => 13,
                _ => 16
            }
        };
        RegisterElement(header, parent);
        return header;
    }

    /// <summary>
    /// Toggles the configurable
    /// </summary>
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
