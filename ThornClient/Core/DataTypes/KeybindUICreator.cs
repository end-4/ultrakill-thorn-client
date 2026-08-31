using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// UI creator for Keybind settings
/// </summary>
public class KeybindUICreator : IConfigurableUICreator<Setting<Keybind>> {
    public GameObject? CreateUI(Setting<Keybind> element) {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "KeybindSetting"));
        if (go == null) return null;
        go.AddComponent<KeybindSettingController>().TargetSetting = element;
        return go;
    }
}
