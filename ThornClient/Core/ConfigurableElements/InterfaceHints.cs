using System;
using System.Collections.Generic;
using System.Linq;
using NukeLib.Utils;

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
    /// When true forces bool settings to use a checkmark instead of a switch like [x] instead of ( o)
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

    /// <summary>
    /// Construct a range hint
    /// </summary>
    /// <param name="from">From what</param>
    /// <param name="to">To what</param>
    /// <returns>The interface hint with (only) the range</returns>
    public static InterfaceHints RangeHint(float from = 0f, float to = 1f) {
        return new InterfaceHints {
            Range = Tuple.Create(from, to)
        };
    }

    /// <summary>
    /// Generates enum display name substitutions formatted in Sentence case.
    /// </summary>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <returns>An InterfaceHints record with auto-generated Sentence case enum substitutions</returns>
    public static InterfaceHints SentenceCaseEnumSubstitutions<TEnum>() where TEnum : struct, Enum {
        var enumType = typeof(TEnum);
        if (enumType == null) throw new ArgumentNullException(nameof(enumType));
        if (!enumType.IsEnum) throw new ArgumentException("Provided type must be an enum.", nameof(enumType));
        return new InterfaceHints {
            EnumSubstitutions = Enum.GetNames(enumType)
                .ToDictionary(name => name, name => name.ToSentenceCase())
        };
    }

    /// <summary>
    /// Generates enum display name substitutions formatted in Title Case.
    /// </summary>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <returns>An InterfaceHints record with auto-generated Title Case enum substitutions</returns>
    public static InterfaceHints TitleCaseEnumSubstitutions<TEnum>() where TEnum : struct, Enum {
        var enumType = typeof(TEnum);
        if (enumType == null) throw new ArgumentNullException(nameof(enumType));
        if (!enumType.IsEnum) throw new ArgumentException("Provided type must be an enum.", nameof(enumType));

        return new InterfaceHints {
            EnumSubstitutions = Enum.GetNames(enumType)
                .ToDictionary(name => name, name => name.ToTitleCase())
        };
    }
}
