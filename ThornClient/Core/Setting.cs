using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using NukeLib.Utils;
using ThornClient.Core.DataTypes;

namespace ThornClient.Core;

public abstract class Setting : IConfigurableElement {
    public string GUID { get; }
    public string Name { get; }
    public string Description { get; }
    public InterfaceHints? Hints { get; set; }
    [JsonIgnore] public SettingType Type { get; protected set; }
    [JsonIgnore] public abstract bool IsDefault { get; }

    [JsonIgnore] internal Action? InternalOnValueChanged { get; set; }
    public event Action? OnChanged;

    protected Setting(string guid, string name, string description) {
        GUID = guid;
        Name = name;
        Description = description;
    }

    protected void RaiseOnChanged() => OnChanged?.Invoke();

    public abstract object GetValue();
    public abstract void SetValue(object value);

    /// <summary>
    /// Resets the setting back to its original default value.
    /// </summary>
    public abstract void Reset();
}

public class Setting<T> : Setting {
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

    [JsonIgnore] public T DefaultValue { get; }
    [JsonIgnore] public Action<T>? OnValueChanged { get; set; }

    // Specific events for Keybind settings
    public event Action? OnPress;
    public event Action? OnRelease;

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


    public override object GetValue() => Value!;
    public override void Reset() {
        Value = DefaultValue;
    }

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
