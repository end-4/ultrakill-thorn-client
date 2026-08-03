using System.Collections.Generic;

namespace ThornClient.Core.ConfigurableElements;

/// <summary>
/// A setting group. In the config menu, items in a setting group are put on a separate panel, much like PluginConfigurator's ConfigPanels
/// </summary>
public class SettingGroup : IConfigurableElement {
    /// <summary>
    /// The identifier, unimportant as it's not saved
    /// </summary>
    public string GUID { get; }

    /// <summary>
    /// The name of the setting group
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The description, shown when you hover over the button that opens the group's menu
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Hints are irrelevant for setting groups
    /// </summary>
    public InterfaceHints? Hints { get; set; }

    /// <summary>
    /// Configurable elements in the group
    /// </summary>
    public List<IConfigurableElement> Elements { get; } = [];

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guid">The identifier, unimportant as it's not saved</param>
    /// <param name="name">The name</param>
    /// <param name="description">The description</param>
    public SettingGroup(string guid, string name, string description) {
        GUID = guid;
        Name = name;
        Description = description;
    }
}
