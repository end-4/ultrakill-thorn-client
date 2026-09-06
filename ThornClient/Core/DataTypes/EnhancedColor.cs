using System;
using Newtonsoft.Json;
using NukeLib.Utils;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// Color but with effects.
/// </summary>
[JsonConverter(typeof(EnhancedColorJsonConverter))]
[ConfigurableUICreator(typeof(EnhancedColorUICreator))]
public class EnhancedColor : IEquatable<EnhancedColor> {
    /// <summary>
    /// The base color
    /// </summary>
    public Color BaseColor;

    /// <summary>
    /// The color mode
    /// </summary>
    public EnhancedColorMode Mode;

    public float r => BaseColor.r;
    public float g => BaseColor.g;
    public float b => BaseColor.b;
    public float a => BaseColor.a;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="color">Base color</param>
    /// <param name="mode">Color mode</param>
    public EnhancedColor(Color color, EnhancedColorMode mode = EnhancedColorMode.Static) {
        BaseColor = color;
        Mode = mode;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="r">red</param>
    /// <param name="g">green</param>
    /// <param name="b">blue</param>
    /// <param name="a">alpha</param>
    /// <param name="mode">Color mode</param>
    public EnhancedColor(float r, float g, float b, float a = 1f, EnhancedColorMode mode = EnhancedColorMode.Static) {
        BaseColor = new Color(r, g, b, a);
        Mode = mode;
    }

    /// <summary>
    /// Gets the current color, taking the mode and other factors into account if applicable
    /// </summary>
    /// <returns>The color</returns>
    public Color GetCurrentColor() {
        switch (Mode) {
            case EnhancedColorMode.Static:
                return BaseColor;
            case EnhancedColorMode.HuePulse:
                var globalHue = ColorManager.GlobalHue;
                BaseColor.RGBToOKLCH(out var l, out var c, out var _);
                return ColorUtils.OKLCHToRGB(l, c, globalHue, BaseColor.a);
            default:
                return BaseColor;
        }
    }

    // Equalities ///////////////////////////
    // RE-GENERATE THESE WHEN CHANGING/ADDING FIELDS
    public bool Equals(EnhancedColor? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return BaseColor.Equals(other.BaseColor) && Mode == other.Mode;
    }

    public override bool Equals(object? obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EnhancedColor)obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(BaseColor, (int)Mode);
    }
}

/// <summary>
/// Color mode
/// </summary>
public enum EnhancedColorMode {
    /// <summary>
    /// Makes it a normal Color essentially
    /// </summary>
    Static = 0,

    /// <summary>
    /// All-around hue pulse using OKLCH (HSL/HSV pulse look ass)
    /// </summary>
    HuePulse = 1,
}
