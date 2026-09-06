using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// UI creator for EnhancedColor settings
/// </summary>
public class EnhancedColorUICreator : IConfigurableUICreator<Setting<EnhancedColor>> {
    public GameObject? CreateUI(Setting<EnhancedColor> element) {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "EnhancedColorSetting"));
        if (go == null) return null;
        go.AddComponent<EnhancedColorSettingController>().TargetSetting = element;
        return go;
    }
}
