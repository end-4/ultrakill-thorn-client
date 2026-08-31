using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// UI creator for string settings
/// </summary>
public class StringUICreator : IConfigurableUICreator<Setting<string>> {
    public GameObject? CreateUI(Setting<string> element) {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "TextSetting"));
        if (go == null) return null;
        go.AddComponent<TextSettingController>().TargetSetting = element;
        return go;
    }
}
