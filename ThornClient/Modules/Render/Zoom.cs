using UnityEngine;
using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.Render;

public class Zoom : Module {
    public Setting<float> ZoomFov { get; }

    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "loupe");

    public Zoom() : base("thorn.zoom", "Zoom",
        "Look more closely. Note: zooming only happens when the game is running, so use a keybind to toggle it",
        ModuleCategory.Render, KeyCode.None, KeyCode.None, true) {
        ZoomFov = RegisterSetting("zoomFov", "Zoom FOV", "Zoom amount. Lower = more zoomed in", 30.0f);
    }

    protected override void OnEnable() {
        UpdateZoom(ZoomFov.Value);
        ZoomFov.OnValueChanged += UpdateZoom;
    }

    public override void OnUpdate() {
    }

    private void UpdateZoom(float value) {
        var cc = CameraController.Instance;
        if (cc == null) return;
        cc.Zoom(ZoomFov.Value);
    }

    protected override void OnDisable() {
        ZoomFov.OnValueChanged -= UpdateZoom;
        var cc = CameraController.Instance;
        if (cc == null) return;
        cc.StopZoom();
    }
}
