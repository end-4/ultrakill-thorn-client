using System;
using System.Collections.Generic;
using System.Linq;

namespace ThornClient.Core.ConfigurableElements;

public class ConfigHeader : IConfigurableElement {
    public string GUID { get; }
    public string Name { get; }
    public string Description { get; }
    public InterfaceHints? Hints { get; }

    public int FontSize { get; set; } = 10;

    public ConfigHeader(string guid, string name, string description = "") {
        GUID = guid;
        Name = name;
        Description = description;
    }
}
