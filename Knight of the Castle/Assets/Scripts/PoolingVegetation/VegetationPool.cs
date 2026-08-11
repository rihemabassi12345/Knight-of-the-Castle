using System.Collections.Generic;
using UnityEngine;

public class VegetationPool
{
    private readonly GameObject prefab;
    private readonly Stack<GameObject> inactivePool;
    private readonly Transform parent;
    private readonly int maxCapacity;

    public VegetationPool(GameObject currentPrefab, int initialSize, Transform currentParent)
    {
        prefab = currentPrefab;
        parent = currentParent;
        maxCapacity = initialSize;
        inactivePool = new Stack<GameObject>(initialSize);

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.SetActive(false);
            inactivePool.Push(obj);
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        // Strict cap: If pool is empty, return null instead of instantiating new objects
        if (inactivePool.Count == 0)
        {
            return null;
        }

        GameObject obj = inactivePool.Pop();
        Transform t = obj.transform;
        t.SetPositionAndRotation(position, rotation);
        t.localScale = scale;

        obj.SetActive(true);
        return obj;
    }

    public void Release(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);

        // Ensure we don't exceed initial capacity if objects are freed
        if (inactivePool.Count < maxCapacity)
        {
            inactivePool.Push(obj);
        }
        else
        {
            Object.Destroy(obj);
        }
    }
}