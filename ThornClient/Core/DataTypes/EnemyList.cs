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
    public HashSet<EnemyType> Enemies;

    public EnemyList(EnemyType[] enemies) {
        this.Enemies = [.. enemies];
    }

    public EnemyList(List<EnemyType> enemies) {
        this.Enemies = [.. enemies];
    }

    public EnemyList() {
        this.Enemies = [];
    }

    public void Add(EnemyType enemyType) {
        Enemies.Add(enemyType);
    }

    public void Remove(EnemyType enemyType) {
        Enemies.Remove(enemyType);
    }

    public void Toggle(EnemyType enemyType) {
        if (Includes(enemyType)) Remove(enemyType);
        else Add(enemyType);
    }

    public bool Includes(EnemyType enemyType) {
        return Enemies.Contains(enemyType);
    }

    public int Count() {
        return Enemies.Count;
    }

    public EnemyList Clone() {
        return new EnemyList(Enemies.ToList());
    }

    public bool Equals(EnemyList? other) {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Enemies.SetEquals(other.Enemies);
    }
}
