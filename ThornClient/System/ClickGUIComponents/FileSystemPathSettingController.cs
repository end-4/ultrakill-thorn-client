using System;
using System.IO;
using Fireman.Interface;
using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Abstract base controller for file system path setting UI
/// </summary>
public abstract class FileSystemPathSettingController<T> : MonoBehaviour where T : FileSystemPath {
    public Setting<T>? TargetSetting { get; set; }

    /// <summary>
    /// Whether the setting is for a folder
    /// </summary>
    protected abstract bool IsFolder { get; }

    /// <summary>
    /// Whether the setting allows multiple paths
    /// Does nothing for now
    /// </summary>
    protected virtual bool IsMulti => false;

    private ClickHandler? _clickHandler;
    private TextMeshProUGUI? _valText;

    private void Start() {
        _clickHandler = gameObject.AddComponent<ClickHandler>();
        _clickHandler.OnPress += Pick;

        var select = gameObject.FindRecursive("Value");
        var ico = select?.FindRecursive("TargetIcon")?.GetComponent<Image>();
        if (ico != null) ico.sprite = AssetManager.Get<Sprite>(ClickGUI.BundleKey, IsFolder ? "folder_clear" : "file");
        _valText = select?.FindRecursive("CurrentPath")?.GetComponent<TextMeshProUGUI>();

        if (TargetSetting != null) TargetSetting.OnChanged += UpdateValue;
        UpdateValue();
    }

    private void OnDestroy() {
        if (TargetSetting != null) TargetSetting.OnChanged -= UpdateValue;
        if (_clickHandler != null) _clickHandler.OnPress -= Pick;
    }

    private void Pick() {
        var fm = FileManager.CreatePicker(Path.GetDirectoryName(TargetSetting?.Value.Path),
            isSelectionFolder: IsFolder);
        fm.ItemsPicked += SetValue;
        var obj = fm.gameObject;
        var layoutElement = obj.GetOrAddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;
        var rt = obj.GetComponent<RectTransform>();
        ClickGUI.SpawnContent(obj);
        rt.sizeDelta = new Vector2(570, 380);
    }

    private void SetValue(string[] paths) {
        if (paths.Length < 1 || TargetSetting == null) return;
        TargetSetting.Value = (T)Activator.CreateInstance(typeof(T), paths[0]);
    }

    private void UpdateValue() {
        if (TargetSetting == null || _valText == null) return;
        var itemPath = IsFolder
            ? new DirectoryInfo(TargetSetting.Value.Path).FullName
            : new FileInfo(TargetSetting.Value.Path).FullName;
        var truncatedName = FishInitialTruncate(itemPath);
        _valText.SetText(truncatedName);
    }

    /// <summary>
    /// Truncates a path, preserving only the last two segments and the root, while others become their initials
    /// Additionally turns separators into slashes
    /// Example: C:\Users\end\Pictures\MindflayerPics\01.jpg -> C:/U/e/P/MindflayerPics/01.jpg
    /// </summary>
    /// <param name="path">The full file path</param>
    /// <returns>The truncated path</returns>
    private string FishInitialTruncate(string path) {
        if (string.IsNullOrEmpty(path)) return path;
        var isUnix = path.Contains('/');

        // If it's Unix path we split by slashes only
        char[] splitChars = isUnix ? ['/'] : ['\\', '/'];
        string[] segments = path.Split(splitChars, StringSplitOptions.None);

        // Truncate all except last two
        // We start with index 1 because on Windows we wanna keep the drive letter,
        //   and on Unix item 0 is an empty string anyway
        for (int i = 1; i < segments.Length - 2; i++) {
            if (!string.IsNullOrEmpty(segments[i])) {
                segments[i] = segments[i][0].ToString();
            }
        }

        // Note that normal slashes are still valid on Windows
        return string.Join("/", segments);
    }
}

/// <summary>
/// Controller for file path setting UI
/// </summary>
public class FilePathSettingController : FileSystemPathSettingController<FilePath> {
    /// <inheritdoc />
    protected override bool IsFolder => false;
}

/// <summary>
/// Controller for folder path setting UI
/// </summary>
public class FolderPathSettingController : FileSystemPathSettingController<FolderPath> {
    /// <inheritdoc />
    protected override bool IsFolder => true;
}
