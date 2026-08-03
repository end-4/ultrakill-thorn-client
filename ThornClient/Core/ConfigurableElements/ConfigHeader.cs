using System;
using System.Collections.Generic;
using System.Linq;

namespace ThornClient.Core.ConfigurableElements;

/// <summary>
/// A header, for organization in the config menu. Register this to your Configurable's Elements for a header to show up in the config menu.
/// See also SettingGroup, which puts elements in a submenu.
/// </summary>
public class ConfigHeader : IConfigurableElement {
    /// <summary>
    /// The unique identifier for this header. It's there but not important, as headers are not saved to config files.
    /// </summary>
    public string GUID { get; }
    /// <summary>
    /// The name of the header. Shown as the header name.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The description of the header. Shown in small text below the header name.
    /// </summary>
    public string Description { get; }
    /// <summary>
    /// Irrelevant for headers.
    /// </summary>
    public InterfaceHints? Hints { get; }
    /// <summary>
    /// Font size of the header name. 
    /// For reference, the size of the description text is always 10. 
    /// Recommended values are 16 for H1 and 13 for H2. 
    /// If you need further categorization, consider using a SettingGroup.
    /// </summary>
    public int FontSize { get; set; } = 10;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="guid">Identifier for the element. Unimportant for headers.</param>
    /// <param name="name">The name of the header, shows up as a big text.</param>
    /// <param name="description">The description of the header, shows up in small text below the header name.</param>
    public ConfigHeader(string guid, string name, string description = "") {
        GUID = guid;
        Name = name;
        Description = description;
    }
}
