using System;
using System.Collections.Generic;
using System.Linq;

namespace ThornClient.Core.ConfigurableElements;

public class ConfigButtonRow : IConfigurableElement {
    public string GUID { get; }
    public string Name { get; }
    public string Description { get; }
    public InterfaceHints? Hints { get; set; }
    public Action<int> OnClick { get; set; }
    public List<string> Texts;

    public ConfigButtonRow(string guid, string name, string description, string[] texts) {
        GUID = guid;
        Name = name;
        Description = description;
        Texts = texts.ToList();
    }
}
