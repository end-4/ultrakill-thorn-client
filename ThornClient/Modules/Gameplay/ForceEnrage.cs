using System;
using NukeLib.Game;
using NukeLib.Utils;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Managers;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Gameplay;

/// <summary>
/// Module that forces enemies to be enraged, if applicable.
/// </summary>
public class ForceEnrage : Module {
    // /// <summary>
    // /// Icon of the module
    // /// </summary>
    // public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "angry"); // TODO add this icon to bundle and use it

    /// <inheritdoc />
    public override string[] Tags => ["buff", "angry", "pissed"];

    /// <inheritdoc />
    public ForceEnrage() : base("thorn.forceEnrage", "Force Enrage", "Makes enemies instantly enraged",
        ModuleCategory.Gameplay) {
    }


    /// <inheritdoc />
    public override string CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    /// <inheritdoc />
    protected override void OnEnable() {
        UpdateRage(true);
        CheatManager.UpdateCheatiness();
        SceneUtils.SafeSceneLoadedNoParam += AddInfoLine;
        SceneUtils.SafeSceneLoadedDelayedNoParam += UpdateRage;
        EnemyEvents.OnSpawn += Enrage;
        AddInfoLine();
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        UpdateRage(false);
        EnemyEvents.OnSpawn -= Enrage;
        SceneUtils.SafeSceneLoadedNoParam -= AddInfoLine;
        SceneUtils.SafeSceneLoadedDelayedNoParam -= UpdateRage;
        FinalRankHelper.RemoveInfoLine(InfoLine);
    }

    private const string InfoLine = "<color=#fff>+ <color=#ff0000>RAGEBAITED</color>";

    private void AddInfoLine() {
        FinalRankHelper.AddInfoLine(InfoLine);
    }

    private void UpdateRage() {
        UpdateRage(IsEnabled);
    }

    private void UpdateRage(bool enabled) {
        foreach (EnemyIdentifier eid in Object.FindObjectsOfType<EnemyIdentifier>()) {
            if (enabled) Enrage(eid);
            else UnEnrage(eid);
        }
    }

    private void Enrage(EnemyIdentifier eid) {
        Enrage(eid.gameObject);
    }

    private void UnEnrage(EnemyIdentifier eid) {
        Enrage(eid.gameObject, false);
    }

    private void Enrage(GameObject obj, bool enraged = true) {
        // Delaying 1 frame makes it work for Swordsmachines
        ExecutionUtils.RunNextFrame(() => {
            if (obj == null) return;
            IEnrage comp = obj.GetComponent<IEnrage>();
            if (comp != null) {
                if (enraged) comp.Enrage();
                else comp.UnEnrage();
                return;
            }

            // ffs hakita pls use the interface
            // Known weird stuff: MaliciousFace.Enrage, Power.EnrageNow
            // They don't even have an un-enrage but it put this here just in case...
            string[] targetMethods = enraged ? ["Enrage", "EnrageNow"] : ["UnEnrage", "UnEnrageNow"];

            foreach (Component component in obj.GetComponents<Component>()) {
                if (component != null && component.TryInvokeAny(targetMethods)) {
                    break;
                }
            }
        });
    }
}
