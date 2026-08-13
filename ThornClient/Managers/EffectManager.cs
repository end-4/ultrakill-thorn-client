using System.IO;
using UnityEngine;

namespace ThornClient.Managers;

public static class EffectManager {
    private static readonly string BundlePath = Path.Combine(Plugin.workingDir, "assets", "thorn_effects.bundle");
    public static readonly string BundleKey = "effects";

    public static void Initialize() {
    }

    static EffectManager() {
        AssetManager.LoadBundle(BundleKey, BundlePath);
    }

    public static Material GetMaterial(string materialName) {
        return AssetManager.Get<Material>(BundleKey, materialName);
    }
}
