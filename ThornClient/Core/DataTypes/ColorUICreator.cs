using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// UI creator for color settings
/// </summary>
public class ColorUICreator : IConfigurableUICreator<Setting<Color>> {
    public GameObject? CreateUI(Setting<Color> element) {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ColorSetting"));
        if (go == null) return null;
        go.AddComponent<ColorSettingController>().TargetSetting = element;
        return go;
    }
}
