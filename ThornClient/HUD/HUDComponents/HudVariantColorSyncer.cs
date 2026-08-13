using NukeLib.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// Component to sync the color of the Image component in the same GameObject to the user configured variant color. Remember to set the Variation property.
/// </summary>
public class HudVariantColorSyncer : MonoBehaviour {
    /// <summary>
    /// The weapon variant. Changing this will update the color of the HUD element accordingly.
    /// </summary>
    public int Variation {
        get;
        set {
            field = value;
            UpdateColor();
        }
    } = 0;

    /// <summary>
    /// The color multiplier. Changing this will update the color of the HUD element accordingly.
    /// </summary>
    public float ColorMultiplier = 1f;

    private Image? _target;

    private void Start() {
        _target = GetComponent<Image>();
        PrefsManager.onPrefChanged += CheckAndUpdate;
        UpdateColor();
    }

    private void OnDestroy() {
        PrefsManager.onPrefChanged -= CheckAndUpdate;
    }

    private void CheckAndUpdate(string key, object? obj) {
        if (key.StartsWith("hudColor")) {
            UpdateColor();
        }
    }

    /// <summary>
    /// Updates the color of the HUD element.
    /// </summary>
    public void UpdateColor() {
        if (_target == null) return;
        var color = ColorUtils.GetWeaponVariantColor(Variation) * ColorMultiplier;
        _target.color = new Color(color.r, color.g, color.b);
    }
}
