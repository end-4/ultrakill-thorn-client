using System;
using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
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
    public BoundedValueHudModule(string guid, string name, string description, float bound = 1, string displayName = "",
        Sprite? displayIcon = null) : base(guid, name, description) {
        Style = CreateSetting("indicatorStyle", "Indicator Style", "The style to present the value",
            IndicatorStyle.Progress);
        ShowName = CreateSetting("showName", "Show Name", "The name of the value", true);
        ColorGroup = CreateGroup("colorGroup", "Color Group", "Colors used on the indicator");
        ValueColor = CreateSetting("valueColor", "Value Color", "The color of the value",
            new Color(0.098f, 0.624f, 0.525f), ColorGroup);
        SoftBoundColor = CreateSetting("softBoundColor", "Soft Bound Color",
            "The color of the soft bound, for example HP hard damage", new Color(1f, 1f, 1f, 0.36f), ColorGroup);
        Style.OnValueChanged += SwitchStyle;
        DisplayName = displayName.Length > 0 ? displayName : name;
        DisplayIcon = displayIcon ?? Icon;
        Bound = bound;
        BoundReduction = 0;
        SwitchStyle(Style.Value);
    }

    private static Dictionary<IndicatorStyle, string> _stylePaths = new() {
        [IndicatorStyle.Progress] = "ProgressStyle",
        [IndicatorStyle.Circular] = "CircularStyle",
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
            IBoundedValueController? comp = null;
            switch (pair.Key) {
                case IndicatorStyle.Progress:
                    comp = targetObj?.AddComponent<ProgressBoundedValueController>();
                    break;
                case IndicatorStyle.Circular:
                    comp = targetObj?.AddComponent<CircularBoundedValueController>();
                    break;
            }

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

        if (_contentObject != null) _contentObject.UnfuckLayoutHack();
    }
}
