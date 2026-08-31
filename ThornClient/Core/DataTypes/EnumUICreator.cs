using System;
using System.Linq;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// UI creator for enum settings
/// </summary>
public class EnumUICreator : IConfigurableUICreator<Setting> {
    public GameObject? CreateUI(Setting element) {
        var enumType = element.GetValue().GetType();
        int totalChars = Enum.GetNames(enumType).Sum(name => name.Length);
        if (element.Hints?.EnumPreferButtonGroup == true ||
            (totalChars <= 20 && element.Hints?.EnumPreferButtonGroup != false)) {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey,
                "ConnectedButtonGroupSetting"));
            if (go == null) return null;
            go.AddComponent<ConnectedButtonGroupSettingController>().TargetSetting = element;
            return go;
        } else {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "DropdownSetting"));
            if (go == null) return null;
            go.AddComponent<WindowedEnumSettingController>().TargetSetting = element;
            return go;
        }
    }
}
