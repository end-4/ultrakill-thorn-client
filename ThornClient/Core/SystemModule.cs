namespace ThornClient.Core;

/// <summary>
/// Abstract class for Modules that are hidden and always enabled
/// </summary>
public abstract class SystemModule : Module {
    public sealed override string CheatReason => "";

    internal SystemModule(string guid, string name, string description)
        : base(guid, name, description, ModuleCategory.Misc, hasToggling: false) {

    }
}
