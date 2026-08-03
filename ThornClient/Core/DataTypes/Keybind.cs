using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// A pair of KeyCodes consisting of one main key and optionally a modifier
/// </summary>
[JsonConverter(typeof(KeybindJsonConverter))]
public class Keybind : IEquatable<Keybind> {
    /// <summary>
    /// The main key
    /// </summary>
    public KeyCode Key { get; set; }
    
    /// <summary>
    /// The modifier key
    /// </summary>
    public KeyCode Modifier { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key">The main key</param>
    /// <param name="modifier">The modifier key</param>
    public Keybind(
        KeyCode key,
        KeyCode modifier = KeyCode.None
    ) {
        Key = key;
        Modifier = modifier;
    }

    /// <summary>
    /// Checks if the keybind is equal to another keybind
    /// </summary>
    /// <param name="other">The other keybind to compare</param>
    /// <returns>True if the keybinds are equal, false otherwise</returns>
    public bool Equals(Keybind? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Key == other.Key && Modifier == other.Modifier;
    }
}
