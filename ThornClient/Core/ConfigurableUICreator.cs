using System;
using ThornClient.Core.ConfigurableElements;
using UnityEngine;

namespace ThornClient.Core;

/// <summary>
/// Interface for UI creators for settings and other configurable elements
/// </summary>
public interface IConfigurableUICreator {
    /// <summary>
    /// Creates a UI element for the given configurable element
    /// </summary>
    /// <param name="element">The element to create UI for</param>
    /// <returns>The created GameObject, or null if creation failed</returns>
    GameObject? CreateUI(IConfigurableElement element);
}

/// <summary>
/// Generic interface for strongly-typed UI creation
/// </summary>
public interface IConfigurableUICreator<in T> : IConfigurableUICreator where T : IConfigurableElement {
    /// <summary>
    /// Creates UI for a specific element type
    /// </summary>
    GameObject? CreateUI(T element);

    GameObject? IConfigurableUICreator.CreateUI(IConfigurableElement element) {
        return element is T typedElement ? CreateUI(typedElement) : null;
    }
}

/// <summary>
/// Marks a data type or configurable element type to use a specific UI creator
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ConfigurableUICreatorAttribute : Attribute {
    public Type CreatorType { get; }

    /// <summary>
    /// Specify an UI creator type for a certain configurable element
    /// </summary>
    /// <param name="creatorType">The type of the creator</param>
    /// <exception cref="ArgumentException">Thrown when the given type does not implement IConfigurableUICreator</exception>
    public ConfigurableUICreatorAttribute(Type creatorType) {
        if (!typeof(IConfigurableUICreator).IsAssignableFrom(creatorType)) {
            throw new ArgumentException($"Type {creatorType.Name} must implement IConfigurableUICreator");
        }

        CreatorType = creatorType;
    }
}
