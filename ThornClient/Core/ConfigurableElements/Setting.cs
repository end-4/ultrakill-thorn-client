using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using NukeLib.Utils;
using ThornClient.Core.DataTypes;

namespace ThornClient.Core.ConfigurableElements;

/// <summary>
/// A setting item that is saved to a Configurable's config file.
/// You cannot instantiate this directly; use the generic class instead.
/// </summary>
public abstract class Setting : IConfigurableElement {
    /// <summary>
    /// The unique identifier. This MUST be unique within the Configurable (Module) and is used as a key in the config file.
    /// </summary>
    public string GUID { get; }

    /// <summary>
    /// The friendly name of the setting
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description of the setting. Shown in the tooltip when you hover over the setting.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The interface hints for the setting.
    /// Used for tweaking the appearance of the setting in the config menu where applicable.
    /// Can be null.
    /// </summary>
    public InterfaceHints? Hints { get; set; }

    /// <summary>
    /// The type of the setting. Used for determining how to display the setting in the config menu.
    /// </summary>
    [JsonIgnore] public SettingType Type { get; protected set; }

    /// <summary>
    /// Whether the setting is currently at its default value.
    /// </summary>
    [JsonIgnore] public abstract bool IsDefault { get; }

    [JsonIgnore] internal Action? InternalOnValueChanged { get; set; }

    /// <summary>
    /// Emitted when the setting's value changes.
    /// This is a general event that does not provide the new value. OnValueChanged does.
    /// </summary>
    public event Action? OnChanged;

    /// <summary>
    /// Constructor for a setting. This is protected, as you should not instantiate a Setting directly without a type.
    /// </summary>
    /// <param name="guid">The unique identifier for the setting.</param>
    /// <param name="name">The friendly name of the setting.</param>
    /// <param name="description">The description of the setting.</param>
    protected Setting(string guid, string name, string description) {
        GUID = guid;
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Raises the OnChanged event. This is called internally when the value changes.
    /// </summary>
    protected void RaiseOnChanged() => OnChanged?.Invoke();

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <returns></returns>
    public abstract object GetValue();

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="value"></param>
    public abstract void SetValue(object value);

    /// <summary>
    /// Resets the setting back to its original default value.
    /// </summary>
    public abstract void Reset();
}

/// <summary>
/// A typed setting item that is saved to a Configurable's config file.
/// Register/Instantiate through Configurable.RegisterSetting().
/// </summary>
/// <typeparam name="T">The type of the setting. Supported types:
/// <see cref="bool"/>, <see cref="Color"/>, <see cref="EnemyList"/>, <see cref="Keybind"/>, <see cref="float"/>, <see cref="int"/>, <see cref="string"/>
/// If your type is not supported, you must register a JsonConverter using ConfigManager.RegisterJsonConverter(). Unsupported types as of currently will not be shown in the config menu.
/// </typeparam>
public class Setting<T> : Setting {
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public T Value {
        get;
        set {
            if (Equals(field, value)) return;
            field = value;

            OnValueChanged?.Invoke(field);
            InternalOnValueChanged?.Invoke();
            RaiseOnChanged();
        }
    }

    /// <summary>
    /// The default value of the setting.
    /// </summary>
    [JsonIgnore] public T DefaultValue { get; }
    /// <summary>
    /// Emitted when the setting's value changes. Provides the new value as a parameter.
    /// </summary>
    [JsonIgnore] public Action<T>? OnValueChanged { get; set; }

    /// <summary>
    /// For Keybinds, this is emitted when the key combination is pressed down.
    /// </summary>
    public event Action? OnPress;
    /// <summary>
    /// For Keybinds, this is emitted when the key combination is released.
    /// </summary>
    public event Action? OnRelease;

    /// <summary>
    /// Whether the setting's value is at its default
    /// </summary>
    [JsonIgnore]
    public override bool IsDefault {
        get {
            if (Value == null && DefaultValue == null) return true;
            if (Value == null || DefaultValue == null) return false;

            if (Value is Color valColor && DefaultValue is Color defaultColor) {
                return valColor.Approximately(defaultColor);
            }
            if (Value is float valFloat && DefaultValue is float defaultFloat) {
                return Mathf.Approximately(valFloat, defaultFloat);
            }
            if (typeof(T).IsEnum) {
                return Convert.ToInt64(Value) == Convert.ToInt64(DefaultValue);
            }

            return EqualityComparer<T>.Default.Equals(Value, DefaultValue);
        }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="guid">The unique identifier. This MUST be unique within the Configurable (Module) and is used as a key in the config file.</param>
    /// <param name="name">The name of the setting</param>
    /// <param name="description">The description of the setting</param>
    /// <param name="defaultValue">The default value</param>
    public Setting(string guid, string name, string description, T defaultValue) : base(guid, name, description) {
        Value = defaultValue;
        DefaultValue = defaultValue;

        Type = typeof(T) switch {
            Type t when t == typeof(KeyCode) => SettingType.Bind,
            Type t when t == typeof(Keybind) => SettingType.Bind,
            Type t when t == typeof(bool) => SettingType.Bool,
            Type t when t == typeof(Color) => SettingType.Color,
            Type t when t == typeof(EnemyList) => SettingType.EnemyList,
            Type t when t.IsEnum => SettingType.Enum,
            Type t when t == typeof(float) => SettingType.Float,
            Type t when t == typeof(int) => SettingType.Int,
            Type t when t == typeof(string) => SettingType.Text,
            _ => SettingType.Unsupported
        };
    }

    internal void RaiseOnPress() => OnPress?.Invoke();
    internal void RaiseOnRelease() => OnRelease?.Invoke();


    /// <summary>
    /// Gets the current value
    /// </summary>
    /// <returns></returns>
    public override object GetValue() => Value!;

    /// <summary>
    /// Resets the value
    /// </summary>
    public override void Reset() {
        Value = DefaultValue;
    }

    /// <summary>
    /// Sets the value
    /// </summary>
    /// <param name="value">New value</param>
    public override void SetValue(object value) {
        try {
            // Normal matching type
            if (value is T directValue) {
                Value = directValue;
                return;
            }

            // JToken handling
            if (value is JToken token) {
                Value = token.ToObject<T>()!;
                return;
            }

            // Handle ThornClient.Core.Keybind
            if (value is string s && typeof(T) == typeof(Keybind)) {
                Value = (T)(object)JsonConvert.DeserializeObject<Keybind>($"\"{s}\"")!;
                return;
            }

            // Enums...
            if (typeof(T).IsEnum) {
                if (value is string str) {
                    Value = (T)Enum.Parse(typeof(T), str, ignoreCase: true);
                } else {
                    Value = (T)Enum.ToObject(typeof(T), Convert.ToInt64(value));
                }
                return;
            }

            // Fallback
            Value = (T)Convert.ChangeType(value, typeof(T));
        } catch (Exception e) {
            Plugin.Log.LogError($"[Setting] Failed to set {Name}'s value {value} to {typeof(T).Name}: {e}");
        }
    }
}
