using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ThornClient.Core;

/// <summary>
/// Reflection utilities for safely scanning assemblies and types.
/// </summary>
internal static class ReflectionUtils {
    /// <summary>
    /// Safely fetches types from an assembly, suppressing loading errors.
    /// </summary>
    /// <param name="assembly">The assembly</param>
    /// <returns>Types from the given assembly</returns>
    public static IEnumerable<Type> SafeGetTypes(Assembly assembly) {
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
    /// <param name="assembly">The assembly</param>
    /// <returns>True if the assembly is a framework/system assembly</returns>
    public static bool IsSystemAssembly(Assembly assembly) {
        var name = assembly.FullName;
        if (string.IsNullOrEmpty(name)) return true;

        return name.StartsWith("System") || name.StartsWith("Microsoft") || name.StartsWith("mscorlib") ||
               name.StartsWith("Unity") || name.StartsWith("UnityEngine");
    }

    /// <summary>
    /// Safely checks if targetType can be assigned from the candidate type,
    /// suppressing exceptions caused by missing assembly references.
    /// </summary>
    /// <param name="targetType">The target type</param>
    /// <param name="candidateType">The candidate type</param>
    /// <returns>True if assignable, false otherwise</returns>
    public static bool SafeIsAssignableFrom(Type targetType, Type candidateType) {
        try {
            return targetType.IsAssignableFrom(candidateType);
        } catch (TypeLoadException) {
            // Occurs when candidateType references fields/methods from an assembly that isn't loaded
            return false;
        } catch (Exception) {
            return false;
        }
    }
}

