using NukeLib.Game;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.Render;

/// <summary>
/// Module that adds an overlay to portals
/// </summary>
public class VisiblePortals : Module {
    /// <summary>
    /// Fill color
    /// </summary>
    public Setting<Color> FillColor;

    /// <summary>
    /// Whether to draw border around the fill
    /// </summary>
    public Setting<bool> BorderEnabled;

    /// <summary>
    /// The color of the portal border
    /// </summary>
    public Setting<Color> BorderColor;

    /// <summary>
    /// Distance to push the overlay away (when drawn precisely at the portal it'll constantly flash)
    /// </summary>
    public Setting<float> Offset;

    /// <summary>
    /// ESP basically
    /// </summary>
    public Setting<bool> AlwaysOnTop;

    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "circle_cross");

    /// <inheritdoc />
    public override string[] Tags => ["portal", "accessibility", "overlay", "translucent", "lines", "visual"];

    /// <inheritdoc />
    public VisiblePortals() : base("thorn.visiblePortals", "Visible Portals",
        "Draws translucent overlay on portals for accessibility",
        ModuleCategory.Render) {
        FillColor = CreateSetting("fillColor", "Fill color",
            "Fill color of the portal overlays",
            new Color(0.21f, 0.69f, 1f, 0.1f));

        BorderEnabled = CreateSetting("borderEnabled", "Enable border",
            "Whether to draw an outline", true);

        BorderColor = CreateSetting("borderColor", "Border color", "Color of the outline",
            new Color(0.65f, 0.78f, 1f, 1f));

        var advancedGroup = CreateGroup("advanced", "Advanced", "Options that you most likely don't need to touch");

        Offset = CreateSetting("offset", "Offset",
            "Distance away from the portal to draw the overlay. Having a value of 0 is most accurate but causes flashing.",
            0.03f, advancedGroup);

        AlwaysOnTop = CreateSetting("alwaysOnTop", "Always on top",
            "Portal ESP basically", false, advancedGroup);
    }

    /// <inheritdoc />
    public override void OnRender() {
        var mainCam = Camera.main;
        if (mainCam == null) return;

        var portals = PortalHelper.ActivePortals;
        if (portals.Count == 0) return;

        var material = AlwaysOnTop.Value ? RenderManager.LineTop : RenderManager.Line;
        if (material == null) return;

        material.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(mainCam.worldToCameraMatrix);
        GL.LoadProjectionMatrix(mainCam.projectionMatrix);

        Vector3 camPos = mainCam.transform.position;
        float offset = Offset.Value;

        // Fill
        GL.Begin(GL.QUADS);
        GL.Color(FillColor.Value);

        foreach (var portal in portals) {
            if (portal == null || !portal.gameObject.activeInHierarchy) continue;

            Transform entry = portal.entry != null ? portal.entry : portal.transform;
            PortalHelper.GetPortalCorners(entry, portal, out var p0, out var p1, out var p2,
                out var p3, offsetDistance: offset, cameraPos: camPos);
            GL.Vertex(p0);
            GL.Vertex(p1);
            GL.Vertex(p2);
            GL.Vertex(p3);

            if (portal.exit != null && portal.exit != portal.entry) {
                PortalHelper.GetPortalCorners(portal.exit, portal, out var ep0, out var ep1,
                    out var ep2, out var ep3, offsetDistance: offset, cameraPos: camPos);
                GL.Vertex(ep0);
                GL.Vertex(ep1);
                GL.Vertex(ep2);
                GL.Vertex(ep3);
            }
        }

        GL.End();

        // Borders
        if (BorderEnabled.Value && BorderColor.Value.a > 0.01f) {
            GL.Begin(GL.LINES);
            GL.Color(BorderColor.Value);

            foreach (var portal in portals) {
                if (portal == null || !portal.gameObject.activeInHierarchy) continue;

                Transform entry = portal.entry != null ? portal.entry : portal.transform;
                PortalHelper.GetPortalCorners(entry, portal, out var p0, out var p1, out var p2,
                    out var p3, offsetDistance: offset, cameraPos: camPos);
                DrawQuadOutline(p0, p1, p2, p3);

                if (portal.exit != null && portal.exit != portal.entry) {
                    PortalHelper.GetPortalCorners(portal.exit, portal, out var ep0, out var ep1,
                        out var ep2, out var ep3, offsetDistance: offset, cameraPos: camPos);
                    DrawQuadOutline(ep0, ep1, ep2, ep3);
                }
            }

            GL.End();
        }

        GL.PopMatrix();
    }

    private static void DrawQuadOutline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
        GL.Vertex(p0);
        GL.Vertex(p1);
        GL.Vertex(p1);
        GL.Vertex(p2);
        GL.Vertex(p2);
        GL.Vertex(p3);
        GL.Vertex(p3);
        GL.Vertex(p0);
    }
}
