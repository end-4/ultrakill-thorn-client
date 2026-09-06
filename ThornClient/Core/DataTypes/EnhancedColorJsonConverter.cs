using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ThornClient.Core.DataTypes;

public class EnhancedColorJsonConverter : JsonConverter<EnhancedColor> {
    public override void WriteJson(JsonWriter writer, EnhancedColor? value, JsonSerializer serializer) {
        if (value == null) {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("baseColor");
        writer.WriteValue("#" + ColorUtility.ToHtmlStringRGBA(value.BaseColor));
        writer.WritePropertyName("mode");
        writer.WriteValue((int)value.Mode);
        writer.WriteEndObject();
    }

    public override EnhancedColor ReadJson(JsonReader reader, Type objectType, EnhancedColor? existingValue, bool hasExistingValue,
        JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) {
            return new EnhancedColor(Color.white, EnhancedColorMode.Static);
        }

        if (reader.TokenType == JsonToken.String) {
            string raw = reader.Value?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(raw)) {
                var parsedColor = Color.white;
                if (raw.StartsWith("#")) {
                    ColorUtility.TryParseHtmlString(raw, out parsedColor);
                } else if (ColorUtility.TryParseHtmlString("#" + raw, out var parsed)) {
                    parsedColor = parsed;
                }

                return new EnhancedColor(parsedColor, EnhancedColorMode.Static);
            }

            return new EnhancedColor(Color.white, EnhancedColorMode.Static);
        }

        var obj = JObject.Load(reader);
        var baseColorToken = obj["baseColor"] ?? obj["BaseColor"];
        var modeToken = obj["mode"] ?? obj["Mode"];

        var baseColor = Color.white;
        if (baseColorToken != null && !string.IsNullOrWhiteSpace(baseColorToken.ToString())) {
            string raw = baseColorToken.ToString();
            if (raw.StartsWith("#")) {
                ColorUtility.TryParseHtmlString(raw, out baseColor);
            } else if (ColorUtility.TryParseHtmlString("#" + raw, out var parsed)) {
                baseColor = parsed;
            }
        }

        var mode = EnhancedColorMode.Static;
        if (modeToken != null && modeToken.Type != JTokenType.Null) {
            if (modeToken.Type == JTokenType.Integer || modeToken.Type == JTokenType.Float) {
                mode = (EnhancedColorMode)Mathf.Clamp((int)modeToken, 0, Enum.GetValues(typeof(EnhancedColorMode)).Length - 1);
            } else if (Enum.TryParse<EnhancedColorMode>(modeToken.ToString(), true, out var parsedMode)) {
                mode = parsedMode;
            }
        }

        return new EnhancedColor(baseColor, mode);
    }
}
