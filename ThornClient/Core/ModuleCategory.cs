namespace ThornClient.Core;

/// <summary>
/// The category of a module
/// </summary>
public enum ModuleCategory {
    /// <summary>
    /// Modules that affect enemies, such as buffs
    /// </summary>
    Enemy, // mitosis

    /// <summary>
    /// Modules that affect the player, such as movement tweaks and control enhancements
    /// </summary>
    Player, // minecraft fly, size

    /// <summary>
    /// Modules that affect the rendering, such as ESP
    /// </summary>
    Render, // viewmodel transform, hide gui, visible portals

    /// <summary>
    /// Modules that affect the world, such as turning the floor into lava
    /// </summary>
    World, // waypoints, ice floor, lava floor, atlantis

    /// <summary>
    /// Modules that don't fit into other categories
    /// </summary>
    Misc,
    
    /// <summary>
    /// Modules that affect the HUD. Use this ONLY on HudModules, or your module will not show up!
    /// </summary>
    Hud
}
