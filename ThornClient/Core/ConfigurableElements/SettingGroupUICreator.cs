using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Core.ConfigurableElements;

/// <summary>
/// UI creator for setting groups
/// </summary>
public class SettingGroupUICreator : IConfigurableUICreator<SettingGroup> {
    public GameObject? CreateUI(SettingGroup element) {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "WindowedGroupSetting"));
        if (go == null) return null;
        go.AddComponent<WindowedGroupSettingController>().TargetGroup = element;
        return go;
    }
}
