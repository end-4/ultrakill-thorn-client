using System;
using GameConsole.pcon;
using Newtonsoft.Json;
using UnityEngine;

namespace ThornClient.Core;

[JsonConverter(typeof(KeybindJsonConverter))]
public class Keybind : IEquatable<Keybind> {
    public KeyCode Key { get; set; }
    public KeyCode Modifier { get; set; }

    public Keybind(
        KeyCode key,
        KeyCode modifier = KeyCode.None
    ) {
        Key = key;
        Modifier = modifier;
    }

    public bool Equals(Keybind? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Key == other.Key && Modifier == other.Modifier;
    }
}
