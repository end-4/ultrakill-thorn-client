using System;
using Newtonsoft.Json;
using UnityEngine;
using ThornClient.Managers;

namespace ThornClient.Core;

public abstract class Module : Configurable {
    [JsonIgnore] public ModuleCategory Category { get; }
    public virtual string? CheatReason => null;
    public virtual Sprite Icon => AssetManager.Get<Sprite>(ThornClient.System.ClickGUI.BundleKey, "cube");
    public virtual string[]? Tags { get; }

    /// <summary>
    /// Constructor for a module
    /// </summary>
    /// <param name="guid">The globally unique identifier. It's recommended to follow a PROVIDER.NAME format, such as thorn.fullbright or myQolMod.betterCrosshairs</param>
    /// <param name="name">The display name</param>
    /// <param name="description">The description, shown when hovered in the config menu</param>
    /// <param name="moduleCategory">The category of the module</param>
    /// <param name="defaultKey">The default key to toggle the module, recommended to leave out to avoid conflicts. You can nudge the user to set one via Notiffy.</param>
    /// <param name="defaultModifier">The default modifier key to toggle the module</param>
    /// <param name="defaultToggleOnRelease">Whether the module should toggle on key release</param>
    /// <param name="hasToggling">Whether the module can be toggled</param>
    protected Module(
        string guid,
        string name,
        string description,
        ModuleCategory moduleCategory,
        KeyCode defaultKey = KeyCode.None,
        KeyCode defaultModifier = KeyCode.None,
        bool defaultToggleOnRelease = false,
        bool hasToggling = true)
        : base(guid, name, description, defaultKey, defaultModifier, defaultToggleOnRelease, hasToggling) {
        Category = moduleCategory;
        OnToggleStateChanged += (enabled) => ModuleManager.HeyIToggled(this, enabled);
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
    public virtual void OnRender() {
    }
}
