using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private static Dictionary<HudSurface, GameObject> _surfaces = new Dictionary<HudSurface, GameObject>();

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
        SceneUtils.SafeSceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        try {
            // Plugin.Log.LogInfo($"[HUD Manager] Preparing for scene '{SceneHelper.CurrentScene}'...");
            var rootGameObjects = scene.GetRootGameObjects();
            // Plugin.Log.LogInfo($"[HUD Manager] Root objects: {rootGameObjects.Length}");
            var player = rootGameObjects.Where(obj => obj.name == "Player").FirstOrDefault();
            var canvas = rootGameObjects.Where(obj => obj.name == "Canvas").FirstOrDefault();
            // Plugin.Log.LogInfo($"[HUD Manager] player: {player}, canvas: {canvas}");
            if (player == null) return;
            // Plugin.Log.LogInfo($"[HUD Manager] pass player nonnull");
            var hud = player.FindRecursive("Main Camera/HUD Camera/HUD");
            // Plugin.Log.LogInfo($"[HUD Manager] hud {hud}");
            if (canvas == null || hud == null) return;
            _surfaces[HudSurface.Left] = hud.FindRecursive("GunCanvas");
            _surfaces[HudSurface.Right] = hud.FindRecursive("StyleCanvas");
            _surfaces[HudSurface.Overlay] = canvas.FindRecursive("Crosshair Filler");
            // Plugin.Log.LogInfo(
            //     $"[HUD Manager] surfaces {_surfaces[HudSurface.Left]}, {_surfaces[HudSurface.Right]}, {_surfaces[HudSurface.Overlay]}");
            if (_surfaces.Values.Any(g => g == null)) return;
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
