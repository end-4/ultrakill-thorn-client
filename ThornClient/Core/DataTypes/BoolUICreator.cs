using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// UI creator for bool settings
/// </summary>
public class BoolUICreator : IConfigurableUICreator<Setting<bool>> {
    public GameObject? CreateUI(Setting<bool> element) {
        if (element.Hints?.BoolPreferCheckmark == true) {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "BoolSetting"));
            if (go == null) return null;
            go.AddComponent<BoolSettingController>().TargetSetting = element;
            return go;
        } else {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "SwitchBoolSetting"));
            if (go == null) return null;
            go.AddComponent<SwitchBoolSettingController>().TargetSetting = element;
            return go;
        }
    }
}
