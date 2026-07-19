using Newtonsoft.Json;
using UnityEngine;

namespace ThornClient.Core;

public abstract class Module : Configurable {
    [JsonIgnore] public ModuleCategory Category { get; }
    public virtual string? CheatReason => null;
    public virtual string IconName { get; protected set; } = "cube";

    protected Module(
        string guid,
        string name,
        string description,
        ModuleCategory moduleCategory,
        KeyCode defaultKey = KeyCode.None,
        KeyCode defaultModifier = KeyCode.None,
        bool defaultToggleOnRelease = false,
        bool hasToggling = true)
        : base(guid, name, description, defaultKey, defaultModifier, defaultToggleOnRelease, hasToggling)
    {
        Category = moduleCategory;
    }

    /// <summary>
    /// Invoked every frame, similar to MonoBehavior.Update()
    /// </summary>
    public virtual void OnUpdate() {
    }

    /// <summary>
    /// Invoked every frame during the late screen rendering pass.
    /// Place raw low-level vector drawings (GL code) inside this method block.
    /// </summary>
    public virtual void OnRender() { }
}

