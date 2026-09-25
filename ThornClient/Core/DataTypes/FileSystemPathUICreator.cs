using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// UI creator for EnhancedColor settings
/// </summary>
public abstract class FileSystemPathUICreator<TData> : IConfigurableUICreator<Setting<TData>> where TData : FileSystemPath {
    /// <summary>
    /// Creates the UI without a controller.
    /// </summary>
    /// <param name="element">Setting element</param>
    /// <returns>The GameObject of the UI without a controller</returns>
    protected GameObject? CreateFileSystemSettingUIWithoutController(Setting<TData> element, bool isFolder, bool multi) {
        var go = Object.Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "FileSystemPathSetting"));
        if (go == null) return null;
        return go;
    }

    /// <inheritdoc />
    public abstract GameObject? CreateUI(Setting<TData> element);
}

/// <summary>
/// UI creator for FilePath settings
/// </summary>
public class FilePathUICreator : FileSystemPathUICreator<FilePath> {
    /// <inheritdoc />
    public override GameObject? CreateUI(Setting<FilePath> element) {
        var obj = CreateFileSystemSettingUIWithoutController(element, false, false);
        var comp = obj.AddComponent<FilePathSettingController>();
        comp.TargetSetting = element;
        return obj;
    }
}

/// <summary>
/// UI creator for FilePath settings
/// </summary>
public class FolderPathUICreator : FileSystemPathUICreator<FolderPath> {
    /// <inheritdoc />
    public override GameObject? CreateUI(Setting<FolderPath> element) {
        var obj = CreateFileSystemSettingUIWithoutController(element, true, false);
        var comp = obj.AddComponent<FolderPathSettingController>();
        comp.TargetSetting = element;
        return obj;
    }
}


