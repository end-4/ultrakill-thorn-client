using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ThornClient.Core.DataTypes;

internal class WaypointJsonConverter : JsonConverter<Waypoint> {
    public override void WriteJson(JsonWriter writer, Waypoint? value, JsonSerializer serializer) {
        if (value == null) {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();

        writer.WritePropertyName("sceneName");
        writer.WriteValue(value.SceneName);

        writer.WritePropertyName("position");
        writer.WriteStartArray();
        writer.WriteValue(value.Position.x);
        writer.WriteValue(value.Position.y);
        writer.WriteValue(value.Position.z);
        writer.WriteEndArray();

        writer.WritePropertyName("name");
        writer.WriteValue(value.Name);

        writer.WriteEndObject();
    }

    public override Waypoint? ReadJson(
        JsonReader reader,
        Type objectType,
        Waypoint? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    ) {
        if (reader.TokenType == JsonToken.Null) {
            return null;
        }

        try {
            JObject obj = JObject.Load(reader);

            string sceneName = obj.Value<string>("sceneName") ?? string.Empty;
            string name = obj.Value<string>("name") ?? string.Empty;

            Vector3 position = Vector3.zero;

            if (obj["position"] is JArray { Count: >= 3 } posArray) {
                float x = posArray[0].Value<float>();
                float y = posArray[1].Value<float>();
                float z = posArray[2].Value<float>();
                position = new Vector3(x, y, z);
            }

            return new Waypoint(sceneName, position, name);
        } catch (Exception e) {
            Plugin.Log.LogError($"[WaypointJsonConverter] Failed to parse Waypoint JSON object: {e}");
        }

        return null;
    }
}