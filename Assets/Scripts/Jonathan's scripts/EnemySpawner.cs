using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<EnemySpawnConfig> enemiesToSpawn;
    public float spawnRadius;

    private bool hasSpawned;

    private void Start()
    {
        bool initialSpawnState = false;
        hasSpawned = initialSpawnState;

        int listCount = enemiesToSpawn.Count;
        for (int i = 0; i < listCount; i++)
        {
            EnemySpawnConfig config = enemiesToSpawn[i];
            GameObject prefab = config.enemyPrefab;
            int count = config.spawnCount;

            ObjectPoolManager.Instance.CreatePool(prefab, count);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = other.CompareTag("Player");
        if (isPlayer)
        {
            if (!hasSpawned)
            {
                bool spawnedState = true;
                hasSpawned = spawnedState;
                SpawnEnemies();
            }
        }
    }
    private void SpawnEnemies()
    {
        int listCount = enemiesToSpawn.Count;
        for (int i = 0; i < listCount; i++)
        {
            EnemySpawnConfig config = enemiesToSpawn[i];
            GameObject prefab = config.enemyPrefab;
            int spawnCount = config.spawnCount;
            bool useSpecific = config.useSpecificSpawnPoints;
            List<Transform> specificPoints = config.specificSpawnPoints;

            for (int j = 0; j < spawnCount; j++)
            {
                Vector3 spawnPosition = GetRandomSpawnPosition();

                if (useSpecific)
                {
                    bool hasPointsList = specificPoints != null;
                    if (hasPointsList)
                    {
                        int pointsCount = specificPoints.Count;
                        bool hasPoints = pointsCount > 0;
                        if (hasPoints)
                        {
                            int pointIndex = j % pointsCount;
                            Transform pointTransform = specificPoints[pointIndex];
                            Vector3 specificPos = pointTransform.position;
                            spawnPosition = specificPos;
                        }
                    }
                }

                Quaternion defaultRotation = Quaternion.identity;
                GameObject spawnedEnemy = ObjectPoolManager.Instance.SpawnFromPool(prefab, spawnPosition, defaultRotation);

                EnemyHealth healthScript = spawnedEnemy.GetComponent<EnemyHealth>();
                bool hasHealthScript = healthScript != null;

                if (hasHealthScript)
                {
                    healthScript.originalPrefab = prefab;
                }
            }
        }
    }
    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle;
        float randomX = randomCircle.x * spawnRadius;
        float randomZ = randomCircle.y * spawnRadius;

        Transform spawnerTransform = transform;
        Vector3 centerPosition = spawnerTransform.position;

        float finalX = centerPosition.x + randomX;
        float finalY = centerPosition.y;
        float finalZ = centerPosition.z + randomZ;

        Vector3 targetPosition = new Vector3(finalX, finalY, finalZ);

        return targetPosition;
    }
}