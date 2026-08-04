using System;
using System.Collections.Generic;
using System.Linq;
using ThornClient.Core;
using UnityEngine;
using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.HUD;

public class ActiveModules : TextHudModule {
    private SortedList<string, Module> modules = [];
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "list");
    public override string[] Tags => ["module", "control", "system"];

    public ActiveModules() : base("thorn.activeModules", "Active Modules", "A list of active modules") {
        UpdateWholeList();
    }

    protected override void OnEnable() {
        ModuleManager.SomeModuleToggled += UpdateWholeList;
        UpdateWholeList();
    }

    protected override void OnDisable() {
        ModuleManager.SomeModuleToggled -= UpdateWholeList;
    }

    private void UpdateWholeList(Module module, bool enabled) {
        UpdateWholeList();
    }

    /// <summary>
    /// Updates the whole list
    /// </summary>
    private void UpdateWholeList() {
        _modules = new SortedList<string, Module>(ModuleManager.Items
            .Where(m => m.IsEnabled && m is not SystemModule && m is not HudModule)
            .ToDictionary(m => m.Name, m => m));

        SyncText();
    }

    private void SyncText() {
        Text = string.Join("\n", _modules.Values.Select(m => m.Name));
    }
}
