using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Core.ConfigurableElements;

/// <summary>
/// UI creator for config button rows
/// </summary>
public class ConfigButtonRowUICreator : IConfigurableUICreator<ConfigButtonRow> {
    public GameObject? CreateUI(ConfigButtonRow element) {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ButtonRowSetting"));
        if (go == null) return null;
        go.AddComponent<ButtonRowSettingController>().TargetButtonRow = element;
        return go;
    }
}
