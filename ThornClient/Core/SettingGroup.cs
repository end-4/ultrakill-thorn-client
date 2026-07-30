using System.Collections.Generic;

namespace ThornClient.Core;

public class SettingGroup : IConfigurableElement {
    public string GUID { get; }
    public string Name { get; }
    public string Description { get; }
    public InterfaceHints? Hints { get; set; }
    public List<IConfigurableElement> Elements { get; } = [];

    public SettingGroup(string guid, string name, string description) {
        GUID = guid;
        Name = name;
        Description = description;
    }
}
