using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Core.ConfigurableElements;

/// <summary>
/// UI creator for config headers
/// </summary>
public class ConfigHeaderUICreator : IConfigurableUICreator<ConfigHeader> {
    public GameObject? CreateUI(ConfigHeader element) {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "HeaderSetting"));
        if (go == null) return null;
        go.AddComponent<HeaderSettingController>().TargetElement = element;
        return go;
    }
}
