namespace ThornClient.Core.ConfigurableElements;

public interface IConfigurableElement {
    string GUID { get; }
    string Name { get; }
    string Description { get; }

    /// <summary>
    /// Interface hints for the UI system, can be null
    /// </summary>
    public InterfaceHints? Hints { get; }
}
