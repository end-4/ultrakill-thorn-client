using System;
using UnityEngine;

namespace ThornClient.Managers;

/// <summary>
/// The manager that runs the rendering pipeline for modules.
/// </summary>
public static class RenderManager {
    /// <summary>
    /// The simple colored shader
    /// </summary>
    public static Shader Colored;

    /// <summary>
    /// The material used for rendering lines and other flat graphics.
    /// </summary>
    public static Material Line;

    /// <summary>
    /// The line material that's always on top
    /// </summary>
    public static Material LineTop;

    private static readonly int ZTest = Shader.PropertyToID("_ZTest");
    private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

    /// <summary>
    /// Does initializations.
    /// </summary>
    public static void Initialize() {
        // Internal-Colored is a built-in flat shader perfect for lines/ESP
        Colored = Shader.Find("Hidden/Internal-Colored");
        if (Colored != null) {
            Line = new Material(Colored);
            LineTop = new Material(Colored);
            LineTop.SetInt(ZTest, (int)UnityEngine.Rendering.CompareFunction.Always);
            LineTop.SetInt(ZWrite, 0);
        } else {
            Plugin.Log.LogError("Failed to find native rendering shader for RenderManager.");
        }
    }

    /// <summary>
    /// The rendering update loop. This is called from the main plugin class.
    /// </summary>
    public static void RenderPipeline() {
        if (Camera.current != Camera.main) return;
        // if (Camera.current != Camera.main || _lineMaterial == null) return;

        // Line.SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix();

        // Feed render passes from active modules
        foreach (var module in ModuleManager.Items) {
            if (module.IsEnabled) {
                try {
                    module.OnRender();
                } catch (Exception e) {
                    Plugin.Log.LogError($"Error rendering module '{module.Name}': {e.Message}");
                }
            }
        }

        GL.PopMatrix();
    }
}
