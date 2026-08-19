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
    /// <summary>
    /// X position of the module
    /// </summary>
    [JsonIgnore]
    public Setting<float> PositionX { get; }

    /// <summary>
    /// Y position of the module
    /// </summary>
    [JsonIgnore]
    public Setting<float> PositionY { get; }

    /// <summary>
    /// Surface to draw the module on
    /// </summary>
    [JsonIgnore]
    public Setting<HudSurface> Surface { get; }

    /// <summary>
    /// X pivot of the module
    /// </summary>
    [JsonIgnore]
    public Setting<float> PivotX { get; }

    /// <summary>
    /// Y pivot of the module
    /// </summary>
    [JsonIgnore]
    public Setting<float> PivotY { get; }

    /// <summary>
    /// Default local position for each surface
    /// </summary>
    public static Dictionary<HudSurface, Vector3> DefaultLocalPosition = new Dictionary<HudSurface, Vector3> {
        { HudSurface.Left, new Vector3(-484f, 344f, 45f) },
        { HudSurface.Right, new Vector3(-232f, -288, 1.0f) },
        { HudSurface.Overlay, new Vector3(0, 0, 0) }
    };

    /// <summary>
    /// Default scale for each surface
    /// </summary>
    public static Dictionary<HudSurface, Vector3> DefaultScale = new Dictionary<HudSurface, Vector3> {
        { HudSurface.Left, new Vector3(4, 4, 2) },
        { HudSurface.Right, new Vector3(1, 1, 1) },
        { HudSurface.Overlay, new Vector3(1, 1, 1) },
    };

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guid">The globally unique identifier. It's recommended to follow a PROVIDER.NAME format, such as thorn.input or crossover.config</param>
    /// <param name="name">The friendly name</param>
    /// <param name="description">The description</param>
    /// <param name="defaultPositionX">Default X position</param>
    /// <param name="defaultPositionY">Default Y position</param>
    /// <param name="defaultPivotX">Default X pivot</param>
    /// <param name="defaultPivotY">Default Y pivot</param>
    protected HudModule(string guid, string name, string description, float defaultPositionX = 0,
        float defaultPositionY = 0, float defaultPivotX = 0, float defaultPivotY = 1)
        : base(guid, name, description, ModuleCategory.Hud, hasToggling: true) {
        PositionX = CreateSetting("positionX", "Position X", "Horizontal position relative to the origin",
            defaultPositionX);
        PositionY = CreateSetting("positionY", "Position Y", "Vertical position relative to the origin",
            defaultPositionY);
        Surface = CreateSetting("surface", "Surface",
            "The surface this component is on. Left is gun panel, Right is style panel, Overlay is normal HUD",
            HudSurface.Overlay);
        PivotX = CreateSetting("pivotX", "Pivot X", "X (0-1) of origin point that the element expands around",
            defaultPivotX);
        PivotY = CreateSetting("pivotY", "Pivot Y", "Y (0-1) of origin point that the element expands around",
            defaultPivotY);

        Surface.OnValueChanged += ResetPositionIfNeeded;
        var hideHint = new InterfaceHints {
            Hidden = true,
        };

        PositionX.Hints = hideHint;
        PositionY.Hints = hideHint;
        PivotX.Hints = hideHint;
        PivotY.Hints = hideHint;
    }

    /// <summary>
    /// The UI element
    /// </summary>
    public GameObject? UIElement => Wrapper;

    protected RectTransform? OverlayRect;

    /// <summary>
    /// The wrapper object
    /// </summary>
    protected GameObject? Wrapper { get; private set; }

    /// <summary>
    /// The content object
    /// </summary>
    protected GameObject? Content;

    /// <summary>
    /// The wrapper rectangle
    /// </summary>
    protected RectTransform? WrapperRect;

    /// <summary>
    /// The content rectangle
    /// </summary>
    protected RectTransform? ContentRect;

    private void InitializeIfNeeded() {
        if (!IsEnabled || SceneHelper.CurrentScene == null)
            return; // Lazy loading and don't create stuff when not possible
        if (Wrapper == null) {
            Wrapper = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "Wrapper"));
            WrapperRect = Wrapper.transform as RectTransform;
            var eleCon = Wrapper.GetOrAddComponent<HudElementController>();
            eleCon.hudModule = this;
            Wrapper.SetActive(IsEnabled);
            OverlayRect = Wrapper.FindRecursive("Overlay")?.transform as RectTransform;
        }

        if (Content == null) {
            Content = CreateHudObject();
            ContentRect = Content.transform as RectTransform;
        }

        if (WrapperRect == null || ContentRect == null) {
            return;
        }

        Content.transform.SetParent(Wrapper.transform, false);
        // Plugin.Log.LogInfo($"{this.GetType().Name} created stuff, parenting");
        Reparent(Surface.Value);
        WrapperRect.sizeDelta = new Vector2(ContentRect.sizeDelta.x, ContentRect.sizeDelta.y);
        UpdateOverlay();
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

    protected virtual void OnHudModuleEnable() {
    }

    protected virtual void OnHudModuleDisable() {
    }

    /// <summary>
    /// Stuff to run when the module is enabled. If you override, make sure to run base.OnEnable() for proper UI creation.
    /// </summary>
    protected sealed override void OnEnable() {
        HudManager.ReadyForScene += InitializeIfNeeded;
        InitializeIfNeeded();
        if (Wrapper != null) {
            Wrapper.SetActive(true);
        }

        OnHudModuleEnable();
    }

    /// <summary>
    /// Stuff to run when the module is disabled
    /// </summary>
    protected sealed override void OnDisable() {
        OnHudModuleDisable();
        if (Wrapper == null) return;
        Wrapper.SetActive(false);
        HudManager.ReadyForScene -= InitializeIfNeeded;
    }

    /// <summary>
    /// Reparents the hud element to the new surface
    /// </summary>
    /// <param name="newSurface">The new surface</param>
    public void Reparent(HudSurface newSurface) {
        if (WrapperRect == null || !HudManager.GetSurface(newSurface, out var surfaceGo) || surfaceGo == null) return;

        // Plugin.Log.LogInfo($"{GetType().Name} reparenting to {newSurface}");
        WrapperRect.SetParent(surfaceGo.transform, false);
        WrapperRect.localScale = DefaultScale[newSurface];

        // Allow showing over fist
        WrapperRect.gameObject.SetLayerRecursive(LayerMask.NameToLayer("AlwaysOnTop"));
        var hudMaterial = Addressables.LoadAssetAsync<Material>("Assets/Materials/HUDMaterial.mat").WaitForCompletion();
        WrapperRect.gameObject.SetImageMaterialRecursive(hudMaterial);
        var hudTextMaterial = Addressables.LoadAssetAsync<Material>("Assets/Fonts/VCR_OSD_MONO Overlay.mat")
            .WaitForCompletion();
        WrapperRect.gameObject.SetTextMaterialRecursive(hudTextMaterial);
    }

    public void UpdateOverlay() {
        if (OverlayRect == null || ContentRect == null || WrapperRect == null) return;
        OverlayRect.pivot = ContentRect.pivot;
        OverlayRect.sizeDelta = ContentRect.sizeDelta;
        OverlayRect.localPosition = ContentRect.localPosition;
    }

    /// <summary>
    /// Method to create the GameObject for the HUD. Please also attach any controller components to it if needed.
    /// </summary>
    /// <returns>The GameObject</returns>
    protected abstract GameObject CreateHudObject();
}
