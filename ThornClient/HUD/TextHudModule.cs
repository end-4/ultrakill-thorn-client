using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace ThornClient.HUD;

/// <summary>
/// HUD element with a background,
/// dynamically sized based on content (you need min size/preferred size on your content element for that)
/// </summary>
public abstract class TextHudModule : FramedHudModule {
    public string Text {
        get;
        set {
            if (field == value) return;
            field = value;

            UpdateText(value);
        }
    }

    public TextHudModule(string guid, string name, string description) : base(guid, name, description) {
    }

    private GameObject? _textObj;
    private TextMeshProUGUI? _textComp;

    private void UpdateText(string value) {
        if (_textComp == null) return;
        if (value != _textComp.text) _textComp.text = value;

        if (_textObj != null) _textObj.UnfuckLayoutHack();
        if (_content != null) _content.UnfuckLayoutHack();
        if (_wrapper != null) _wrapper.UnfuckLayoutHack();
    }

    /// <summary>
    /// The method to create a content object.
    /// Make sure it contains a layout element, so the background can size itself appropriately
    /// </summary>
    /// <returns>The GameObject of the content item</returns>
    protected override GameObject CreateContentObject() {
        GameObject obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "TextLayout"));
        _textObj = obj.FindRecursive("Text");
        if (_textObj != null) _textComp = _textObj.GetComponent<TextMeshProUGUI>();
        UpdateText(Text);
        return obj;
    }
}
