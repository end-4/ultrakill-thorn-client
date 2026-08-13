using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// A pair of KeyCodes consisting of one main key and optionally a modifier
/// </summary>
[JsonConverter(typeof(WaypointJsonConverter))]
public class Waypoint : IEquatable<Waypoint> {
    /// <summary>
    /// The name of the scene taken from SceneHelper.CurrentScene
    /// </summary>
    public string SceneName;

    /// <summary>
    /// The x, y, z coordinates in the world
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The name of the waypoint, such as "Fraud Supermarket"
    /// </summary>
    public string Name;

    public Waypoint(
        string sceneName,
        Vector3 position,
        string name
    ) {
        SceneName = sceneName;
        Position = position;
        Name = name;
    }

    /// <summary>
    /// Checks if the keybind is equal to another keybind
    /// </summary>
    /// <param name="other">The other keybind to compare</param>
    /// <returns>True if the keybinds are equal, false otherwise</returns>
    public bool Equals(Waypoint? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return (
            this.Position.Equals(other.Position)
            && this.Name == other.Name
            && this.SceneName == other.SceneName
        );
    }
}
