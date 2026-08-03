namespace ThornClient.Core;

internal abstract class SystemModule : Module {
    public override string CheatReason => "";

    protected SystemModule(string guid, string name, string description)
        : base(guid, name, description, ModuleCategory.Misc, hasToggling: false) {

    }
}
