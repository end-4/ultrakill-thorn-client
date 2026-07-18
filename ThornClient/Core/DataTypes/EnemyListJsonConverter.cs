using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// The Json Converter for EnemyList
/// </summary>
public class EnemyListJsonConverter : JsonConverter<EnemyList> {
    public override void WriteJson(JsonWriter writer, EnemyList? value, JsonSerializer serializer) {
        if (value == null || value.enemies == null) {
            writer.WriteNull();
            return;
        }

        writer.WriteStartArray();
        foreach (var enemy in value.enemies) {
            writer.WriteValue(enemy.ToString());
        }

        writer.WriteEndArray();
    }

    public override EnemyList? ReadJson(JsonReader reader, Type objectType, EnemyList? existingValue,
        bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) return null;

        var enemyList = existingValue ?? new EnemyList();
        enemyList.enemies ??= new HashSet<EnemyType>();

        if (reader.TokenType == JsonToken.StartArray) {
            while (reader.Read() && reader.TokenType != JsonToken.EndArray) {
                if (reader.TokenType == JsonToken.String) {
                    string value = reader.Value?.ToString() ?? "";
                    if (Enum.TryParse(value, out EnemyType enemyType)) {
                        enemyList.enemies.Add(enemyType);
                    }
                } else if (reader.TokenType == JsonToken.Integer) {
                    int value = Convert.ToInt32(reader.Value);
                    if (Enum.IsDefined(typeof(EnemyType), value)) {
                        enemyList.enemies.Add((EnemyType)value);
                    }
                }
            }

            return enemyList;
        }

        throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing EnemyList");
    }
}
