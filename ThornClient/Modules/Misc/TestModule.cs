using System;
using System.Collections.Generic;
using Notiffy.API;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;

namespace ThornClient.Modules.Misc;

/// <summary>
/// Module for testing
/// </summary>
internal abstract class TestModule : Module {
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

    public Setting<bool> BoolSetting { get; }
    public Setting<bool> BoolSettingCheck { get; }
    public Setting<int> IntSetting { get; }
    public Setting<float> FloatSetting { get; }
    public Setting<string> StringSetting { get; }
    public Setting<Color> ColorSetting { get; }
    public Setting<NonEmulatorYuzu> Cutie { get; }
    public Setting<Baka> FavouriteBaka { get; }
    public Setting<EnemyList> Enemiez { get; }
    public Setting<float> Slippery { get; }
    public SettingGroup NestedGroup { get; }
    public SettingGroup DoubleNestedGroup { get; }

    public override string[] Tags => ["system", "debug", "developer"];

    public TestModule() : base("thorn.test", "Test module", "For UI debugging. This should be removed before release.",
        ModuleCategory.Misc) {
        BoolSetting = CreateSetting("boolField", "Bool setting",
            "Either \"Brother that is the fakest statement I've ever seen\" or \"holy SHIT\"", false);
        BoolSettingCheck = CreateSetting("boolFieldCheck", "Checkmark bool setting",
            "Either \"Brother that is the fakest statement I've ever seen\" or \"holy SHIT\"", true);
        BoolSettingCheck.Hints =
            new InterfaceHints {
                BoolPreferCheckmark = true
            };
        IntSetting = CreateSetting("intField", "Int", "int field!!", 2);
        FloatSetting = CreateSetting("floatField", "Floatie", "float field!!", 4f);
        StringSetting = CreateSetting("textField", "Yap field", "Here bro have a tung tung", "sahur");
        ColorSetting = CreateSetting("colorField", "Some color!", "Color field description",
            new Color(0.86f, 0.82f, 0.71f, 1f));
        Cutie = CreateSetting("favouriteCutie", "Favorite cutie", "h", NonEmulatorYuzu.Mako);
        Cutie.Hints = new InterfaceHints {
            EnumSubstitutions = new Dictionary<string, string> {
                ["Yoshino"] = "White haired mikosan",
                ["Mako"] = "Very cool ninja",
                ["Murasame"] = "500yo kiddy sword",
                ["Roka"] = "Oneesan, I love frogs",
                ["Koharu"] = "Actual rori",
            }
        };

        CreateHeader("beyondBasics", "Beyond basics");
        CreateHeader("subcat", "Subcategory", headerType: HeaderType.H2);
        NestedGroup = CreateGroup("nestedGroup", "Nested group", "Just a test");
        FavouriteBaka = CreateSetting("favouriteBaka", "Favourite baka", "popipo popipo", Baka.Miku, NestedGroup);
        FavouriteBaka.Hints = new InterfaceHints {
            EnumSubstitutions = new Dictionary<string, string> {
                ["Teto"] = "Red one",
                ["Neru"] = "Yello one",
            }
        };
        DoubleNestedGroup = CreateGroup("nestederGroup", "Nestier group", "Nesty testy", NestedGroup);
        CreateHeader("subcat2", "Subcategory 2", headerType: HeaderType.H2);
        Enemiez = CreateSetting("enemiez", "Funny monsters field", "spooky scary skeleton", new EnemyList(),
            NestedGroup);
        Slippery = CreateSetting("slippery", "Slider field", "pls slide", 0.7f, DoubleNestedGroup);

        Slippery.Hints = new InterfaceHints {
            Range = Tuple.Create<float, float>(0, 1),
            Decimals = 2,
        };

        var chemistry = CreateButtonRow("btnRow", "Button Row Name", "Eyyyyy",
            ["Helium", "Nitrogen", "Tantalum", "Iodine"]);
        chemistry.OnClick += x => { NotificationSystem.NotifySend("Thorn::Debug", $"Clicked button numba {x}"); };

        CreateButtonRow("_", "Overflow", "Useless buttons just to test overflow scrolling", ["Foo", "Bar"]);
        CreateButtonRow("_", "Overflow", "Useless buttons just to test overflow scrolling", ["Five"]);
        CreateButtonRow("_", "Overflow", "Useless buttons just to test overflow scrolling", ["Five", "little"]);
        CreateButtonRow("_", "Overflow", "Useless buttons just to test overflow scrolling",
            ["Five", "little", "Chuddies"]);
        CreateButtonRow("_", "Overflow", "Useless buttons just to test overflow scrolling",
            ["Five", "little", "Chuddies", "jumping"]);
        CreateButtonRow("_", "Overflow", "Useless buttons just to test overflow scrolling",
            ["Five", "little", "Chuddies", "jumping", "on"]);
    }

    protected override void OnEnable() {
        NotificationSystem.NotifySend("Thorn::Debug", "Enabled");
    }

    protected override void OnDisable() {
        NotificationSystem.NotifySend("Thorn::Debug", "Disabled");
    }
}
