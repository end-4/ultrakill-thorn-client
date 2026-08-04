using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ThornClient.Core;

/// <summary>
/// Json converter for Unity Color type
/// </summary>
internal class ColorJsonConverter : JsonConverter<Color> {
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer) {
        // #RRGGBBAA
        string hex = "#" + ColorUtility.ToHtmlStringRGBA(value);
        writer.WriteValue(hex);
    }

    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer) {
        string? hex = reader.Value?.ToString();
        if (string.IsNullOrEmpty(hex)) return Color.white;

        if (ColorUtility.TryParseHtmlString(hex, out Color color)) {
            return color;
        }

        return Color.white;
    }
}
