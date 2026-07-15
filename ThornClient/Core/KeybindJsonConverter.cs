using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ThornClient.Core;

public class KeybindJsonConverter : JsonConverter<Keybind> {
    public override void WriteJson(JsonWriter writer, Keybind? value, JsonSerializer serializer) {
        if (value == null || value.Key == KeyCode.None) {
            writer.WriteValue("None");
            return;
        }

        // LeftAlt+K, K, etc.
        string serializedString = value.Modifier != KeyCode.None
            ? $"{value.Modifier}+{value.Key}"
            : $"{value.Key}";

        writer.WriteValue(serializedString);
    }

    public override Keybind? ReadJson(JsonReader reader, Type objectType, Keybind? existingValue, bool hasExistingValue,
        JsonSerializer serializer) {
        string? rawValue = reader.Value?.ToString();
        if (string.IsNullOrEmpty(rawValue) || rawValue == "None") {
            return new Keybind(KeyCode.None, existingValue?.OnPress, existingValue?.OnRelease);
        }

        try {
            string[] tokens = rawValue.Split('+');

            if (tokens.Length == 2) { // Has modifier
                KeyCode mod = (KeyCode)Enum.Parse(typeof(KeyCode), tokens[0]);
                KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), tokens[1]);
                return new Keybind(key, existingValue?.OnPress, existingValue?.OnRelease, mod);
            } else if (tokens.Length == 1) { // Key only
                KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), tokens[0]);
                return new Keybind(key, existingValue?.OnPress, existingValue?.OnRelease);
            }
        } catch (Exception e) {
            Plugin.Log.LogError($"[KeybindJsonConverter] Failed to parse keybind config string \"{rawValue}\": {e.Message}");
        }

        return new Keybind(KeyCode.None, existingValue?.OnPress, existingValue?.OnRelease);
    }
}
