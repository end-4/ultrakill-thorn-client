using ThornClient.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// A component that makes the opacity of the Image component in the same GameObject follow the hudBackgroundOpacity config value from the base game.
/// </summary>
public class HudBackgroundOpacitySyncer : MonoBehaviour {
    private Image? _bg;

    private PrefsManager? _prefs => PrefsManager.Instance;

    /// <summary>
    /// If true forces the background to be transparent. This is used for when the a FramedHudModule is configured to hide the background.
    /// </summary>
    public bool ForceTransparent {
        get;
        set {
            field = value;
            UpdateOpacity();
        }
    }

    private void Start() {
        _bg = gameObject.GetComponent<Image>();
        PrefsManager.onPrefChanged += CheckAndUpdate;
        HudManager.ReadyForScene += UpdateOpacity;
        if (_prefs != null) UpdateOpacity();
    }

    private void OnDestroy() {
        PrefsManager.onPrefChanged -= CheckAndUpdate;
        HudManager.ReadyForScene -= UpdateOpacity;
    }

    private void CheckAndUpdate(string key, object? obj) {
        if (key == "hudBackgroundOpacity") {
            if (float.TryParse(obj?.ToString(), out float opacity)) {
                UpdateOpacity(opacity / 100f);
            }
        }
    }

    /// <summary>
    /// Updates opacity. Range = [0, 1]
    /// </summary>
    /// <param name="opacity"></param>
    protected void UpdateOpacity(float opacity) {
        if (_bg != null) {
            _bg.color = new Color(0, 0, 0, ForceTransparent ? 0 : opacity);
        }
    }

    /// <summary>
    /// Updates opacity based on current configurtion
    /// </summary>
    protected void UpdateOpacity() {
        if (_prefs == null) return;
        var opacity = _prefs.GetFloat("hudBackgroundOpacity", 0.5f);
        UpdateOpacity(opacity / 100f);
    }
}
