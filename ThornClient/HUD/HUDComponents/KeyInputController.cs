using NukeLib.UI;
using ThornClient.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// Component to control the appearance of a key. Just set the Pressed property as needed.
/// </summary>
public class KeyInputController : MonoBehaviour {
    private Image? _border;
    private Image? _icon;

    /// <summary>
    /// Whether the button is currently pressed. Just update this and visuals will follow.
    /// </summary>
    public bool IsPressed {
        get;
        set {
            if (field == value) return;
            field = value;
            UpdateState(value);
        }
    } = false;

    private void Start() {
        _border = gameObject.GetComponent<Image>();
        _icon = gameObject.FindRecursive("Image")?.GetComponent<Image>();
        UpdateState(IsPressed);
    }

    private void UpdateState(bool pressed) {
        if (_border != null) {
            var targetSprite =
                AssetManager.Get<Sprite>(HudManager.BundleKey, pressed ? "Round_FillLarge" : "Round_BorderLarge");
            if (_border.sprite != targetSprite) _border.sprite = targetSprite;
        }

        if (_icon != null) {
            var targetColor = pressed ? Color.black : Color.white;
            if (_icon.color != targetColor) _icon.color = targetColor;
        }
    }
}
