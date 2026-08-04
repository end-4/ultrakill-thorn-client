using NukeLib.UI;
using ThornClient.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// Component to control the appearance of a key. Just set the Pressed property as needed.
/// </summary>
public class KeyInputController : MonoBehaviour {
    /// <summary>
    /// The sprite to use for the button background when not pressed
    /// </summary>
    public Sprite baseSprite = AssetManager.Get<Sprite>(HudManager.BundleKey, "Round_BorderLarge");
    /// <summary>
    /// The sprite to use for the button background when pressed
    /// </summary>
    public Sprite baseSpritePressed = AssetManager.Get<Sprite>(HudManager.BundleKey, "Round_FillLarge");
    /// <summary>
    /// The color to use for the button icon when not pressed
    /// </summary>
    public Color iconColor = Color.white;
    /// <summary>
    /// The color to use for the button icon when pressed
    /// </summary>
    public Color iconColorPressed = Color.black;

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

    /// <summary>
    /// Standard Unity Start declaration
    /// </summary>
    protected virtual void Start() {
        _border = gameObject.GetComponent<Image>();
        _icon = gameObject.FindRecursive("Image", warnings: false)?.GetComponent<Image>();
        UpdateState(IsPressed);
    }

    private void UpdateState(bool pressed) {
        if (_border != null) {
            var targetSprite = pressed ? baseSpritePressed : baseSprite;
            if (_border.sprite != targetSprite) _border.sprite = targetSprite;
        }

        if (_icon != null) {
            var targetColor = pressed ? iconColorPressed : iconColor;
            if (_icon.color != targetColor) _icon.color = targetColor;
        }
    }
}
