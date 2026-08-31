using System;
using System.Collections.Generic;
using System.Linq;
using ThornClient.Core;
using Module = ThornClient.Core.Module;

namespace ThornClient.Managers;

/// <summary>
/// The manager that discovers and loads Modules.
/// </summary>
public static class ModuleManager {
    /// <summary>
    /// All modules
    /// </summary>
    public static List<Module> Items { get; private set; } = [];

    /// <summary>
    /// Triggered when a module is toggled. Useful for module lists elements.
    /// </summary>
    public static Action<Module, bool>? SomeModuleToggled;

    internal static void HeyIToggled(Module module, bool enabled) {
        SomeModuleToggled?.Invoke(module, enabled);
    }

    /// <summary>
    /// Scans for Modules via reflection, then instantiates and registers all of them
    /// </summary>
    public static void Initialize() {
        Plugin.Log.LogInfo($"[Module Manager] Starting...");
        var moduleType = typeof(Module);
        List<Type> discoveredTypes = [];

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            if (ReflectionUtils.IsSystemAssembly(assembly)) continue;
            foreach (var type in ReflectionUtils.SafeGetTypes(assembly)) {
                if (ReflectionUtils.SafeIsAssignableFrom(moduleType, type) && !type.IsInterface && !type.IsAbstract) {
                    discoveredTypes.Add(type);
                }
            }
        }

        foreach (var type in discoveredTypes) {
            try {
                if (Activator.CreateInstance(type) is Module moduleInstance) {
                    Items.Add(moduleInstance);
                    Plugin.Log.LogInfo($"[Module Manager] + {moduleInstance.Name}");
                }
            } catch (Exception ex) {
                Plugin.Log.LogError($"Failed to instantiate module '{type.Name}': {ex}");
            }
        }

        Plugin.Log.LogInfo($"[Module Manager] Loaded {Items.Count} modules");
    }

    /// <summary>
    /// Returns all modules in a category
    /// </summary>
    public static List<Module> GetByCategory(ModuleCategory category) {
        return Items.Where(m => m.Category == category).ToList();
    }

    /// <summary>
    /// Gets a specific module instance by its class name.
    /// </summary>
    public static T Get<T>() where T : Module {
        return Items.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Alternative loose lookup to fetch a module by its display name string.
    /// </summary>
    public static Module GetByName(string name) {
        return Items.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
