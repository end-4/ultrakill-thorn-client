namespace ThornClient.Core;

/// <summary>
/// The surface to draw the HUD on
/// </summary>
public enum HudSurface {
    /// <summary>
    /// Draw on the weapon panel surface
    /// </summary>
    Left, 
    /// <summary>
    /// Draw on the style panel surface
    /// </summary>
    Right, 
    /// <summary>
    /// Draw the item normally untransformed
    /// </summary>
    Overlay
}
