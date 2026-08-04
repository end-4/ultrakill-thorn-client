using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.Render;

/// <summary>
/// Module that zooms, similar to the railcannon's alt fire
/// </summary>
public class Zoom : Module {
    /// <summary>
    /// The FOV that is applied when zooming
    /// </summary>
    public Setting<float> ZoomFov { get; }

    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "loupe");

    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["magnifier", "close", "see"];

    /// <summary>
    /// Constructor
    /// </summary>
    public Zoom() : base("thorn.zoom", "Zoom",
        "Look more closely. Note: zooming only happens when the game is running, so use a keybind to toggle it",
        ModuleCategory.Render, KeyCode.None, KeyCode.None, defaultToggleOnRelease: true) {
        ZoomFov = CreateSetting("zoomFov", "Zoom FOV", "Zoom amount. Lower = more zoomed in", 30.0f);
    }

    /// <summary>
    /// Stuff that run when enabled
    /// </summary>
    protected override void OnEnable() {
        UpdateZoom(ZoomFov.Value);
        ZoomFov.OnValueChanged += UpdateZoom;
    }
    
    private void UpdateZoom(float value) {
        var cc = CameraController.Instance;
        if (cc == null) return;
        cc.Zoom(ZoomFov.Value);
    }

    /// <summary>
    /// Stuff that run when disabled
    /// </summary>
    protected override void OnDisable() {
        ZoomFov.OnValueChanged -= UpdateZoom;
        var cc = CameraController.Instance;
        if (cc == null) return;
        cc.StopZoom();
    }
}
