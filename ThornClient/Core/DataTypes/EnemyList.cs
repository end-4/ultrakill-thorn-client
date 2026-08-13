using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// List of enemy types
/// </summary>
[JsonConverter(typeof(EnemyListJsonConverter))]
public class EnemyList : IEquatable<EnemyList> {
    /// <summary>
    /// The set of enemies
    /// </summary>
    public HashSet<EnemyType> Enemies;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="enemies">The array of enemy types</param>
    public EnemyList(params EnemyType[] enemies) {
        Enemies = [.. enemies];
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="enemies">The list of enemy types</param>
    public EnemyList(IEnumerable<EnemyType> enemies) {
        Enemies = [.. enemies];
    }

    /// <summary>
    /// Parameterless constructor for JSON deserialization
    /// </summary>
    public EnemyList() : this([]) { }

    /// <summary>
    /// Adds an enemy type to the list
    /// </summary>
    /// <param name="enemyType">The enemy type to add</param>
    public void Add(EnemyType enemyType) {
        Enemies.Add(enemyType);
    }

    /// <summary>
    /// Removes an enemy type from the list
    /// </summary>
    /// <param name="enemyType">The enemy type to remove</param>
    public void Remove(EnemyType enemyType) {
        Enemies.Remove(enemyType);
    }

    /// <summary>
    /// Toggles an enemy type in the list
    /// </summary>
    /// <param name="enemyType">The enemy type to toggle</param>
    public void Toggle(EnemyType enemyType) {
        if (Includes(enemyType)) Remove(enemyType);
        else Add(enemyType);
    }

    /// <summary>
    /// Checks if the list includes an enemy type
    /// </summary>
    /// <param name="enemyType">The enemy type to check</param>
    /// <returns>True if the list includes the enemy type, false otherwise</returns>
    public bool Includes(EnemyType enemyType) {
        return Enemies.Contains(enemyType);
    }

    /// <summary>
    /// Gets the count of enemy types in the list
    /// </summary>
    /// <returns>The count of enemy types</returns>
    public int Count() {
        return Enemies.Count;
    }

    /// <summary>
    /// Clones the list
    /// </summary>
    /// <returns>A new instance of the list</returns>
    public EnemyList Clone() {
        return new EnemyList(Enemies);
    }

    /// <summary>
    /// Checks if the list is equal to another list
    /// </summary>
    /// <param name="other">The other list to compare</param>
    /// <returns>True if the lists are equal, false otherwise</returns>
    public bool Equals(EnemyList? other) {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Enemies.SetEquals(other.Enemies);
    }
}
