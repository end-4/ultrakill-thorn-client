using System;
using UnityEngine;

namespace ThornClient.Managers;

/// <summary>
/// The manager that runs the rendering pipeline for modules.
/// </summary>
public static class RenderManager {
    /// <summary>
    /// The material used for rendering lines and other flat graphics.
    /// </summary>
    private static Material _lineMaterial;

    /// <summary>
    /// Does initializations.
    /// </summary>
    public static void Initialize() {
        // Internal-Colored is a built-in flat shader perfect for lines/ESP
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader != null) {
            _lineMaterial = new Material(shader);
        } else {
            Plugin.Log.LogError("Failed to find native rendering shader for RenderManager.");
        }
    }

    /// <summary>
    /// The rendering update loop. This is called from the main plugin class.
    /// </summary>
    public static void RenderPipeline() {
        if (Camera.current != Camera.main || _lineMaterial == null) return;

        _lineMaterial.SetPass(0);
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
