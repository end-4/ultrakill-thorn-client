using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Notiffy.API;
using ThornClient.Managers;
using UnityEngine.SceneManagement;

namespace ThornClient;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("com.github.end-4.nukeLib")]
public class Plugin : BaseUnityPlugin {
    internal static ManualLogSource Log;
    public static Plugin Instance { get; private set; }

    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);
    public const string PluginGUID = "com.github.end-4.thornClient";
    public const string PluginName = "Thorn";
    public const string PluginVersion = "0.1.0";
    public static string PluginIconPath = Path.Combine(workingDir, "icon.png");

    private void Awake() {
        Log = Logger;
        if (Instance != null) return;
        Instance = this;

        AssetManager.Initialize();
        HudManager.Initialize();
        ModuleManager.Initialize();
        RenderManager.Initialize();
        ConfigManager.LoadAll();
        ConfigManager.SetupFileWatcher();
        Harmony harmony = new Harmony(PluginGUID);
        harmony.PatchAll();
        Log.LogInfo($"Thorn is loaded");
    }

    private void Update() {
        ConfigManager.UpdateMainThreadQueue();
        ThornClient.Managers.InputManager.Update();
        foreach (var module in ModuleManager.Items) {
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

    private void OnRenderObject() {
        RenderManager.RenderPipeline();
    }
}
