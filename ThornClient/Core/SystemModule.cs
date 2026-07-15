using UnityEngine;

namespace ThornClient.Core;

internal abstract class SystemModule : Module {
    public override string CheatReason => "";

    protected SystemModule(string guid, string name, string description, KeyCode defaultKey = KeyCode.None)
        : base(guid, name, description, ModuleCategory.Misc, defaultKey) {
        
    }
}
