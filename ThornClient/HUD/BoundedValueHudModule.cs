using System;
using System.Collections.Generic;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.UI;
using ThornClient.HUD.HUDComponents;
using ThornClient.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.HUD;

/// <summary>
/// HUD element with a background,
/// dynamically sized based on content (you need min size/preferred size on your content element for that)
/// </summary>
public abstract class BoundedValueHudModule : FramedHudModule {
    /// <summary>
    /// The style of the indicator to use for this bounded value.
    /// </summary>
    public enum IndicatorStyle {
        /// <summary>
        /// The standard horizontal line style, much like vanilla health bar
        /// </summary>
        Progress = 0,

        /// <summary>
        /// The circular style, like Taberry's cybergrind wave progress indicator
        /// </summary>
        Circular = 1,

        /// <summary>
        /// The straight line style but vertical
        /// </summary>
        VerticalProgress = 2,

        /// <summary>
        /// Circle like vanilla crosshair HUD
        /// </summary>
        CrosshairCircle = 3,
    };

    /// <summary>
    /// The name to display on the HUD (not the ClickGUI)
    /// </summary>
    public string DisplayName {
        get;
        set {
            if (field == value) return;
            field = value;
            NameChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// The icon to display on the HUD (not the ClickGUI)
    /// </summary>
    public Sprite DisplayIcon {
        get;
        set {
            if (field == value) return;
            field = value;
            IconChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Current value (raw, not normalized)
    /// </summary>
    public float Value {
        get;
        set {
            if (Mathf.Approximately(field, value)) return;
            field = value;
            ValueChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Max value (raw, not normalized)
    /// </summary>
    public float Bound {
        get;
        set {
            if (Mathf.Approximately(field, value)) return;
            field = value;
            BoundChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Reduction value from the max value, for temporary limits like hard damage on HP.
    /// </summary>
    public float BoundReduction {
        get;
        set {
            if (Mathf.Approximately(field, value)) return;
            field = value;
            SoftBoundChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Number of digits after the decimal point to display.
    /// </summary>
    public int DecimalPlaces {
        get;
        set {
            if (field == value) return;
            field = value;
            DecimalPlacesChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Event fired when the DisplayName changes.
    /// </summary>
    public event Action<string> NameChanged;

    /// <summary>
    /// Event fired when the DisplayIcon changes.
    /// </summary>
    public event Action<Sprite> IconChanged;

    /// <summary>
    /// Event fired when the Value changes.
    /// </summary>
    public event Action<float> ValueChanged;

    /// <summary>
    /// Event fired when the Bound changes.
    /// </summary>
    public event Action<float> BoundChanged;

    /// <summary>
    /// Event fired when the SoftBound changes.
    /// </summary>
    public event Action<float> SoftBoundChanged;

    /// <summary>
    /// Event fired when the DecimalPlaces changes.
    /// </summary>
    public event Action<int> DecimalPlacesChanged;

    public Setting<IndicatorStyle> Style;

    public Setting<bool> ShowName;
    public Setting<Color> ValueColor;

    public Setting<float> ProgressLength;
    public Setting<bool> ProgressShowIcon;
    public Setting<bool> ProgressShowNumbers;

    public Setting<float> VerticalProgressLength;
    public Setting<bool> VerticalProgressShowIcon;

    public Setting<float> CrosshairCircleAngleFillPercentage;
    public Setting<float> CrosshairCircleStartAnglePercentage;
    // public Setting<float> CrosshairCircleDiameter;

    /// <summary>
    /// The color of the soft bound fill element of the indicator.
    /// </summary>
    public Setting<Color> SoftBoundColor;

    public SettingGroup ColorGroup;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guid">The globally unique identifier for the module, such as thorn.healthHud</param>
    /// <param name="name">The name of the module (shown in the ClickGUI)</param>
    /// <param name="description">The description of the module</param>
    /// <param name="bound">The max value</param>
    /// <param name="displayName">The name that shown on the HUD element</param>
    /// <param name="displayIcon">The icon shown on the HUD element</param>
    /// <param name="decimalPlaces">The decimal places to show on the value text</param>
    public BoundedValueHudModule(string guid, string name, string description, float bound = 1, string displayName = "",
        Sprite? displayIcon = null, int decimalPlaces = 1, Color? defaultValueColor = null,
        Color? defaultSoftBoundColor = null) : base(guid,
        name, description
    ) {
        // Settings
        Style = CreateSetting("indicatorStyle", "Indicator style", "The style to present the value",
            IndicatorStyle.Progress);
        Style.Hints = new InterfaceHints {
            EnumSubstitutions = new Dictionary<string, string> {
                ["VerticalProgress"] = "Vertical Progress",
                ["CrosshairCircle"] = "Crosshair Circle",
            }
        };
        ShowName = CreateSetting("showName", "Show name", "The name of the value", true);
        ColorGroup = CreateGroup("colorGroup", "Colors", "Colors used on the indicator");
        ValueColor = CreateSetting("valueColor", "Value color", "The color of the value",
            defaultValueColor ?? new Color(0.098f, 0.624f, 0.525f), ColorGroup);
        SoftBoundColor = CreateSetting("softBoundColor", "Soft Bound color",
            "The color of the soft bound, for example HP hard damage",
            defaultSoftBoundColor ?? new Color(1f, 1f, 1f, 0.36f), ColorGroup);
        CreateHeader("stylesHeader", "Style-specific settings");
        var progressGroup = CreateGroup("styleProgress", "Progress", "Settings specific to the Progress style");
        ProgressLength = CreateSetting(
            "progressLength", "Length", "How long the bar should be",
            194f, progressGroup
        ); // 194 to avoid breaking change
        ProgressShowIcon = CreateSetting(
            "progressShowIcon", "Show icon", "Whether to show the icon",
            true, progressGroup
        );
        ProgressShowNumbers = CreateSetting(
            "progressShowNumbers", "Show numbers", "Whether to show numbers",
            true, progressGroup
        );
        var verticalProgressGroup = CreateGroup("styleVerticalProgress", "Vertical Progress",
            "Settings specific to the Vertical Progress style");
        VerticalProgressLength = CreateSetting(
            "verticalProgressLength", "Length", "How tall the bar should be",
            194f, verticalProgressGroup
        );
        VerticalProgressShowIcon = CreateSetting(
            "verticalProgressShowIcon", "Show icon", "Whether to show the icon",
            true, verticalProgressGroup
        );

        var crosshairCircleGroup = CreateGroup("styleCrosshairCircle", "Crosshair Circle",
            "Settings specific to the Vertical Progress style");
        CrosshairCircleAngleFillPercentage = CreateSetting(
            "crosshairCircleAngleFillPercentage",
            "Angle fill percentage (0-1)", "How much of the circle to circle around. Value in range [0, 1]",
            1f, crosshairCircleGroup
        );
        CrosshairCircleAngleFillPercentage.Hints = InterfaceHints.RangeHint();
        CrosshairCircleStartAnglePercentage = CreateSetting(
            "crosshairCircleStartAnglePercentage",
            "Start angle (0-1)", "From where to circle around. Value in range [0, 1]",
            0f, crosshairCircleGroup
        );
        CrosshairCircleStartAnglePercentage.Hints = InterfaceHints.RangeHint();
        // CrosshairCircleDiameter = CreateSetting(
        //     "crosshairCircleDiameter",
        //     "Diameter", "How big the circle is",
        //     42f, crosshairCircleGroup
        // );
        CreateHeader("others", "Other settings");

        // Hooks/setups
        ShowName.OnChanged += UnfuckLayouts;
        SceneUtils.SafeSceneLoadedDelayedNoParam += UnfuckLayouts;
        Style.OnValueChanged += SwitchStyle;
        DisplayName = displayName.Length > 0 ? displayName : name;
        DisplayIcon = displayIcon ?? Icon;
        Bound = bound;
        BoundReduction = 0;
        DecimalPlaces = decimalPlaces;
        SwitchStyle(Style.Value);
    }

    private static Dictionary<IndicatorStyle, string> _stylePaths = new() {
        [IndicatorStyle.Progress] = "ProgressStyle",
        [IndicatorStyle.Circular] = "CircularStyle",
        [IndicatorStyle.VerticalProgress] = "VerticalProgressStyle",
        [IndicatorStyle.CrosshairCircle] = "CrosshairCircleStyle",
    };

    private static Dictionary<IndicatorStyle, Func<GameObject, IBoundedValueController?>> _styleComponentFactories =
        new() {
            [IndicatorStyle.Progress] = obj => obj?.AddComponent<ProgressBoundedValueController>(),
            [IndicatorStyle.Circular] = obj => obj?.AddComponent<CircularBoundedValueController>(),
            [IndicatorStyle.VerticalProgress] = obj => obj?.AddComponent<VerticalProgressBoundedValueController>(),
            [IndicatorStyle.CrosshairCircle] = obj => obj?.AddComponent<CrosshairCircleBoundedValueController>(),
        };

    private readonly Dictionary<IndicatorStyle, IBoundedValueController?> _styleComps = new();
    private readonly Dictionary<IndicatorStyle, GameObject?> _styleGameObjects = new();
    private GameObject? _contentObject;

    /// <summary>
    /// Creates the content object that goes on the frame.
    /// </summary>
    /// <returns>The content object</returns>
    protected override GameObject CreateContentObject() {
        _contentObject = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "BoundedValueLayout"));
        // Styles
        foreach (var pair in _stylePaths) {
            var targetObj = _contentObject.FindRecursive(pair.Value);
            // Plugin.Log.LogInfo($"Found {targetObj} for {pair.Key.ToString()}");
            _styleGameObjects[pair.Key] = targetObj;
            var comp = _styleComponentFactories[pair.Key](targetObj);

            _styleComps[pair.Key] = comp;
            if (comp != null) comp.TargetModule = this;
        }

        SwitchStyle(Style.Value);

        return _contentObject;
    }

    private void SwitchStyle(IndicatorStyle style) {
        foreach (var pair in _styleGameObjects) {
            // Plugin.Log.LogInfo($"Checking {pair.Key.ToString()} -> {style == pair.Key}");
            pair.Value?.SetActive(style == pair.Key);
        }

        UnfuckLayouts();
    }

    private void UnfuckLayouts() {
        if (!SceneUtils.IsSafe() || !IsEnabled) return;
        if (_contentObject == null || Wrapper == null) return;
        _contentObject.UnfuckLayoutHack();
        var fitterComp = Wrapper.GetComponent<SingularScalableContentSizeFitter>();
        if (fitterComp != null) {
            fitterComp.enabled = false;
            ExecutionUtils.RunNextFrame(() => {
                if (fitterComp != null) fitterComp.enabled = true;
            });
        }
    }
}
