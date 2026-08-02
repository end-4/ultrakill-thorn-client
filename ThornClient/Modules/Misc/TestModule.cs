using System;
using System.Collections.Generic;
using Notiffy.API;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Misc;

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
        Bulul = RegisterSetting("bulul", "Bulul",
            "Either \"Brother that is the fakest statement I've ever seen\" or \"holy SHIT\"", false);
        Inoue = RegisterSetting("inoue", "Takina Inoue-chan", "int field!!", 2);
        Flatorte = RegisterSetting("flatorte", "Flatorte-chan", "float field!!", 4f);
        Tung = RegisterSetting("tung", "Here bro have a tung tung", "text field!!", "sahur");
        Carbonara = RegisterSetting("carbonara", "Carbonara color", "Color field description",
            new Color(0.86f, 0.82f, 0.71f, 1f));
        Cutie = RegisterSetting("favouriteCutie", "Favorite cutie", "h", NonEmulatorYuzu.Mako);

        RegisterHeader("beyondBasics", "Beyond basics");
        RegisterHeader("subcat", "Subcategory", headerType: HeaderType.H2);
        NestedGroup = CreateGroup("nestedGroup", "Nested group", "Just a test");
        FavouriteBaka = RegisterSetting("favouriteBaka", "Favourite baka", "popipo popipo", Baka.Miku, NestedGroup);
        DoubleNestedGroup = CreateGroup("nestederGroup", "Nestier group", "Nesty testy", NestedGroup);
        RegisterHeader("subcat2", "Subcategory two", headerType: HeaderType.H2);
        Enemiez = RegisterSetting("enemiez", "Funny monsters", "tooltip text!!", new EnemyList(), NestedGroup);
        Slippery = RegisterSetting("slippery", "Slipperie", "pls slide", 0.7f, DoubleNestedGroup);

        Slippery.Hints = new InterfaceHints {
            Range = Tuple.Create<float, float>(0, 1),
            Decimals = 2,
        };

        var kurwa = RegisterButtonRow("btnRow", "Button Row Name", "Eyyyyy",
            ["Helium", "Nitrogen", "Tantalum", "Iodine"]);
        kurwa.OnClick += (int x) => {
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
