using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// UI creator for float settings
/// </summary>
public class FloatUICreator : IConfigurableUICreator<Setting<float>> {
    public GameObject? CreateUI(Setting<float> element) {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "NumberSetting"));
        if (go == null) return null;
        go.AddComponent<FloatSettingController>().TargetSetting = element;
        return go;
    }
}
