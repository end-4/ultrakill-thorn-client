using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ThornClient.Core.DataTypes;

internal class KeybindJsonConverter : JsonConverter<Keybind> {
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
            return new Keybind(KeyCode.None);
        }

        try {
            string[] tokens = rawValue.Split('+');

            if (tokens.Length == 2) { // Has modifier
                var mod = (KeyCode)Enum.Parse(typeof(KeyCode), tokens[0], true);
                var key = (KeyCode)Enum.Parse(typeof(KeyCode), tokens[1], true);
                return new Keybind(key, mod);
            } else if (tokens.Length == 1) { // Key only
                var key = (KeyCode)Enum.Parse(typeof(KeyCode), tokens[0], true);
                return new Keybind(key);
            }
        } catch (Exception e) {
            Plugin.Log.LogError($"[KeybindJsonConverter] Failed to parse Keybind config string \"{rawValue}\": {e}");
        }

        return new Keybind(KeyCode.None);
    }
}
