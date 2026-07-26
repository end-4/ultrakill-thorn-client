using System;
using UnityEngine;

namespace ThornClient.Managers;

public static class RenderManager {
    private static Material _lineMaterial;

    public static void Initialize() {
        // Internal-Colored is a built-in flat shader perfect for lines/ESP
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader != null) {
            _lineMaterial = new Material(shader);
        } else {
            Plugin.Log.LogError("Failed to find native rendering shader for RenderManager.");
        }
    }

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
