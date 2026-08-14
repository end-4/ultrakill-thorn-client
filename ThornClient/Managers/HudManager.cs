using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThornClient.Managers;

/// <summary>
/// Contains some shared functionality for managing the HUD.
/// </summary>
public static class HudManager {
    private static readonly string BundlePath = Path.Combine(Plugin.workingDir, "assets", "thorn_hud.bundle");

    /// <summary>
    /// The key of the asset bundle used for HUD assets.
    /// </summary>
    public static readonly string BundleKey = "hud";

    private static Dictionary<HudSurface, GameObject?> _surfaces = new();

    /// <summary>
    /// Emitted when the HUD manager has finished preparing for a scene and is ready to spawn HUD modules.
    /// </summary>
    public static event Action? ReadyForScene;

    /// <summary>
    /// Scans the assembly via reflection and instantiates + registers HudModules
    /// </summary>
    public static void Initialize() {
        Plugin.Log.LogInfo($"[HUD Manager] Starting...");

        // Assets
        AssetManager.LoadBundle(BundleKey, BundlePath);

        // Hook
        SceneUtils.SafeSceneLoadedDelayed += OnSceneLoaded;
    }

    private const string GunCanvasName = "ThornGunCanvas";
    private const string StyleCanvasName = "ThornStyleCanvas";

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        try {
            var rootGameObjects = scene.GetRootGameObjects();
            var player = rootGameObjects.Where(obj => obj.name == "Player").FirstOrDefault();
            var canvas = rootGameObjects.Where(obj => obj.name == "Canvas").FirstOrDefault();
            var hud = player?.FindRecursive("Main Camera/HUD Camera/HUD");
            if (hud != null) {
                var thornGunCanvas = hud.FindRecursive(GunCanvasName, false);
                var vanillaGunCanvas = hud.FindRecursive("GunCanvas");
                if (thornGunCanvas == null && vanillaGunCanvas != null) {
                    thornGunCanvas = Object.Instantiate(vanillaGunCanvas, vanillaGunCanvas.transform.parent);
                    thornGunCanvas.name = GunCanvasName;
                    thornGunCanvas.transform.SetAsLastSibling();
                }

                _surfaces[HudSurface.Left] = thornGunCanvas ?? vanillaGunCanvas;

                var thornStyleCanvas = hud.FindRecursive(StyleCanvasName, false);
                var vanillaStyleCanvas = hud.FindRecursive("StyleCanvas");
                if (thornStyleCanvas == null && vanillaStyleCanvas != null) {
                    thornStyleCanvas = Object.Instantiate(vanillaStyleCanvas, vanillaStyleCanvas.transform.parent);
                    thornStyleCanvas.name = StyleCanvasName;
                    thornStyleCanvas.transform.SetAsLastSibling();
                }

                _surfaces[HudSurface.Right] = thornStyleCanvas ?? vanillaStyleCanvas;

                // _surfaces[HudSurface.Left] = hud.FindRecursive("GunCanvas");
                // _surfaces[HudSurface.Right] = hud.FindRecursive("StyleCanvas");
            }

            _surfaces[HudSurface.Overlay] = canvas?.FindRecursive("Crosshair Filler");
            if (_surfaces.Values.Any(g => g == null)) return;
            Plugin.Log.LogInfo("[HUD Manager] ReadyForScene...]");
            ReadyForScene?.Invoke();
        } catch (Exception e) {
            Plugin.Log.LogWarning($"[HUD Manager] Scene load hook failed: {e.Message}");
        }
    }

    /// <summary>
    /// Gets the GameObject surface for a given HudSurface enum value.
    /// </summary>
    /// <param name="hudSurface">The enum value</param>
    /// <param name="surface">The resulting GameObject</param>
    /// <returns></returns>
    public static bool GetSurface(HudSurface hudSurface, out GameObject? surface) {
        return _surfaces.TryGetValue(hudSurface, out surface);
    }
}
