using System;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

public class HudVariantColorSyncer : MonoBehaviour {
    public int Variation {
        get => field;
        set {
            field = value;
            UpdateColor();
        }
    } = 0;

    public float ColorMultiplier = 1f;

    private Image? _target;

    private void OnEnable() {
        _target = GetComponent<Image>();
        PrefsManager.onPrefChanged += CheckAndUpdate;
        UpdateColor();
    }

    private void OnDisable() {
        PrefsManager.onPrefChanged -= CheckAndUpdate;
    }

    private void CheckAndUpdate(string key, object? obj) {
        if (key.StartsWith("hudColor")) {
            UpdateColor();
        }
    }

    public void UpdateColor() {
        if (_target == null) return;
        var colSet = ColorBlindSettings.Instance;
        if (colSet == null) return;
        var color = colSet.variationColors[Variation] * ColorMultiplier;
        _target.color = new Color(color.r, color.g, color.b);
    }
}
