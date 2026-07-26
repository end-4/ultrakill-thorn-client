using System.Collections;
using NukeLib.Game.Gameplay;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.Render;

public class ParryEffect : Module {
    public Setting<float> Fov { get; }

    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "feedbacker");

    public ParryEffect() : base("thorn.parryEffect", "Parry Effect",
        "Adds a slight zoom to make parries feel more impactful",
        ModuleCategory.Render, KeyCode.None, KeyCode.None) {
        Fov = RegisterSetting("fov", "FOV Adjustment", "The Field of view shift when parrying", -10f);
    }

    protected override void OnEnable() {
        PunchEvents.OnParry += PerformEffects;
    }

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
