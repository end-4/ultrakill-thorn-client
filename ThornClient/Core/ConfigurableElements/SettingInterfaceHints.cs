using System;
using System.Collections.Generic;

namespace ThornClient.Core.ConfigurableElements;

/// <summary>
/// Interface hints. Not applicable to every setting.
/// </summary>
public record InterfaceHints {
    /// <summary>
    /// The range. When specified adds a slider to int and float settings.
    /// </summary>
    public Tuple<float, float>? Range;

    /// <summary>
    /// The decimals to shown in the input field, relevant for float settings
    /// </summary>
    public int? Decimals;

    /// <summary>
    /// When true forces enum settings to use the Material 3-style button group instead of a dropdown.
    /// </summary>
    public bool? EnumPreferButtonGroup;

    /// <summary>
    /// When true forces bool settings to use a checkmark instead of a switch: [x] <- ( o)
    /// </summary>
    public bool? BoolPreferCheckmark;

    /// <summary>
    /// When true, the setting is hidden from the config menu. Use this for internal persistent states that the user shouldn't care about.
    /// </summary>
    public bool? Hidden;

    /// <summary>
    /// For setting display name for enum values.
    /// Keys must match the enum value names and values are the substitutions.
    /// </summary>
    public Dictionary<string, string>? EnumSubstitutions;

    /// <summary>
    /// For extra non-standard hints. To be used by custom UI elements when that becomes a thing
    /// </summary>
    public Dictionary<string, object>? ExtraHints;
}
