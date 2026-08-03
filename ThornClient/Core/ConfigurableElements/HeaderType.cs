namespace ThornClient.Core.ConfigurableElements;

/// <summary>
/// The header type for ConfigHeader
/// </summary>
public enum HeaderType {
    /// <summary>
    /// First level header
    /// </summary>
    H1, 
    /// <summary>
    /// Second level header. In most cases, it's better to use a SettingGroup then H1 header, then H2 header.
    /// </summary>
    H2
}
