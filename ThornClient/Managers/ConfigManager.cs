using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThornClient.Core;
using ThornClient.Core.DataTypes;

namespace ThornClient.Managers;

public static class ConfigManager {
    private static readonly string ConfigFolder = Path.Combine(Paths.ConfigPath, "ThornClient", "Default");
    private static FileSystemWatcher? _watcher;
    private static DateTime _lastRead = DateTime.MinValue;

    private static readonly HashSet<Configurable> ActiveModuleSyncs = [];
    private static readonly object SyncLock = new();
    private static bool _isBatchSyncing = false;
    private static readonly ConcurrentQueue<Configurable> MainThreadQueue = new();

    private static readonly JsonSerializerSettings SerializerSettings = new() {
        Formatting = Formatting.Indented,
        Converters = {
            new ColorJsonConverter(),
            new KeybindJsonConverter()
        }
    };

    private class ModuleConfigDataTransferObject {
        public bool IsEnabled { get; set; }
        public Dictionary<string, JToken> Settings { get; set; } = [];
    }

    /// <summary>
    /// Generates a safe file path using the module's (friendly) name string
    /// </summary>
    public static string GetConfigPath(Configurable configurable) {
        return Path.Combine(ConfigFolder, $"{configurable.GUID}.json");
    }

    /// <summary>
    /// Saves a Configurable's settings to disk
    /// </summary>
    public static void SaveConfig(Configurable configurable) {
        // Plugin.Log.LogInfo($"Saving {configurable.Name} to {GetConfigPath(configurable)}");
        lock (SyncLock) {
            if (_isBatchSyncing || ActiveModuleSyncs.Contains(configurable)) return;
            ActiveModuleSyncs.Add(configurable);
        }

        try {
            if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);

            var dto = new ModuleConfigDataTransferObject {
                IsEnabled = configurable.IsEnabled
            };

            foreach (var setting in configurable.Settings) {
                var rawValue = setting.GetValue();
                // Plugin.Log.LogInfo($"Setting {setting.Name} -> {rawValue}");
                if (rawValue != null) {
                    dto.Settings[setting.GUID] = JToken.FromObject(rawValue, JsonSerializer.Create(SerializerSettings));
                } else {
                    dto.Settings[setting.GUID] = JValue.CreateNull();
                }
            }

            string jsonString = JsonConvert.SerializeObject(dto, Formatting.Indented, settings: SerializerSettings);
            string filePath = GetConfigPath(configurable);

            File.WriteAllText(filePath, jsonString);
        } catch (Exception e) {
            Plugin.Log.LogError($"[ConfigManager] Failed to save {configurable.Name}: {e}");
        } finally {
            lock (SyncLock) {
                ActiveModuleSyncs.Remove(configurable);
            }
        }
    }

    /// <summary>
    /// Loads a Configurable's settings from disk
    /// </summary>
    public static void LoadConfig(Configurable configurable) {
        lock (SyncLock) {
            if (ActiveModuleSyncs.Contains(configurable)) return;
            ActiveModuleSyncs.Add(configurable);
        }

        string filePath = GetConfigPath(configurable);
        if (!File.Exists(filePath)) {
            lock (SyncLock) {
                ActiveModuleSyncs.Remove(configurable);
            }

            return;
        }

        try {
            string jsonString = File.ReadAllText(filePath);
            var dto = JsonConvert.DeserializeObject<ModuleConfigDataTransferObject>(jsonString,
                settings: SerializerSettings);
            if (dto == null) return;

            if (dto.IsEnabled != configurable.IsEnabled) {
                configurable.Toggle();
            }

            // Plugin.Log.LogInfo($"--- Checking {configurable.GUID}, json: {dto}");

            foreach (var setting in configurable.Settings) {
                // Plugin.Log.LogInfo($"  - Setting {setting}");
                if (dto.Settings.TryGetValue(setting.GUID, out var token) && token != null) {
                    try {
                        // Get the inner type of the Setting (e.g., UnityEngine.Color or Keybind)
                        Type targetType = setting.GetType().GetGenericArguments()[0];

                        // Deserialize directly to the correct type using your serializer settings
                        object? deserializedValue =
                            token.ToObject(targetType, JsonSerializer.Create(SerializerSettings));

                        if (deserializedValue != null) {
                            setting.SetValue(deserializedValue);
                        }
                    } catch (Exception ex) {
                        Plugin.Log.LogError($"[ConfigManager] Failed to convert setting {setting.GUID}: {ex.Message}");
                    }
                }
            }
        } catch (Exception e) {
            Plugin.Log.LogError($"[ConfigManager] Failed to load {configurable.Name}: {e}");
        } finally {
            lock (SyncLock) {
                ActiveModuleSyncs.Remove(configurable);
            }
        }
    }

    /// <summary>
    /// Saves all settings
    /// </summary>
    public static void SaveAll() {
        lock (SyncLock) {
            if (_isBatchSyncing) return;
            _isBatchSyncing = true;
        }

        try {
            foreach (var module in ModuleManager.Modules) {
                SaveConfig(module);
            }
            // Plugin.Log.LogInfo("[ConfigManager] Saved all module settings");
        } finally {
            lock (SyncLock) {
                _isBatchSyncing = false;
            }
        }
    }

    /// <summary>
    /// Loads all settings
    /// </summary>
    public static void LoadAll() {
        lock (SyncLock) {
            if (_isBatchSyncing) return;
            _isBatchSyncing = true;
        }

        try {
            foreach (var module in ModuleManager.Modules) {
                LoadConfig(module);
            }
            // Plugin.Log.LogInfo("[ConfigManager] Loaded all module settings");
        } finally {
            lock (SyncLock) {
                _isBatchSyncing = false;
            }
        }
    }

    /// <summary>
    /// Initializes a background listener that updates variables on text save events
    /// </summary>
    public static void SetupFileWatcher() {
        if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);
        if (_watcher != null) return;

        _watcher = new FileSystemWatcher {
            Path = ConfigFolder,
            Filter = "*.json",
            NotifyFilter = NotifyFilters.LastWrite
        };

        _watcher.Changed += OnConfigFileChanged;
        _watcher.EnableRaisingEvents = true;

        Plugin.Log.LogInfo("[ConfigManager] Background configuration hot-reload watcher is active");
    }

    private static void OnConfigFileChanged(object sender, FileSystemEventArgs e) {
        // Debounce
        DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);
        if (lastWriteTime - _lastRead < TimeSpan.FromMilliseconds(100)) return;
        _lastRead = lastWriteTime;

        string processedFileName = Path.GetFileNameWithoutExtension(e.Name);
        Configurable? targetModule = null;

        // While module has this file name?
        foreach (var module in ModuleManager.Modules) {
            string expectedFileName = Path.GetFileNameWithoutExtension(GetConfigPath(module));

            // Case insensitive match because Windows Explorer is sloppy
            if (string.Equals(expectedFileName, processedFileName, StringComparison.OrdinalIgnoreCase)) {
                targetModule = module;
                break;
            }
        }

        if (targetModule != null) {
            // Push to queue to safely process on the main engine update tick loop
            MainThreadQueue.Enqueue(targetModule);
        }
    }

    /// <summary>
    /// To drain config hot reloads
    /// </summary>
    public static void UpdateMainThreadQueue() {
        while (MainThreadQueue.TryDequeue(out var module)) {
            // Plugin.Log.LogInfo($"[ConfigManager] Processing hot-reload thread action for: {module.Name}");
            LoadConfig(module);
        }
    }
}
