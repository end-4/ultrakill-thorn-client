using System.Collections;
using NukeLib.Game;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.Render;

/// <summary>
/// Module that adds effects when parrying
/// </summary>
public class ParryEffect : Module {
    /// <summary>
    /// FOV change when parrying
    /// </summary>
    public Setting<float> Fov { get; }

    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "feedbacker");

    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["punch", "fov"];

    /// <summary>
    /// Constructor
    /// </summary>
    public ParryEffect() : base("thorn.parryEffect", "Parry Effect",
        "Adds a slight zoom to make parries feel more impactful",
        ModuleCategory.Render) {
        Fov = CreateSetting("fov", "FOV Adjustment", "The Field of view shift when parrying", -10f);
    }

    /// <summary>
    /// Stuff that run when enabled
    /// </summary>
    protected override void OnEnable() {
        PunchEvents.OnParry += PerformEffects;
    }

    /// <summary>
    /// Stuff that run when disabled
    /// </summary>
    protected override void OnDisable() {
        PunchEvents.OnParry -= PerformEffects;
    }

    private IEnumerator StopZoomNextFrame() {
        yield return null;
        var cc = CameraController.Instance;
        if (cc != null) {
            cc.StopZoom();
        }
    }

    private void PerformEffects() {
        var cc = CameraController.Instance;
        if (cc != null && !Mathf.Approximately(Fov.Value, 0f)) {
            var adjustedAmount = cc.cam.fieldOfView + Fov.Value;
            cc.Zoom(adjustedAmount);
            cc.cam.fieldOfView = adjustedAmount;
            Plugin.Instance.StartCoroutine(StopZoomNextFrame());
        }
    }
}
