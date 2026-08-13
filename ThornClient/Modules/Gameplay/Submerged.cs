using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using NukeLib.Game;
using NukeLib.Utils;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Gameplay;

/// <summary>
/// Module that puts everything in water
/// </summary>
public class Submerged : Module {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "water");

    /// <inheritdoc />
    public override string[] Tags => ["water", "wet", "ocean", "sea", "wrath"];

    /// <inheritdoc />
    public override string CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    private GameObject? _water;

    public Setting<Color> WaterColor;

    /// <inheritdoc />
    public Submerged() : base(
        "thorn.submerged", "Submerged",
        "Makes the world full of water",
        ModuleCategory.Gameplay
    ) {
        WaterColor = CreateSetting("waterColor", "Water color (re-enable after changing)", "Color of the water",
            new Color(0f, 0.5f, 1f));
    }

    private static string InfoLine = "<color=#fff>+ <color=#1fe4f2>ATLANTIS</color>";

    protected override void OnEnable() {
        SceneUtils.SafeSceneLoadedNoParam += EnsureWater;
        SceneUtils.SafeSceneLoadedNoParam += AddInfoLine;
        EnsureWater(true);
        WaterColor.OnValueChanged += SyncWaterColor;
        FinalRankHelper.AddInfoLine(InfoLine);
        CheatManager.UpdateCheatiness();
        AddInfoLine();
    }

    protected override void OnDisable() {
        SceneUtils.SafeSceneLoadedNoParam -= EnsureWater;
        SceneUtils.SafeSceneLoadedNoParam -= AddInfoLine;
        WaterColor.OnValueChanged -= SyncWaterColor;
        EnsureWater(false);
        FinalRankHelper.RemoveInfoLine(InfoLine);
    }

    private void AddInfoLine() {
        FinalRankHelper.AddInfoLine(InfoLine);
    }

    private void SyncWaterColor(Color color) {
        if (!SceneUtils.IsSafe()) return;
        if (_water == null || _water.GetComponent<Water>() == null) return;
        _water.GetComponent<Water>().clr = color;
    }

    private void EnsureWater() {
        EnsureWater(true);
    }

    private void EnsureWater(bool on) {
        if (!SceneUtils.IsSafe()) return;
        if (on) {
            if (_water != null) return;
            _water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _water.name = "ThornSubmergedWater";
            _water.GetOrAddComponent<Rigidbody>().isKinematic = true;
            _water.GetOrAddComponent<Collider>().isTrigger = true;
            _water.GetOrAddComponent<Water>().clr = WaterColor.Value;
            _water.GetOrAddComponent<MeshRenderer>().enabled = false;
            _water.transform.localScale = Vector3.one * 9999999999;
        } else {
            Object.Destroy(_water);
        }
    }
}
