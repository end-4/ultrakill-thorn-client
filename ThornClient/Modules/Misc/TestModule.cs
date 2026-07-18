using System;
using System.Collections.Generic;
using Notiffy.API;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.Core;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Misc;

public class TestModule : Module {
    public Setting<bool> Bulul { get; }
    public Setting<int> Inoue { get; }
    public Setting<float> Flatorte { get; }
    public Setting<string> Tung { get; }
    public Setting<Color> Carbonara { get; }

    public TestModule() : base("thorn.test", "Test module", "For UI debugging. This should be removed before release.",
        ModuleCategory.Misc) {
        Bulul = RegisterSetting("bulul", "Bulul",
            "Either \"Brother that is the fakest statement I've ever seen\" or \"holy SHIT\"", false);
        Inoue = RegisterSetting("inoue", "Takina Inoue-chan", "int field!!", 2);
        Flatorte = RegisterSetting("flatorte", "Flatorte-chan", "float field!!", 4f);
        Tung = RegisterSetting("tung", "Here bro have a tung tung", "text field!!", "sahur");
        Carbonara = RegisterSetting("carbonara", "Carbonara color", "Color field description", new Color(219, 209, 180, 88));
    }

    protected override void OnEnable() {
        NotificationSystem.NotifySend("Thorn::Debug", "Enabled");
    }

    protected override void OnDisable() {
        NotificationSystem.NotifySend("Thorn::Debug", "Disabled");
    }
}
