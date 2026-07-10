using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Notiffy.API;
using ThornClient.Core;
using ThornClient.Managers;
using Module = System.Reflection.Module;

namespace ThornClient;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("com.github.end-4.nukeLib")]
public class Plugin : BaseUnityPlugin {
    // Logger
    internal static ManualLogSource Log;

    // Plugin config
    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);
    public const string PluginGUID = "com.github.end-4.thornClient";
    public const string PluginName = "ThornClient";
    public const string PluginVersion = "0.1.0";
    public static string PluginIconPath = Path.Combine(workingDir, "icon.png");

    private void Awake() {
        Log = Logger;
        ModuleManager.Initialize();
        ConfigManager.LoadAll();
        ConfigManager.SetupFileWatcher();
        Log.LogInfo($"Thorn is loaded");
    }

    private void Update() {
        ThornClient.Managers.InputManager.Update();
        foreach (var module in ModuleManager.Modules) {
            if (module.IsEnabled) {
                try {
                    module.OnUpdate();
                } catch (Exception e) {
                    NotificationSystem.NotifySend($"Thorn > {module.Name}",
                        $"Error in update loop of {module.Name}, disabling to prevent further issues. Check the logs for further details.",
                        appName: "Thorn", iconFilePath: PluginIconPath);
                    Log.LogError(
                        $"Error in module '{module.Name}' during Update! Disabling to prevent log spam/crashes");
                    Log.LogError(e);
                    module.Toggle();
                }
            }
        }
    }

    private void OnDestroy() {
        ConfigManager.SaveAll();
    }
}
