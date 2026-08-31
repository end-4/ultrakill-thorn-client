using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// UI creator for enemy list settings
/// </summary>
public class EnemyListUICreator : IConfigurableUICreator<Setting<EnemyList>> {
    public GameObject? CreateUI(Setting<EnemyList> element) {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "EnemyListSetting"));
        if (go == null) return null;
        go.AddComponent<EnemyListSettingController>().TargetSetting = element;
        return go;
    }
}
