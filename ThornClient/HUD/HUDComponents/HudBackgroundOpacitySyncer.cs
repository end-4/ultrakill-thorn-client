using ThornClient.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

public class HudBackgroundOpacitySyncer : MonoBehaviour {
    private Image _bg;

    private PrefsManager? _prefs => PrefsManager.Instance;

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

    private void UpdateOpacity(Scene _, LoadSceneMode __) {
        UpdateOpacity();
    }
}
