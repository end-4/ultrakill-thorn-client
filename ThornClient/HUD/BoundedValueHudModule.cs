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
        Progress = 0
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
    /// Soft bound value (raw, not normalized). This is for temporary limits like hard damage on HP.
    /// </summary>
    public float SoftBound {
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

    // TODO make this^ work and add more option to hide stuff. Maybe via some comp to be put in ancestor that does syncing like the color one

    /// <summary>
    /// The color of the main value fill element of the indicator.
    /// </summary>
    public Setting<Color> ValueColor;

    /// <summary>
    /// The color of the soft bound fill element of the indicator.
    /// </summary>
    public Setting<Color> SoftBoundColor;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guid">The globally unique identifier for the module, such as thorn.healthHud</param>
    /// <param name="name">The name of the module (shown in the ClickGUI)</param>
    /// <param name="description">The description of the module</param>
    public BoundedValueHudModule(string guid, string name, string description) : base(guid, name, description) {
        Style = CreateSetting("indicatorStyle", "Indicator Style", "The style to present the value",
            IndicatorStyle.Progress);
        ValueColor = CreateSetting("valueColor", "Value Color", "The color of the value",
            new Color(0.098f, 0.624f, 0.525f));
        SoftBoundColor = CreateSetting("softBoundColor", "Soft Bound Color",
            "The color of the soft bound, for example HP hard damage", new Color(1f, 1f, 1f, 0.36f));
        Style.OnValueChanged += SwitchStyle;
        SwitchStyle(Style.Value);
    }

    private static Dictionary<IndicatorStyle, string> _stylePaths = new() {
        [IndicatorStyle.Progress] = "ProgressStyle",
    };

    private static Dictionary<IndicatorStyle, IBoundedValueController?> _styleComps = new();
    private static Dictionary<IndicatorStyle, GameObject?> _styleGameObjects = new();

    /// <summary>
    /// Creates the content object that goes on the frame.
    /// </summary>
    /// <returns>The content object</returns>
    protected sealed override GameObject CreateContentObject() {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "BoundedValueLayout"));
        // Styles
        foreach (var pair in _stylePaths) {
            var targetObj = go.FindRecursive(pair.Value);
            // Plugin.Log.LogInfo($"Found {targetObj} for {pair.Key.ToString()}");
            _styleGameObjects[pair.Key] = targetObj;
            switch (pair.Key) {
                case IndicatorStyle.Progress:
                    var comp = targetObj?.AddComponent<ProgressBoundedValueController>();
                    _styleComps[pair.Key] = comp;
                    if (comp != null) comp.TargetModule = this;
                    // Plugin.Log.LogInfo($"Added comp {_styleComps[pair.Key]}");
                    break;
            }
        }

        SwitchStyle(Style.Value);

        return go;
    }

    private void SwitchStyle(IndicatorStyle style) {
        foreach (var pair in _styleGameObjects) {
            // Plugin.Log.LogInfo($"Checking {pair.Key.ToString()} -> {style == pair.Key}");
            pair.Value?.SetActive(style == pair.Key);
        }
    }
}
