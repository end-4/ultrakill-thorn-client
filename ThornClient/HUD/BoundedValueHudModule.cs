using System;
using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.HUD.HUDComponents;
using ThornClient.Managers;
using ThornClient.System;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.HUD;

/// <summary>
/// HUD element with a background,
/// dynamically sized based on content (you need min size/preferred size on your content element for that)
/// </summary>
public abstract class BoundedValueHudModule : FramedHudModule {
    public enum IndicatorStyle {
        Progress = 0
    };

    public string DisplayName {
        get;
        set {
            if (field == value) return;
            field = value;
            NameChanged?.Invoke(value);
        }
    }

    public Sprite DisplayIcon {
        get;
        set {
            if (field == value) return;
            field = value;
            IconChanged?.Invoke(value);
        }
    }

    public float Value {
        get;
        set {
            if (Mathf.Approximately(field, value)) return;
            field = value;
            ValueChanged?.Invoke(value);
        }
    }

    public float Bound {
        get;
        set {
            if (Mathf.Approximately(field, value)) return;
            field = value;
            BoundChanged?.Invoke(value);
        }
    }

    public float SoftBound {
        get;
        set {
            if (Mathf.Approximately(field, value)) return;
            field = value;
            SoftBoundChanged?.Invoke(value);
        }
    }

    public int DecimalPlaces {
        get;
        set {
            if (field == value) return;
            field = value;
            DecimalPlacesChanged?.Invoke(value);
        }
    }

    public event Action<string> NameChanged;
    public event Action<Sprite> IconChanged;
    public event Action<float> ValueChanged;
    public event Action<float> BoundChanged;
    public event Action<float> SoftBoundChanged;
    public event Action<int> DecimalPlacesChanged;

    public Setting<IndicatorStyle> Style;
    public Setting<bool> ShowName;
    public Setting<Color> ValueColor;
    public Setting<Color> SoftBoundColor;

    public BoundedValueHudModule(string guid, string name, string description) : base(guid, name, description) {
        Style = CreateSetting("indicatorStyle", "Indicator Style", "The style to present the value", IndicatorStyle.Progress);
        ShowName = CreateSetting("showName", "Show Name", "The name of the value", true);
        ValueColor = CreateSetting("valueColor", "Value Color", "The color of the value", new Color(0.098f, 0.624f, 0.525f));
        SoftBoundColor = CreateSetting("softBoundColor", "Soft Bound Color", "The color of the soft bound, for example HP hard damage", new Color(1f, 1f, 1f, 0.36f));
        Style.OnValueChanged += SwitchStyle;
        SwitchStyle(Style.Value);
    }

    private static Dictionary<IndicatorStyle, string> _stylePaths = new() {
        [IndicatorStyle.Progress] = "ProgressStyle",
    };

    private static Dictionary<IndicatorStyle, IBoundedValueController?> _styleComps = new();
    private static Dictionary<IndicatorStyle, GameObject?> _styleGameObjects = new();

    protected override GameObject CreateContentObject() {
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
