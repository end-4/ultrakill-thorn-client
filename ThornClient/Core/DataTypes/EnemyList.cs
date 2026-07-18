using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ThornClient.Core.DataTypes;

/// <summary>
/// List of enemy types
/// </summary>
[JsonConverter(typeof(EnemyListJsonConverter))]
public class EnemyList : IEquatable<EnemyList> {
    public HashSet<EnemyType> enemies;

    public EnemyList(EnemyType[] enemies) {
        this.enemies = new HashSet<EnemyType>(enemies);
    }

    public EnemyList() {
        this.enemies = [];
    }

    public void Add(EnemyType enemyType) {
        enemies.Add(enemyType);
    }

    public void Remove(EnemyType enemyType) {
        enemies.Remove(enemyType);
    }

    public bool Equals(EnemyList? other) {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return enemies.SetEquals(other.enemies);
    }
}
