using System;

namespace ThornClient.Core;

public record InterfaceHints {
    public Tuple<float, float>? Range;
    public int? Decimals;
    public bool? EnumPreferButtonGroup;
    public bool? Hidden;
}
