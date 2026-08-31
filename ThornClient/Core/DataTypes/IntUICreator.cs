using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// UI creator for int settings
/// </summary>
public class IntUICreator : IConfigurableUICreator<Setting<int>> {
    public GameObject? CreateUI(Setting<int> element) {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "NumberSetting"));
        if (go == null) return null;
        go.AddComponent<IntSettingController>().TargetSetting = element;
        return go;
    }
}
