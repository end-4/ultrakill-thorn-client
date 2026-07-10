using Newtonsoft.Json;
using UnityEngine;

namespace ThornClient.Core;

public enum ModuleCategory {
    Enemy,
    Player,
    Render,
    World,
    Misc,
}

public abstract class Module : Configurable {
    [JsonIgnore] public ModuleCategory Category { get; }

    /// <summary>
    /// Reason this module is cheaty. It's a getter thingy so you can apply conditions other than IsEnabled to it
    /// Null or empty string = normal, non-empty string = cheaty
    /// When a module might become cheaty, you should also call CheatManager.UpdateCheatiness()
    /// </summary>
    public virtual string? CheatReason => null;

    protected Module(string name, string description, ModuleCategory moduleCategory,
        KeyCode defaultKey = KeyCode.None, KeyCode defaultModifier = KeyCode.None,
        bool defaultToggleOnRelease = false)
        : base(name, description, defaultKey, defaultModifier, defaultToggleOnRelease) {
        Category = moduleCategory;
    }

    public virtual void OnUpdate() {
    }
}
