using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using UnityEngine;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// A component that sets visibility for children of a GameObject, following a bool Setting
/// </summary>
public class BatchBoolSettingVisibilitySyncer : MonoBehaviour {
    /// <summary>
    /// Pairs to sync visibility. the keys being the setting and the value being the paths to the target item relative to the GameObject this component is added to.
    /// </summary>
    public Dictionary<Setting<bool>, string> SyncPairs = [];

    private Dictionary<string, GameObject?> Cached = [];

    private void Start() {
        foreach (var pair in SyncPairs) {
            pair.Key.OnChanged += SyncVisibilities;
        }
        SyncVisibilities();
    }

    private void OnDestroy() {
        foreach (var pair in SyncPairs) {
            pair.Key.OnChanged -= SyncVisibilities;
        }
    }

    private void SyncVisibilities() {
        foreach (var pair in SyncPairs) {
            var obj = GetObjectFromPath(pair.Value);
            if (obj == null) continue;
            if (obj.activeSelf != pair.Key.Value) {
                obj.SetActive(pair.Key.Value);
            }
        }
        gameObject.UnfuckLayoutHack();
    }

    private GameObject? GetObjectFromPath(string path) {
        if (Cached.ContainsKey(path)) return Cached[path];
        Cached[path] = gameObject.FindRecursive(path);
        return Cached[path];
    }
}
