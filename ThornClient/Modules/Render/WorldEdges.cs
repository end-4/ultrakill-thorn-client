using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.Render;

// ------------------
// Thank you Bryan :3
// ------------------

/// <summary>
/// Module that applies an "edges" look to the world
/// </summary>
public class WorldEdges : Module {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "icosahedron");

    /// <inheritdoc />
    public override string[] Tags => ["wireframe", "world"];

    public Setting<Color> FillColor;

    /// <summary>
    /// Constructor
    /// </summary>
    public WorldEdges() : base("thorn.worldEdges", "Edges (World)", "Applies wireframe shader to the world",
        ModuleCategory.Render) {
        FillColor = CreateSetting("fillColor", "Fill color", "The color that fills in the surfaces", Color.black);
    }

    /// <inheritdoc />
    protected override void OnEnable() {
        Camera.onPreRender += PreCamHook;
        Camera.onPostRender += PostCamHook;
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        Camera.onPreRender -= PreCamHook;
        Camera.onPostRender -= PostCamHook;
    }

    private void PreCamHook(Camera cam) {
        // Only target main cam
        if (cam != Camera.main) return;

        // Clear previous frame
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = FillColor.Value;

        // Tell Unity to use wireframe bullshit b4 this camera starts rendering anything
        GL.wireframe = true;
    }

    private void PostCamHook(Camera cam) {
        if (cam != Camera.main)
            return;

        // Disable wireframe so other camera's are unaffected
        GL.wireframe = false;
    }
}
