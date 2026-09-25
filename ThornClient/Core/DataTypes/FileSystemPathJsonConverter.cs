using System;
using Newtonsoft.Json;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// JSON converter for FileSystemPath
/// </summary>
/// <typeparam name="T">The type that inherits from FileSystemPath</typeparam>
public abstract class BasePathJsonConverter<T> : JsonConverter<T> where T : FileSystemPath {
    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer) {
        if (value is null) {
            writer.WriteNull();
            return;
        }

        writer.WriteValue(value.Path);
    }

    /// <inheritdoc />
    public override T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue,
        JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) return null;
        var pathStr = reader.Value?.ToString() ?? string.Empty;
        return (T?)Activator.CreateInstance(typeof(T), pathStr);
    }
}

/// <summary>
/// JSON converter for FilePath
/// </summary>
public class FilePathConverter : BasePathJsonConverter<FilePath> {
}

/// <summary>
/// JSON converter for FolderPath
/// </summary>
public class FolderPathConverter : BasePathJsonConverter<FolderPath> {
}
