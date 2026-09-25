using System;
using Newtonsoft.Json;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// A file system path, either a file or folder
/// </summary>
public abstract class FileSystemPath : IEquatable<FileSystemPath> {
    public string Path { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path">The path</param>
    protected FileSystemPath(string path) {
        Path = path;
    }

    /// <inheritdoc />
    public override string ToString() => Path;

    /// <inheritdoc />
    public bool Equals(FileSystemPath? other) => other != null && Path == other.Path;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is FileSystemPath other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Path.GetHashCode();
}

/// <summary>
/// A folder path
/// </summary>
[JsonConverter(typeof(FolderPathConverter))]
[ConfigurableUICreator(typeof(FolderPathUICreator))]
public class FolderPath : FileSystemPath {
    /// <inheritdoc />
    public FolderPath(string path) : base(path) { }
}

/// <summary>
/// A file path
/// </summary>
[JsonConverter(typeof(FilePathConverter))]
[ConfigurableUICreator(typeof(FilePathUICreator))]
public class FilePath : FileSystemPath {
    /// <inheritdoc />
    public FilePath(string path) : base(path) { }
}

