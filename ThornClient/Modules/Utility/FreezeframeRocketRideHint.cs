using System;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.Utility;

/// <summary>
/// Module that draws lines to help with Freezeframe rocket riding.
/// </summary>
public class FreezeframeRocketRideHint : Module {
    /// <summary>
    /// When to show rocket ride hint
    /// </summary>
    public enum RocketHintShowWhen {
        /// <summary>
        /// Only when holding Freezeframe
        /// </summary>
        Freezeframe,

        /// <summary>
        /// When holding any Rocket Launcher
        /// </summary>
        RocketLauncher,

        /// <summary>
        /// Always show
        /// </summary>
        Always
    }

    /// <summary>
    /// When to show the hint lines
    /// </summary>
    public Setting<RocketHintShowWhen> ShowWhen { get; }

    /// <summary>
    /// The color of the line.
    /// </summary>
    public Setting<Color> LineColor { get; }

    /// <summary>
    /// The thickness of the line.
    /// </summary>
    public Setting<float> LineThickness { get; }

    /// <summary>
    /// The width of the line.
    /// </summary>
    public Setting<float> LineWidth { get; }

    /// <summary>
    /// How far in front of the player the line should be.
    /// </summary>
    public Setting<float> Distance { get; }

    /// <summary>
    /// The vertical angle of the upper line.
    /// </summary>
    public Setting<float> UpperAngle { get; }

    /// <summary>
    /// The vertical angle of the lower line.
    /// </summary>
    public Setting<float> LowerAngle { get; }

    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "rocket_ride");

    /// <inheritdoc />
    public override string[] Tags => ["movement", "freezeframe"];

    /// <inheritdoc />
    public FreezeframeRocketRideHint() : base("thorn.freezeframeRocketRideHint", "Rocket Ride Hint",
        "Draws assist lines to help with Freezeframe rocket ride aiming", ModuleCategory.Utility) {
        ShowWhen = CreateSetting("showWhen", "Show when holding", "When to show the hint lines",
            RocketHintShowWhen.Freezeframe);
        LineColor = CreateSetting("lineColor", "Line Color", "Color used for the hint line.",
            new Color(64f / 255f, 232f / 255f, 1f));
        LineThickness = CreateSetting("lineThickness", "Line Thickness", "How thicc the line is", 2f);
        LineWidth = CreateSetting("lineWidth", "Line Width", "How long the line is", 1f);
        Distance = CreateSetting("distance", "Distance", "How far away from the camera to show the lines", 3f);
        UpperAngle = CreateSetting("upperAngle", "Upper Angle", "The vertical angle of the upper line, in degrees.",
            7f);
        UpperAngle.Hints = new InterfaceHints { Range = Tuple.Create(-90f, 90f) };
        LowerAngle = CreateSetting("lowerAngle", "Lower Angle", "The vertical angle of the lower line, in degrees.",
            34f);
        LowerAngle.Hints = new InterfaceHints { Range = Tuple.Create(-90f, 90f) };
    }

    private GunControl? gc => GunControl.Instance;

    /// <summary>
    /// The rendering loop.
    /// </summary>
    public override void OnRender() {
        switch (ShowWhen.Value) {
            case RocketHintShowWhen.Always:
                break;
            case RocketHintShowWhen.RocketLauncher:
                if (gc == null || gc.currentWeapon == null ||
                    gc.currentWeapon.GetComponent<RocketLauncher>() == null) return;
                break;
            case RocketHintShowWhen.Freezeframe:
                if (gc == null || gc.currentWeapon == null ||
                    gc.currentWeapon.GetComponent<RocketLauncher>() == null ||
                    gc.currentWeapon.GetComponent<RocketLauncher>().variation != 0) return;
                break;
        }

        var mainCam = Camera.main;
        if (mainCam == null) return;

        if (RenderManager.LineTop == null) return;

        RenderManager.LineTop.SetPass(0);

        // Render in 3D space
        GL.PushMatrix();
        GL.MultMatrix(mainCam.worldToCameraMatrix);
        GL.LoadProjectionMatrix(mainCam.projectionMatrix);

        GL.Begin(GL.QUADS);
        GL.Color(LineColor.Value);

        float thickness = LineThickness.Value * 0.01f;

        Vector3 cameraPos = mainCam.transform.position;
        Vector3 cameraForward = mainCam.transform.forward;
        Vector3 flatForward = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;

        DrawHintLine(UpperAngle.Value, cameraPos, flatForward, thickness);
        DrawHintLine(LowerAngle.Value, cameraPos, flatForward, thickness);

        GL.End();
        GL.PopMatrix();
    }

    private void DrawHintLine(float angleDegrees, Vector3 cameraPos, Vector3 flatForward, float thickness) {
        float angleInRadians = angleDegrees * -1 * Mathf.Deg2Rad;
        Vector3 direction = flatForward * Mathf.Cos(angleInRadians) + Vector3.up * Mathf.Sin(angleInRadians);

        Vector3 lineCenter = cameraPos + direction * Distance.Value;
        Vector3 lineDirection = Vector3.Cross(Vector3.up, flatForward).normalized;
        Vector3 startPoint = lineCenter - lineDirection * (LineWidth.Value / 2);
        Vector3 endPoint = lineCenter + lineDirection * (LineWidth.Value / 2);

        Vector3 camToLineCenterDir = (lineCenter - cameraPos).normalized;
        Vector3 perpendicular = Vector3.Cross(endPoint - startPoint, camToLineCenterDir).normalized * (thickness / 2f);

        GL.Vertex(startPoint - perpendicular);
        GL.Vertex(startPoint + perpendicular);
        GL.Vertex(endPoint + perpendicular);
        GL.Vertex(endPoint - perpendicular);
    }
}
