using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Notiffy.API;
using NukeLib.Utils;
using ThornClient.Managers;
using ThornClient.Patches;

namespace ThornClient;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("com.github.end-4.nukeLib")]
[BepInDependency("com.github.end-4.notiffy")]
public class Plugin : BaseUnityPlugin {
    internal static ManualLogSource Log;
    public static Plugin Instance { get; private set; }

    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);
    public const string PluginGUID = "com.github.end-4.thornClient";
    public const string PluginName = "Thorn";
    public const string PluginVersion = "0.1.5";
    public static string PluginIconPath = Path.Combine(workingDir, "icon.png");

    public static Harmony HarmonyInstance;

    private void Awake() {
        Log = Logger;
        if (Instance != null) return;
        Instance = this;

        ProfileManager.Initialize();
        AssetManager.Initialize();
        HudManager.Initialize();
        EffectManager.Initialize();
        ModuleManager.Initialize();
        RenderManager.Initialize();
        ConfigManager.LoadAll();
        ConfigManager.SetupFileWatcher();

        HarmonyInstance = new Harmony(PluginGUID);
        HarmonyInstance.PatchAll(typeof(OptionsManagerPatches));
        HarmonyInstance.PatchAll(typeof(CheatsManagerPatches));
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
                        $"Error in Update loop of {module.Name}, disabling to prevent further issues. Check BepInEx logs for further details.",
                        appName: "Thorn", iconFilePath: PluginIconPath);
                    Log.LogError(
                        $"Error in module '{module.Name}' during Update! Disabling to prevent log spam");
                    Log.LogError(e);
                    module.Toggle();
                }
            }
        }
    }

    private void FixedUpdate() {
        // We assume physics are only ingame
        // (also fixes that one null error i got out of nowhere)
        if (!SceneUtils.IsInGame()) return;
        foreach (var module in ModuleManager.Items) {
            if (module.IsEnabled) {
                try {
                    module.OnFixedUpdate();
                } catch (Exception e) {
                    NotificationSystem.NotifySend($"Thorn > {module.Name}",
                        $"Error in FixedUpdate loop of {module.Name}, disabling to prevent further issues. Check BepInEx logs for further details.",
                        appName: "Thorn", iconFilePath: PluginIconPath);
                    Log.LogError(
                        $"Error in module '{module.Name}' during FixedUpdate! Disabling to prevent log spam");
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
