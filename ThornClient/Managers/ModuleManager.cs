using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ThornClient.Core;
using Module = ThornClient.Core.Module;

namespace ThornClient.Managers;

public static class ModuleManager {
    public static List<Module> Modules { get; private set; } = [];

    /// <summary>
    /// Scans the entire project assembly via reflection, instantiates every class
    /// that inherits from Module, and registers it automatically.
    /// </summary>
    public static void Initialize() {
        Plugin.Log.LogInfo($"ModuleManager starting...");

        var moduleType = typeof(Module);

        // Scan the active executing assembly for any concrete class inheriting from Module
        var discoveredTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => moduleType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in discoveredTypes) {
            try {
                // Instantiate the module via its default constructor
                if (Activator.CreateInstance(type) is Module moduleInstance) {
                    Modules.Add(moduleInstance);
                    Plugin.Log.LogInfo($"Added module: {moduleInstance.Name} [{moduleInstance.Category}]");
                }
            } catch (Exception ex) {
                Plugin.Log.LogInfo($"Failed to instantiate module type '{type.Name}': {ex.Message}");
            }
        }

        Plugin.Log.LogInfo($"ModuleManager initialized. Found {Modules.Count} modules.");
    }

    /// <summary>
    /// Returns all modules in a category
    /// </summary>
    public static List<Module> GetByCategory(ModuleCategory category) {
        return Modules.Where(m => m.Category == category).ToList();
    }

    /// <summary>
    /// Gets a specific module instance by its class name.
    /// </summary>
    public static T Get<T>() where T : Module {
        return Modules.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Alternative loose lookup to fetch a module by its display name string.
    /// </summary>
    public static Module GetByName(string name) {
        return Modules.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
