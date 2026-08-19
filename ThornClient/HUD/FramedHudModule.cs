using NukeLib.Utils;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.UI;
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
    /// <summary>
    /// Setting: whether to show a frame behind the content
    /// </summary>
    public Setting<bool> ShowBackground;

    public Setting<float> Scale;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guid">The unique identifier for the module</param>
    /// <param name="name">The name of the module</param>
    /// <param name="description">The description of the module</param>
    public FramedHudModule(string guid, string name, string description) : base(guid, name, description) {
        ShowBackground = CreateSetting("showBackground", "Show Background",
            "Whether to show a frame behind the content", true);
        ShowBackground.OnValueChanged += SetShowBackground;
        Scale = CreateSetting("scale", "Scale", "Makes the content bigger or smaller", 1f);
        Scale.OnChanged += SetScaleNextFrame;
        PivotX.OnChanged += UpdatePivotNextFrame;
        PivotY.OnChanged += UpdatePivotNextFrame;
        OnToggleStateChanged += _ => {
            SetShowBackground(ShowBackground.Value);
            SetScaleNextFrame();
            UpdatePivotNextFrame();
        };
    }

    private void UpdatePivotNextFrame() {
        ExecutionUtils.RunNextFrame(UpdatePivot);
    }

    private void UpdatePivot() {
        if (_content == null || _wrapper == null) return;
        var wTrans = _wrapper.transform as RectTransform;
        var cTrans = _content.transform as RectTransform;
        if (wTrans == null || cTrans == null) return;
        cTrans.pivot = wTrans.pivot;
    }

    private void SetShowBackground(bool value) {
        if (_opacitySyncer != null) _opacitySyncer.ForceTransparent = !value;
    }

    private void SetScaleNextFrame() {
        ExecutionUtils.RunNextFrame(SetScale);
    }

    private void SetScale() {
        SetScale(Scale.Value);
    }

    private void SetScale(float value) {
        if (_content != null) _content.transform.localScale = Vector3.one * value;
    }

    /// <summary>
    /// The frame object that holds the content object.
    /// </summary>
    protected GameObject? Background;

    /// <summary>
    /// The component that syncs the background opacity with the base game's HUD opacity setting.
    /// </summary>
    protected HudBackgroundOpacitySyncer? _opacitySyncer;

    /// <summary>
    /// The method to create a content object.
    /// Make sure it contains a layout element, so the background can size itself appropriately
    /// </summary>
    /// <returns>The GameObject of the content item</returns>
    protected abstract GameObject CreateContentObject();

    /// <summary>
    /// Creates the HUD object that's the frame to hold the content object.
    /// </summary>
    /// <returns>The frame object</returns>
    protected sealed override GameObject CreateHudObject() {
        GameObject obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "Background"));
        var content = CreateContentObject();
        if (content != null) content.transform.SetParent(obj.transform);
        if (_wrapper != null) {
            _wrapper.AddComponent<HorizontalLayoutGroup>();
            var confitNormal = _wrapper.GetOrAddComponent<ContentSizeFitter>();
            if (confitNormal != null) Object.Destroy(confitNormal);
            var confit = _wrapper.GetOrAddComponent<SingularScalableContentSizeFitter>();
            confit.horizontalFit = ContentSizeFitter.FitMode.MinSize;
            confit.verticalFit = ContentSizeFitter.FitMode.MinSize;
        }

        Background = obj;
        _opacitySyncer = Background.GetOrAddComponent<HudBackgroundOpacitySyncer>();
        _opacitySyncer.ForceTransparent = !ShowBackground.Value;

        UpdatePivotNextFrame();
        SetScaleNextFrame();

        return obj;
    }
}
