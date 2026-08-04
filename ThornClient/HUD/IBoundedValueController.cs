namespace ThornClient.HUD;

/// <summary>
/// Interface for a controller that manages a bounded value HUD module.
/// </summary>
public interface IBoundedValueController {
    /// <summary>
    /// The target bounded value HUD module that this controller handles.
    /// </summary>
    public BoundedValueHudModule? TargetModule { get; set; }
}
