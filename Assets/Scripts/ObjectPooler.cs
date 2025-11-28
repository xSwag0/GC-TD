using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int amountToSpawn = 7;
    private List<GameObject> _pool;
    
    void Start()
    {
        _pool = new List<GameObject>();
        for (int i = 0; i < amountToSpawn; i++)
        {
            CreateNewObject();
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject spawnedObject = Instantiate(prefab, transform);
        spawnedObject.SetActive(false);
        _pool.Add(spawnedObject);
        return spawnedObject;
    }

    public GameObject GetObjectFromPool()
    {
        foreach (GameObject spawnedObject in _pool)
        {
            if (spawnedObject.activeSelf == false)
            {
                return spawnedObject;
            }
        }
        return CreateNewObject(); 
    }
}
