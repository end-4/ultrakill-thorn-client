using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;

namespace ThornClient.Managers;

/// <summary>
/// Handles saving and loading of configurable settings to and from disk, as well as hot-reloading when configuration files are changed externally.
/// </summary>
public static class ConfigManager {
    private static readonly string ConfigFolder = Path.Combine(Paths.ConfigPath, "ThornClient", "Default");
    private static FileSystemWatcher? _watcher;
    private static DateTime _lastRead = DateTime.MinValue;

    private static readonly HashSet<Configurable> ActiveSyncs = [];
    private static readonly object SyncLock = new();
    private static bool _isBatchSyncing = false;
    private static readonly ConcurrentQueue<Configurable> MainThreadQueue = new();

    private static readonly JsonSerializerSettings SerializerSettings = new() {
        Formatting = Formatting.Indented,
        Converters = {
            new ColorJsonConverter(),
            new KeybindJsonConverter(),
            new Newtonsoft.Json.Converters.StringEnumConverter()
        }
    };

    /// <summary>
    /// Registers a JSON converter for a setting of custom type.
    /// </summary>
    /// <param name="converter"></param>
    public static void RegisterJsonConverter(JsonConverter converter) {
        if (SerializerSettings.Converters.Any(c => c.GetType() == converter.GetType())) return;
        SerializerSettings.Converters.Add(converter);
    }

    private class ConfigDataTransferObject {
        public bool IsEnabled { get; set; }
        public Dictionary<string, JToken> Settings { get; set; } = [];
    }

    private static List<Configurable> GetAllConfigurables() {
        return [..ModuleManager.Items];
    }

    /// <summary>
    /// Collects settings from the elements list that might contain subgroups
    /// </summary>
    /// <param name="elements">The elements list, possibly nested</param>
    /// <param name="foundSettings">The list to collect settings to</param>
    private static void CollectSettings(IEnumerable<IConfigurableElement> elements, List<Setting> foundSettings) {
        foreach (var element in elements) {
            if (element is Setting setting) {
                foundSettings.Add(setting);
            } else if (element is SettingGroup group) {
                CollectSettings(group.Elements, foundSettings);
            }
        }
    }

    /// <summary>
    /// Generates a safe file path using the configurable's GUID
    /// </summary>
    public static string GetConfigPath(Configurable configurable) {
        return Path.Combine(ConfigFolder, $"{configurable.GUID}.json");
    }

    /// <summary>
    /// Saves a Configurable's settings to disk
    /// </summary>
    public static void SaveConfig(Configurable configurable) {
        lock (SyncLock) {
            if (_isBatchSyncing || ActiveSyncs.Contains(configurable)) return;
            ActiveSyncs.Add(configurable);
        }

        try {
            if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);

            var dto = new ConfigDataTransferObject {
                IsEnabled = configurable.IsEnabled
            };

            var allSettings = new List<Setting>();
            CollectSettings(configurable.Elements, allSettings);

            foreach (var setting in allSettings) {
                var rawValue = setting.GetValue();
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
                ActiveSyncs.Remove(configurable);
            }
        }
    }

    /// <summary>
    /// Loads a Configurable's settings from disk
    /// </summary>
    public static void LoadConfig(Configurable configurable) {
        lock (SyncLock) {
            if (ActiveSyncs.Contains(configurable)) return;
            ActiveSyncs.Add(configurable);
        }

        string filePath = GetConfigPath(configurable);
        if (!File.Exists(filePath)) {
            lock (SyncLock) {
                ActiveSyncs.Remove(configurable);
            }
            return;
        }

        try {
            string jsonString = File.ReadAllText(filePath);
            var dto = JsonConvert.DeserializeObject<ConfigDataTransferObject>(jsonString,
                settings: SerializerSettings);
            if (dto == null) return;

            if (dto.IsEnabled != configurable.IsEnabled) {
                configurable.Toggle();
            }

            var allSettings = new List<Setting>();
            CollectSettings(configurable.Elements, allSettings);

            foreach (var setting in allSettings) {
                if (dto.Settings.TryGetValue(setting.GUID, out var token) && token != null) {
                    try {
                        Type targetType = setting.GetType().GetGenericArguments()[0];

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
                ActiveSyncs.Remove(configurable);
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
            foreach (var configurable in GetAllConfigurables()) {
                SaveConfig(configurable);
            }
            // Plugin.Log.LogInfo("[ConfigManager] Saved all configurable settings");
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
            foreach (var configurable in GetAllConfigurables()) {
                LoadConfig(configurable);
            }
            // Plugin.Log.LogInfo("[ConfigManager] Loaded all configurable settings");
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
        Configurable? targetConfigurable = null;

        // While module has this file name?
        foreach (var configurable in GetAllConfigurables()) {
            string expectedFileName = Path.GetFileNameWithoutExtension(GetConfigPath(configurable));

            // Case insensitive match because Windows Explorer is sloppy
            if (string.Equals(expectedFileName, processedFileName, StringComparison.OrdinalIgnoreCase)) {
                targetConfigurable = configurable;
                break;
            }
        }

        if (targetConfigurable != null) {
            // Push to queue to safely process on the main engine update tick loop
            MainThreadQueue.Enqueue(targetConfigurable);
        }
    }

    /// <summary>
    /// To drain config hot reloads
    /// </summary>
    public static void UpdateMainThreadQueue() {
        while (MainThreadQueue.TryDequeue(out var module)) {
            // Plugin.Log.LogInfo($"[ConfigManager] Processing hot-reload for: {module.Name}");
            LoadConfig(module);
        }
    }
}
