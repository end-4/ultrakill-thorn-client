using System;
using System.Collections.Generic;
using System.Reflection;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using UnityEngine;

namespace ThornClient.Managers;

/// <summary>
/// Manages UI creators for settings and configurable elements.
/// Annotate your data type with [ConfigurableUICreator(typeof(YourUICreatorType))] for custom UI
/// </summary>
public static class UICreatorManager {
    private static bool _isInitialized = false;
    private static readonly Dictionary<Type, IConfigurableUICreator> RegisteredCreators = [];

    static UICreatorManager() {
        RegisterUICreator(typeof(Setting<bool>), typeof(BoolUICreator));
        RegisterUICreator(typeof(Setting<Color>), typeof(ColorUICreator));
        RegisterUICreator(typeof(Setting<Enum>), typeof(EnumUICreator));
        RegisterUICreator(typeof(Setting<float>), typeof(FloatUICreator));
        RegisterUICreator(typeof(Setting<int>), typeof(IntUICreator));
        RegisterUICreator(typeof(Setting<string>), typeof(StringUICreator));
        RegisterAttributeDeclaredCreators();
    }

    /// <summary>
    /// Registers a UI creator for a specific type
    /// </summary>
    /// <param name="elementType">The type of the element</param>
    /// <param name="creator"></param>
    /// <exception cref="ArgumentException">When the element type isn't a configurable element</exception>
    public static void RegisterUICreator(Type elementType, IConfigurableUICreator creator) {
        if (RegisteredCreators.ContainsKey(elementType)) return;

        if (!typeof(IConfigurableElement).IsAssignableFrom(elementType)) {
            throw new ArgumentException($"Type {elementType.Name} must implement IConfigurableElement");
        }

        RegisteredCreators[elementType] = creator;
    }

    /// <summary>
    /// Registers a UI creator for a specific type
    /// </summary>
    /// <param name="elementType">The type of the element</param>
    /// <param name="creatorType">The UI creator type</param>
    /// <exception cref="ArgumentException">When the UI creator type doesn't implement IConfigurableUICreator or when the element type isn't a configurable element</exception>
    public static void RegisterUICreator(Type elementType, Type creatorType) {
        if (!typeof(IConfigurableUICreator).IsAssignableFrom(creatorType)) {
            throw new ArgumentException($"Type {creatorType.Name} must implement {nameof(IConfigurableUICreator)}");
        }

        if (Activator.CreateInstance(creatorType) is IConfigurableUICreator creator) {
            RegisterUICreator(elementType, creator);
        }
    }

    /// <summary>
    /// Scans for [ConfigurableUICreator] attributes and add them
    /// </summary>
    private static void RegisterAttributeDeclaredCreators() {
        if (_isInitialized) return;
        _isInitialized = true;

        try {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (ReflectionUtils.IsSystemAssembly(assembly)) continue;
                foreach (var type in ReflectionUtils.SafeGetTypes(assembly)) {
                    try {
                        var attribute = type.GetCustomAttribute<ConfigurableUICreatorAttribute>();
                        if (attribute == null) continue;

                        if (Activator.CreateInstance(attribute.CreatorType) is not IConfigurableUICreator creator) {
                            Plugin.Log.LogWarning(
                                $"[UICreatorManager] Failed to instantiate creator {attribute.CreatorType.Name}");
                            continue;
                        }

                        // If it's an IConfigurable, register it directly.
                        // Else, consider it a setting data type and register for Setting<type>
                        Type targetType = typeof(IConfigurableElement).IsAssignableFrom(type)
                            ? type
                            : typeof(Setting<>).MakeGenericType(type);

                        RegisterUICreator(targetType, creator);
                    } catch (Exception ex) {
                        Plugin.Log.LogError($"[UICreatorManager] Failed to register creator for {type.Name}: {ex}");
                    }
                }
            }
        } catch (Exception ex) {
            Plugin.Log.LogError($"[UICreatorManager] Failed to initialize: {ex}");
        }
    }

    /// <summary>
    /// Tries to get a UI creator for a given type
    /// </summary>
    public static bool TryGetUICreator(Type elementType, out IConfigurableUICreator? creator) {
        if (RegisteredCreators.TryGetValue(elementType, out creator)) {
            return true;
        }

        // Handle Setting<SomeEnum>
        if (elementType.IsGenericType &&
            elementType.GetGenericTypeDefinition() == typeof(Setting<>) &&
            elementType.GetGenericArguments()[0].IsEnum) {
            return RegisteredCreators.TryGetValue(typeof(Setting<Enum>), out creator);
        }

        creator = null;
        return false;
    }
}
