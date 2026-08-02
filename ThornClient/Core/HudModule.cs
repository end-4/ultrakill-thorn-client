using System.Collections.Generic;
using Newtonsoft.Json;
using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using ThornClient.HUD.HUDComponents;
using ThornClient.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ThornClient.Core;

/// <summary>
/// The base class for HUD modules
/// </summary>
public abstract class HudModule : Module {
    [JsonIgnore] public Setting<float> PositionX { get; }
    [JsonIgnore] public Setting<float> PositionY { get; }
    [JsonIgnore] public Setting<HudSurface> Surface { get; }
    [JsonIgnore] public Setting<float> PivotX { get; }
    [JsonIgnore] public Setting<float> PivotY { get; }

    public static Dictionary<HudSurface, Vector3> DefaultLocalPosition = new Dictionary<HudSurface, Vector3> {
        { HudSurface.Left, new Vector3(-484f, 344f, 45f) },
        { HudSurface.Right, new Vector3(-232f, -288, 1.0f) },
        { HudSurface.Overlay, new Vector3(0, 0, 0) }
    };

    public static Dictionary<HudSurface, Vector3> DefaultScale = new Dictionary<HudSurface, Vector3> {
        { HudSurface.Left, new Vector3(4, 4, 2) },
        { HudSurface.Right, new Vector3(1, 1, 1) },
        { HudSurface.Overlay, new Vector3(1, 1, 1) },
    };

    protected HudModule(string guid, string name, string description, float defaultPositionX = 0,
        float defaultPositionY = 0, float defaultPivotX = 0, float defaultPivotY = 1)
        : base(guid, name, description, ModuleCategory.Hud, hasToggling: true) {
        PositionX = RegisterSetting("positionX", "Position X", "Horizontal position relative to the origin",
            defaultPositionX);
        PositionY = RegisterSetting("positionY", "Position Y", "Vertical position relative to the origin",
            defaultPositionY);
        Surface = RegisterSetting("surface", "Surface",
            "The surface this component is on. Left is gun panel, Right is style panel, Overlay is normal HUD",
            HudSurface.Overlay);
        PivotX = RegisterSetting("pivotX", "Pivot X", "X (0-1) of origin point that the element expands around",
            defaultPivotX);
        PivotY = RegisterSetting("pivotY", "Pivot Y", "Y (0-1) of origin point that the element expands around",
            defaultPivotY);

        HudManager.ReadyForScene += InitializeIfNeeded;
        Surface.OnValueChanged += ResetPositionIfNeeded;
        var hideHint = new InterfaceHints {
            Hidden = true,
        };

        PositionX.Hints = hideHint;
        PositionY.Hints = hideHint;
        PivotX.Hints = hideHint;
        PivotY.Hints = hideHint;
    }

    public GameObject? UIElement => _wrapper;

    protected GameObject? _wrapper;
    protected GameObject? _content;
    protected RectTransform? _wrapperRect;
    protected RectTransform? _contentRect;

    private void InitializeIfNeeded() {
        if (!IsEnabled) return; // Lazy loading
        if (_wrapper == null) {
            _wrapper = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "Wrapper"));
            _wrapperRect = _wrapper.transform as RectTransform;
            var eleCon = _wrapper.GetOrAddComponent<HudElementController>();
            eleCon.hudModule = this;
            _wrapper.SetActive(IsEnabled);
        }

        if (_content == null) {
            _content = CreateHudObject();
            _contentRect = _content.transform as RectTransform;
        }

        if (_wrapperRect == null || _contentRect == null) {
            return;
        }

        _content.transform.SetParent(_wrapper.transform, false);
        // Plugin.Log.LogInfo($"{this.GetType().Name} created stuff, parenting");
        Reparent(Surface.Value);
        _wrapperRect.sizeDelta = new Vector2(_contentRect.sizeDelta.x, _contentRect.sizeDelta.y);
    }

    private void ResetPositionIfNeeded(HudSurface surface) {
        // If it's initial config load don't reset.
        // Hack(-ish): undefined scene name = first config load
        // It *is* true, it's just weird
        // Plugin.Log.LogInfo($"hud module might reset pos in scene {SceneHelper.CurrentScene}");
        if (string.IsNullOrEmpty(SceneHelper.CurrentScene)) return;
        // Plugin.Log.LogInfo($"indeed");
        PositionX.Value = DefaultLocalPosition[surface].x;
        PositionY.Value = DefaultLocalPosition[surface].y;
    }

    protected override void OnEnable() {
        InitializeIfNeeded();
        if (_wrapper == null) return;
        _wrapper.SetActive(true);
    }

    protected override void OnDisable() {
        if (_wrapper == null) return;
        _wrapper.SetActive(false);
    }

    public void Reparent(HudSurface newSurface) {
        if (_wrapperRect == null || !HudManager.GetSurface(newSurface, out var surfaceGo) || surfaceGo == null) return;

        // Plugin.Log.LogInfo($"{GetType().Name} reparenting to {newSurface}");
        _wrapperRect.SetParent(surfaceGo.transform, false);
        _wrapperRect.localScale = DefaultScale[newSurface];

        // Allow showing over fist
        _wrapperRect.gameObject.SetLayerRecursive(LayerMask.NameToLayer("AlwaysOnTop"));
        var hudMaterial = Addressables.LoadAssetAsync<Material>("Assets/Materials/HUDMaterial.mat").WaitForCompletion();
        _wrapperRect.gameObject.SetImageMaterialRecursive(hudMaterial);
        var hudTextMaterial = Addressables.LoadAssetAsync<Material>("Assets/Fonts/VCR_OSD_MONO Overlay.mat").WaitForCompletion();
        _wrapperRect.gameObject.SetTextMaterialRecursive(hudTextMaterial);

    }

    /// <summary>
    /// Method to create the GameObject for the HUD. Please also attach any controller components to it.
    /// </summary>
    /// <returns>The GameObject</returns>
    protected abstract GameObject CreateHudObject();
}
