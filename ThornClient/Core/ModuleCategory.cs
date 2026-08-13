namespace ThornClient.Core;

/// <summary>
/// The category of a module
/// </summary>
public enum ModuleCategory {
    /// <summary>
    /// Utility modules
    /// </summary>
    Utility, // minecraft fly, size

    /// <summary>
    /// Modules that affect the rendering
    /// </summary>
    Render, // hide gui, visible portals

    /// <summary>
    /// Modules that affect gameplay, such as turning the floor into lava
    /// </summary>
    Gameplay, // waypoints, ice floor

    /// <summary>
    /// Modules that don't fit into other categories
    /// </summary>
    Misc,

    /// <summary>
    /// Modules that affect the HUD. Use this ONLY on HudModules, or your module will not show up!
    /// </summary>
    Hud
}
