using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        bool hasInstance = Instance != null;
        if (hasInstance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        Dictionary<GameObject, Queue<GameObject>> newDict = new Dictionary<GameObject, Queue<GameObject>>();
        poolDictionary = newDict;
    }
    public void CreatePool(GameObject prefab, int poolSize)
    {
        bool hasKey = poolDictionary.ContainsKey(prefab);
        if (!hasKey)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(prefab, objectPool);
        }
    }
    public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        bool hasKey = poolDictionary.ContainsKey(prefab);
        if (!hasKey)
        {
            return null;
        }

        Queue<GameObject> objectPool = poolDictionary[prefab];
        int currentCount = objectPool.Count;

        if (currentCount == 0)
        {
            GameObject newObj = Instantiate(prefab);
            newObj.SetActive(false);
            objectPool.Enqueue(newObj);
        }

        GameObject objectToSpawn = objectPool.Dequeue();
        objectToSpawn.SetActive(true);

        Transform objTransform = objectToSpawn.transform;
        objTransform.position = position;
        objTransform.rotation = rotation;

        return objectToSpawn;
    }
    public void ReturnToPool(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        bool hasKey = poolDictionary.ContainsKey(prefab);

        if (hasKey)
        {
            Queue<GameObject> objectPool = poolDictionary[prefab];
            objectPool.Enqueue(obj);
        }
    }
}