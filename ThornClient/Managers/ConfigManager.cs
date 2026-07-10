using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThornClient.Core;

namespace ThornClient.Managers;

public static class ConfigManager {
    private static readonly string ConfigFolder = Path.Combine(Paths.ConfigPath, "ThornClient", "Default");
    private static FileSystemWatcher? _watcher;
    private static DateTime _lastRead = DateTime.MinValue;
    public static bool IsSyncing { get; private set; }

    private class ModuleConfigDataTransferObject {
        public bool IsEnabled { get; set; }
        public Dictionary<string, object> Settings { get; set; } = [];
    }

    private static string ConfigPathForName(string name) => Path.Combine(ConfigFolder, $"{name}.json");

    /// <summary>
    /// Saves a Configurable's settings to disk
    /// </summary>
    public static void SaveConfig(Configurable configurable) {
        if (IsSyncing) return;
        IsSyncing = true;

        try {
            if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);

            var dto = new ModuleConfigDataTransferObject {
                IsEnabled = configurable.IsEnabled
            };

            // Map all explicitly registered settings to clean string keys
            foreach (var setting in configurable.Settings) {
                dto.Settings[setting.Name] = setting.GetValue();
            }

            string jsonString = JsonConvert.SerializeObject(dto, Formatting.Indented);
            string filePath = ConfigPathForName(configurable.Name);

            File.WriteAllText(filePath, jsonString);
        } catch (Exception e) {
            Plugin.Log.LogError($"[ConfigManager] Failed to save {configurable.Name}: {e}");
        } finally {
            IsSyncing = false;
        }
    }

    /// <summary>
    /// Loads a Configurable's settings from disk
    /// </summary>
    public static void LoadConfig(Configurable configurable) {
        if (IsSyncing) return;
        IsSyncing = true;
        string filePath = ConfigPathForName(configurable.Name);
        if (!File.Exists(filePath)) return;

        try {
            string jsonString = File.ReadAllText(filePath);
            var dto = JsonConvert.DeserializeObject<ModuleConfigDataTransferObject>(jsonString);
            if (dto == null) return;

            if (dto.IsEnabled != configurable.IsEnabled) configurable.Toggle();
            Plugin.Log.LogInfo($"[ConfigManager] Reloading settings for feature {configurable.Name}");
            foreach (var setting in configurable.Settings) {
                if (dto.Settings.TryGetValue(setting.Name, out var savedValue)) {
                    if (savedValue != null) {
                        if (savedValue is JToken token) {
                            object primitiveValue = token.ToObject(typeof(object));
                            if (primitiveValue != null) {
                                setting.SetValue(primitiveValue);
                            }
                        } else {
                            // Fallback safe assignment
                            setting.SetValue(savedValue);
                        }
                    }
                }
            }
        } catch (Exception e) {
            Plugin.Log.LogError($"[ConfigManager] Failed to load {configurable.Name}: {e}");
        } finally {
            IsSyncing = false;
        }
    }

    /// <summary>
    /// Saves all settings
    /// </summary>
    public static void SaveAll() {
        foreach (var module in ModuleManager.Modules) {
            SaveConfig(module);
        }

        Plugin.Log.LogInfo("[ConfigManager] Saved all module settings");
    }

    /// <summary>
    /// Loads all settings
    /// </summary>
    public static void LoadAll() {
        foreach (var module in ModuleManager.Modules) {
            LoadConfig(module);
        }

        Plugin.Log.LogInfo("[ConfigManager] Loaded all module settings");
    }

    /// <summary>
    /// Initializes a live background listener that updates variables on text save events
    /// </summary>
    public static void SetupFileWatcher() {
        if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);
        if (_watcher != null) return;

        _watcher = new FileSystemWatcher {
            Path = ConfigFolder,
            Filter = "*.json",
            NotifyFilter = NotifyFilters.LastWrite
        };

        // Hook the changed event
        _watcher.Changed += OnConfigFileChanged;
        _watcher.EnableRaisingEvents = true;

        Plugin.Log.LogInfo("[ConfigManager] Background configuration hot-reload watcher is active.");
    }

    private static void OnConfigFileChanged(object sender, FileSystemEventArgs e) {
        // Debounce
        DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);
        if (lastWriteTime - _lastRead < TimeSpan.FromMilliseconds(100)) return;
        _lastRead = lastWriteTime;

        var moduleName = Path.GetFileNameWithoutExtension(e.Name);
        var targetModule = ModuleManager.GetByName(moduleName);

        if (targetModule != null) {
            LoadConfig(targetModule);
        }
    }
}
