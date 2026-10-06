using System.IO;
using System.Reflection;
using BepInEx;

namespace ThornClientModules;

public class ModulesPlugin : BaseUnityPlugin {
    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);
}
