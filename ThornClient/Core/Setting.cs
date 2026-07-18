using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ThornClient.Core;

public abstract class Setting {
    public string GUID { get; }
    public string Name { get; }
    public string Description { get; }
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

    [JsonIgnore]
    public override bool IsDefault => EqualityComparer<T>.Default.Equals(Value, DefaultValue);

    public Setting(string guid, string name, string description, T defaultValue) : base(guid, name, description) {
        Value = defaultValue;
        DefaultValue = defaultValue;

        Type = typeof(T) switch {
            Type t when t == typeof(bool) => SettingType.Bool,
            Type t when t == typeof(float) => SettingType.Float,
            Type t when t == typeof(int) => SettingType.Int,
            Type t when t == typeof(string) => SettingType.Text,
            Type t when t == typeof(Color) => SettingType.Color,
            Type t when t == typeof(KeyCode) => SettingType.Bind,
            Type t when t == typeof(Keybind) => SettingType.Bind,
            _ => SettingType.Text
        };
    }

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
            if (typeof(T) == typeof(Keybind) && value is string keybindString) {
                // Deserialize using your existing custom converter logic by wrapping it as a JSON string
                var parsedKeybind = JsonConvert.DeserializeObject<Keybind>($"\"{keybindString}\"");

                // Preserve the callbacks from the current keybind instance so they don't break
                if (parsedKeybind != null && Value is Keybind currentKeybind) {
                    parsedKeybind.OnPress = currentKeybind.OnPress;
                    parsedKeybind.OnRelease = currentKeybind.OnRelease;
                }

                Value = (T)(object)parsedKeybind!;
                return;
            }

            // Fallback
            if (typeof(T).IsEnum) {
                Value = (T)Enum.ToObject(typeof(T), Convert.ToInt32(value));
            } else {
                Value = (T)Convert.ChangeType(value, typeof(T));
            }
        } catch (Exception e) {
            Plugin.Log.LogError(
                $"[Setting] Failed to set {Name}'s value {value} to {typeof(T).Name}: {e}");
        }
    }
}
