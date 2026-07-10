using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace ThornClient;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("com.end-4.nukeLib")]
public class Plugin : BaseUnityPlugin {
    // Logger
    internal static ManualLogSource Log;

    // Plugin config
    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);
    public const string PluginGUID = "com.github.end-4.thornClient";
    public const string PluginName = "ThornClient";
    public const string PluginVersion = "0.1.0";

    private void Awake() {
        Log = Logger;
        Log.LogInfo($"Thorn is loaded");
    }
}
