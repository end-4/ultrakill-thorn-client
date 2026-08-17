using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Managers;

/// <summary>
/// The managers that loads all assets of requested bundles at once, then expose them via a single getter
/// </summary>
public static class AssetManager {
    // Key format: "bundleKey/typeName/assetName"
    private static readonly Dictionary<string, Object> Objects = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, AssetBundle> LoadedBundles = new();

    static AssetManager() {
    }

    /// <summary>
    /// Initializes the AssetManager.
    /// </summary>
    public static void Initialize() {
    }

    /// <summary>
    /// Load a bundle and keep all of its assets in memory for later retrieval. If the bundle is already loaded, this method does nothing.
    /// </summary>
    /// <param name="bundleKey">The key for the bundle for later retrieval</param>
    /// <param name="bundlePath">The path to the bundle file</param>
    public static void LoadBundle(string bundleKey, string bundlePath) {
        if (LoadedBundles.ContainsKey(bundleKey)) return;

        var bundle = AssetBundle.LoadFromFile(bundlePath);
        if (bundle == null) {
            Plugin.Log.LogError($"[AssetManager] Failed to load bundle at {bundlePath}");
            return;
        }

        LoadedBundles[bundleKey] = bundle;

        var allAssets = bundle.LoadAllAssets<Object>();
        foreach (var asset in allAssets) {
            // Include asset.GetType().Name in the key so Sprites and Texture2Ds never collide!
            string key = BuildKey(bundleKey, asset.GetType(), asset.name);

            if (!Objects.TryAdd(key, asset)) {
                Plugin.Log.LogWarning(
                    $"[AssetManager] Duplicate asset key detected: '{asset.name}' of type '{asset.GetType().Name}' in bundle '{bundleKey}'");
            }
        }
    }

    /// <summary>
    /// Get a preloaded asset of type T
    /// </summary>
    /// <param name="bundleKey">The bundle key</param>
    /// <param name="assetName">The name of the asset in the bundle</param>
    /// <typeparam name="T">The type of the asset</typeparam>
    /// <returns>The asset</returns>
    public static T Get<T>(string bundleKey, string assetName) where T : Object {
        // Try exact type lookup first (e.g. "clickGui/Sprite/sun")
        string exactKey = BuildKey(bundleKey, typeof(T), assetName);
        if (Objects.TryGetValue(exactKey, out var asset)) {
            return (T)asset;
        }

        Plugin.Log.LogError($"[AssetManager] Asset '{assetName}' of type '{typeof(T).Name}' not found in bundle '{bundleKey}'.");
        return null;
    }

    private static string BuildKey(string bundleKey, Type type, string assetName) {
        return $"{bundleKey}/{type.Name}/{assetName}";
    }
}
