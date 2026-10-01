using System.IO;
using System.Reflection;

namespace ThornClientModules;

/// <summary>
/// The BaseUnityPlugin equivalent for the ThornClientModules bundle.
/// It just stores some info and is not a BaseUnityPlugin.
/// </summary>
public static class ModulesBundleInfo {
    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);
}
