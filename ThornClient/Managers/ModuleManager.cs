using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ThornClient.Core;
using Module = ThornClient.Core.Module;

namespace ThornClient.Managers;

public static class ModuleManager {
    public static List<Module> Items { get; private set; } = [];

    public static Action<Module, bool>? SomeModuleToggled;

    internal static void HeyIToggled(Module module, bool enabled) {
        SomeModuleToggled?.Invoke(module, enabled);
    }

    /// <summary>
    /// Scans the entire assembly via reflection, instantiates every class
    /// that inherits from Module, and registers it
    /// </summary>
    public static void Initialize() {
        Plugin.Log.LogInfo($"[Module Manager] Starting...");

        var moduleType = typeof(Module);

        // Scan the active executing assembly for any concrete class inheriting from Module
        var discoveredTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => moduleType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in discoveredTypes) {
            try {
                // Instantiate the module
                if (Activator.CreateInstance(type) is Module moduleInstance) {
                    Items.Add(moduleInstance);
                }
            } catch (Exception ex) {
                Plugin.Log.LogInfo($"Failed to instantiate module '{type.Name}': {ex.Message}");
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
