using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ThornClient.Core;

public abstract class Setting {
    public string Name { get; }
    public string Description { get; }
    [JsonIgnore] public SettingType Type { get; protected set; }

    // Internal event used exclusively by Configurable for auto-saving
    [JsonIgnore] internal Action InternalOnValueChanged { get; set; }

    protected Setting(string name, string description) {
        Name = name;
        Description = description;
    }

    public abstract object GetValue();
    public abstract void SetValue(object value);
}

public class Setting<T> : Setting {
    public T Value {
        get;
        set {
            if (Equals(field, value)) return;
            field = value;

            OnValueChanged?.Invoke(field);
            InternalOnValueChanged?.Invoke();
        }
    }

    [JsonIgnore] public T DefaultValue { get; }
    [JsonIgnore] public Action<T> OnValueChanged { get; set; }

    public Setting(string name, string description, T defaultValue) : base(name, description) {
        Value = defaultValue;
        DefaultValue = defaultValue;

        Type = typeof(T) switch {
            Type t when t == typeof(bool) => SettingType.Boolean,
            Type t when t == typeof(float) => SettingType.Float,
            Type t when t == typeof(int) => SettingType.Int,
            Type t when t == typeof(string) => SettingType.Text,
            Type t when t == typeof(Color) => SettingType.Color,
            Type t when t == typeof(KeyCode) => SettingType.Bind,
            Type t when t == typeof(Keybind) => SettingType.Bind,
            _ => SettingType.Text
        };
    }

    public override object GetValue() => Value;

    public override void SetValue(object value) {
        try {
            if (typeof(T).IsEnum) {
                Value = (T)Enum.ToObject(typeof(T), Convert.ToInt32(value));
            } else {
                Value = (T)Convert.ChangeType(value, typeof(T));
            }
        } catch (Exception e) {
            Plugin.Log.LogInfo(
                $"[Setting] Failed to set {Name}'s value {value} to {typeof(T).Name}: {e}");
        }
    }
}

