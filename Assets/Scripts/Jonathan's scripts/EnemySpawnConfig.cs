using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnemySpawnConfig
{
    public GameObject enemyPrefab;
    public int spawnCount;
    public bool useSpecificSpawnPoints;
    public List<Transform> specificSpawnPoints;
}