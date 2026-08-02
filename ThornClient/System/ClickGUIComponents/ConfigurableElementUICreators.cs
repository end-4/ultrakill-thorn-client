using System;
using System.Collections.Generic;
using System.Linq;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;

using ThornClient.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.System.ClickGUIComponents;

public static class ConfigurableElementUICreators {
    public static readonly Dictionary<SettingType, Func<Setting, Transform, GameObject>> SettingUICreators = new() {
        [SettingType.Bind] = (setting, parent) => {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "KeybindSetting"), parent);
            go.AddComponent<KeybindSettingController>().TargetSetting = (Setting<Keybind>)setting;
            return go;
        },
        [SettingType.Bool] = (setting, parent) => {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "BoolSetting"), parent);
            go.AddComponent<BoolSettingController>().TargetSetting = (Setting<bool>)setting;
            return go;
        },
        [SettingType.Color] = (setting, parent) => {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ColorSetting"), parent);
            go.AddComponent<ColorSettingController>().TargetSetting = (Setting<Color>)setting;
            return go;
        },
        [SettingType.EnemyList] = (setting, parent) => {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "EnemyListSetting"), parent);
            go.AddComponent<EnemyListSettingController>().TargetSetting = (Setting<EnemyList>)setting;
            return go;
        },
        [SettingType.Enum] = (setting, parent) => {
            var enumType = setting.GetValue().GetType();
            int totalChars = Enum.GetNames(enumType).Sum(name => name.Length);
            if (setting.Hints?.EnumPreferButtonGroup == true ||
                (totalChars <= 20 && setting.Hints?.EnumPreferButtonGroup != false)) {
                var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ConnectedButtonGroupSetting"), parent);
                go.AddComponent<ConnectedButtonGroupSettingController>().TargetSetting = setting;
                return go;
            } else {
                var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "DropdownSetting"), parent);
                go.AddComponent<DropdownSettingController>().TargetSetting = setting;
                return go;
            }
        },
        [SettingType.Float] = (setting, parent) => {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "NumberSetting"), parent);
            go.AddComponent<FloatSettingController>().TargetSetting = (Setting<float>)setting;
            return go;
        },
        [SettingType.Int] = (setting, parent) => {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "NumberSetting"), parent);
            go.AddComponent<IntSettingController>().TargetSetting = (Setting<int>)setting;
            return go;
        },
        [SettingType.Text] = (setting, parent) => {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "TextSetting"), parent);
            go.AddComponent<TextSettingController>().TargetSetting = (Setting<string>)setting;
            return go;
        }
    };

    public static readonly Dictionary<Type, Func<IConfigurableElement, Transform, GameObject>> MenuUICreators = new() {
        [typeof(ConfigButtonRow)] = (configurableElement, parent) => {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ButtonRowSetting"), parent);
            go.AddComponent<ButtonRowSettingController>().TargetButtonRow = (ConfigButtonRow)configurableElement;
            return go;
        },
        [typeof(SettingGroup)] = (configurableElement, parent) => {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "WindowedGroupSetting"), parent);
            go.AddComponent<WindowedGroupSettingController>().TargetGroup = (SettingGroup)configurableElement;
            return go;
        },
        [typeof(ConfigHeader)] = (configurableElement, parent) => {
            var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "HeaderSetting"), parent);
            go.AddComponent<HeaderSettingController>().TargetElement = (ConfigHeader)configurableElement;
            return go;
        },
    };
}
