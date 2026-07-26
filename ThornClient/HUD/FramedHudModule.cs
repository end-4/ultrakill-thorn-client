using ThornClient.Core;
using ThornClient.HUD.HUDComponents;
using ThornClient.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD;

/// <summary>
/// HUD element with a background,
/// dynamically sized based on content (you need min size/preferred size on your content element for that)
/// </summary>
public abstract class FramedHudModule : HudModule {
    public FramedHudModule(string guid, string name, string description) : base(guid, name, description) {
    }

    protected GameObject? Background;


    /// <summary>
    /// The method to create a content object.
    /// Make sure it contains a layout element, so the background can size itself appropriately
    /// </summary>
    /// <returns>The GameObject of the content item</returns>
    protected abstract GameObject CreateContentObject();

    protected sealed override GameObject CreateHudObject() {
        GameObject obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "Background"));
        var content = CreateContentObject();
        if (content != null) content.transform.SetParent(obj.transform);
        if (_wrapper != null) {
            _wrapper.AddComponent<HorizontalLayoutGroup>();
            var confit = _wrapper.AddComponent<ContentSizeFitter>();
            confit.horizontalFit = ContentSizeFitter.FitMode.MinSize;
            confit.verticalFit = ContentSizeFitter.FitMode.MinSize;
        }

        Background = obj;
        Background.AddComponent<HudBackgroundOpacitySyncer>();

        return obj;
    }


}
