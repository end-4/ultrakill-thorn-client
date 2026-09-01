namespace ThornClient.Core.ConfigurableElements;

/// <summary>
/// Interface for a configurable element, which can include settings and other non-saved elements for the config menu UI.
/// </summary>
public interface IConfigurableElement {
    /// <summary>
    /// The unique identifier for this element, used as a key in the config file. If this is a saved setting, it must be unique within the configurable (module).
    /// </summary>
    string GUID { get; }

    /// <summary>
    /// The name of the element that shows in the config menu.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The description of the element. Shown in the tooltip when you hover over the element.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Interface hints for the UI system, can be null
    /// </summary>
    public InterfaceHints? Hints { get; }
}
