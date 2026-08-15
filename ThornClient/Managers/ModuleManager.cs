using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            if (IsSystemAssembly(assembly)) continue;
            foreach (var type in SafeGetTypes(assembly)) {
                if (SafeIsAssignableFrom(moduleType, type) && !type.IsInterface && !type.IsAbstract) {
                    discoveredTypes.Add(type);
                }
            }
        }

        foreach (var type in discoveredTypes) {
            try {
                if (Activator.CreateInstance(type) is Module moduleInstance) {
                    Items.Add(moduleInstance);
                }
            } catch (Exception ex) {
                Plugin.Log.LogError($"Failed to instantiate module '{type.Name}': {ex}");
            }
        }

        Plugin.Log.LogInfo($"[Module Manager] Loaded {Items.Count} modules");
    }

    /// <summary>
    /// Safely checks if targetType can be assigned from the candidate type,
    /// suppressing exceptions caused by missing assembly references.
    /// </summary>
    /// <param name="targetType">The target type</param>
    /// <param name="candidateType">The candidate type</param>
    /// <returns>True if assignable, false otherwise</returns>
    private static bool SafeIsAssignableFrom(Type targetType, Type candidateType) {
        try {
            return targetType.IsAssignableFrom(candidateType);
        } catch (TypeLoadException) {
            // Occurs when candidateType references fields/methods from an assembly that isn't loaded
            return false;
        } catch (Exception) {
            return false;
        }
    }

    /// <summary>
    /// Safely fetches types from an assembly, suppressing loading errors.
    /// </summary>
    /// <param name="assembly">The assembly</param>
    /// <returns>Types from the given assembly</returns>
    private static IEnumerable<Type> SafeGetTypes(Assembly assembly) {
        try {
            return assembly.GetTypes();
        } catch (ReflectionTypeLoadException ex) {
            // Return any successfully loaded types while ignoring missing ones
            return ex.Types.Where(t => t != null)!;
        } catch (Exception) {
            return [];
        }
    }

    /// <summary>
    /// Filters out standard framework assemblies to avoid unnecessary scanning overhead.
    /// </summary>
    private static bool IsSystemAssembly(Assembly assembly) {
        var name = assembly.FullName;
        if (string.IsNullOrEmpty(name)) return true;

        return name.StartsWith("System") || name.StartsWith("Microsoft") || name.StartsWith("mscorlib") ||
               name.StartsWith("Unity") || name.StartsWith("UnityEngine");
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
