using System;
using Notiffy.API;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;

namespace ThornClient.Modules.Misc;

/// <summary>
/// Module for testing
/// </summary>
public class TestModule : Module {
    public enum NonEmulatorYuzu {
        Yoshino,
        Mako,
        Lena,
        Murasame,
        Roka,
        Koharu
    }

    public enum Baka {
        Miku,
        Teto,
        Neru
    }

    public Setting<bool> Bulul { get; }
    public Setting<int> Inoue { get; }
    public Setting<float> Flatorte { get; }
    public Setting<string> Tung { get; }
    public Setting<Color> Carbonara { get; }
    public Setting<NonEmulatorYuzu> Cutie { get; }
    public Setting<Baka> FavouriteBaka { get; }
    public Setting<EnemyList> Enemiez { get; }
    public Setting<float> Slippery { get; }
    public SettingGroup NestedGroup { get; }
    public SettingGroup DoubleNestedGroup { get; }

    public override string[] Tags => ["system", "debug", "developer"];

    public TestModule() : base("thorn.test", "Test module", "For UI debugging. This should be removed before release.",
        ModuleCategory.Misc) {
        Bulul = CreateSetting("boolField", "Bool setting",
            "Either \"Brother that is the fakest statement I've ever seen\" or \"holy SHIT\"", false);
        Inoue = CreateSetting("intField", "Int", "int field!!", 2);
        Flatorte = CreateSetting("floatField", "Floatie", "float field!!", 4f);
        Tung = CreateSetting("textField", "Yap field", "Here bro have a tung tung", "sahur");
        Carbonara = CreateSetting("colorField", "Some color!", "Color field description",
            new Color(0.86f, 0.82f, 0.71f, 1f));
        Cutie = CreateSetting("favouriteCutie", "Favorite cutie", "h", NonEmulatorYuzu.Mako);

        CreateHeader("beyondBasics", "Beyond basics");
        CreateHeader("subcat", "Subcategory", headerType: HeaderType.H2);
        NestedGroup = CreateGroup("nestedGroup", "Nested group", "Just a test");
        FavouriteBaka = CreateSetting("favouriteBaka", "Favourite baka", "popipo popipo", Baka.Miku, NestedGroup);
        DoubleNestedGroup = CreateGroup("nestederGroup", "Nestier group", "Nesty testy", NestedGroup);
        CreateHeader("subcat2", "Subcategory 2", headerType: HeaderType.H2);
        Enemiez = CreateSetting("enemiez", "Funny monsters field", "spooky scary skeleton", new EnemyList(), NestedGroup);
        Slippery = CreateSetting("slippery", "Slider field", "pls slide", 0.7f, DoubleNestedGroup);

        Slippery.Hints = new InterfaceHints {
            Range = Tuple.Create<float, float>(0, 1),
            Decimals = 2,
        };

        var chemistry = CreateButtonRow("btnRow", "Button Row Name", "Eyyyyy",
            ["Helium", "Nitrogen", "Tantalum", "Iodine"]);
        chemistry.OnClick += x => {
            NotificationSystem.NotifySend("Thorn::Debug", $"Clicked button numba {x}");
        };
    }

    protected override void OnEnable() {
        NotificationSystem.NotifySend("Thorn::Debug", "Enabled");
    }

    protected override void OnDisable() {
        NotificationSystem.NotifySend("Thorn::Debug", "Disabled");
    }
}
