using NukeLib.UI;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD;

/// <summary>
/// HUD element with a background,
/// dynamically sized based on content (you need min size/preferred size on your content element for that)
/// </summary>
public abstract class TextHudModule : FramedHudModule {
    /// <summary>
    /// The text displayed by this HUD module. Changes are reactive.
    /// </summary>
    public string Text {
        get;
        set {
            if (field == value) return;
            field = value;

            UpdateText(value);
        }
    }

    /// <summary>
    /// The icon displayed by this HUD module. Changes are reactive. Set to null to hide the icon.
    /// </summary>
    public virtual Sprite? DisplayIcon {
        get;
        set {
            if (field == value) return;
            field = value;

            UpdateIcon(value);
        }
    } = null;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guid">The unique identifier for the module</param>
    /// <param name="name">The name of the module</param>
    /// <param name="description">The description of the module</param>
    public TextHudModule(string guid, string name, string description) : base(guid, name, description) {
    }

    private GameObject? _textObj;
    private TextMeshProUGUI? _textComp;
    private Image? _icon;

    private void UpdateText(string value) {
        if (_textComp == null) return;
        // bool newWidth = value.Length != _textComp.text.Length;
        if (value != _textComp.text) {
            _textComp.text = value;
            UnfuckAll();
        }
        // if (newWidth) UnfuckAll();
    }

    private void UpdateIcon(Sprite? icon) {
        if (_icon == null) return;
        if (icon == null) {
            _icon.gameObject.SetActive(false);
        } else {
            _icon.gameObject.SetActive(true);
            _icon.sprite = icon;
        }
    }

    private void UnfuckAll() {
        if (_textObj != null) _textObj.UnfuckLayoutHack();
        if (_content != null) _content.UnfuckLayoutHack();
        if (_wrapper != null) _wrapper.UnfuckLayoutHack();
    }

    /// <summary>
    /// Creates the content object for this HUD module. This method is sealed and should not be overridden by subclasses.
    /// </summary>
    /// <returns>The content object (which is a row containing the icon and text) to put on the frame</returns>
    protected sealed override GameObject CreateContentObject() {
        GameObject obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "TextLayout"));
        _textObj = obj.FindRecursive("Text");
        _icon = obj.FindRecursive("Icon")?.GetComponent<Image>();
        if (_textObj != null) _textComp = _textObj.GetComponent<TextMeshProUGUI>();
        UpdateText(Text);
        UpdateIcon(DisplayIcon);
        return obj;
    }
}
