using System;
using System.Collections.Generic;
using System.Linq;

namespace ThornClient.Core.ConfigurableElements;

/// <summary>
/// A button row. Register this to your Configurable's Elements for a button array to show up in the config menu.
/// </summary>
public class ConfigButtonRow : IConfigurableElement {
    /// <summary>
    /// The unique identifier for this button row. It's there but not important, as button rows are not saved to config files.
    /// </summary>
    public string GUID { get; }
    /// <summary>
    /// The name of the button row. Not important as it's not shown anywhere and not saved to config.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The description of the button row. Shown in the tooltip when you hover over the button row.
    /// </summary>
    public string Description { get; }
    /// <summary>
    /// </summary>
    public InterfaceHints? Hints { get; set; }
    /// <summary>
    /// The action emitted when a button is clicked. The int parameter is the 0-base index of the button that was clicked.
    /// </summary>
    public Action<int> OnClick { get; set; }
    /// <summary>
    /// Texts on the buttons. The number of buttons is determined by the number of texts in this list.
    /// </summary>
    public List<string> Texts;

    /// <summary>
    /// Constructor for a button row.
    /// </summary>
    /// <param name="guid">The identifier, required but unimportant</param>
    /// <param name="name">The name of the button row, required but unimportant</param>
    /// <param name="description">The description of the button row, shown on the tooltip when you hover on the menu</param>
    /// <param name="texts">The texts for the buttons</param>
    public ConfigButtonRow(string guid, string name, string description, string[] texts) {
        GUID = guid;
        Name = name;
        Description = description;
        Texts = texts.ToList();
    }
}
