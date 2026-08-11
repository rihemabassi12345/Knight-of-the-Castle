using System.Collections.Generic;
using UnityEngine;

public class VegetationPool
{
    private readonly GameObject prefab;
    private readonly Stack<GameObject> inactivePool;
    private readonly Transform parent;

    public VegetationPool(GameObject Currentprefab, int initialSize, Transform Currentparent)
    {
        prefab = Currentprefab;
        parent = Currentparent;
        inactivePool = new Stack<GameObject>(initialSize);

        // Pre-warm pool in memory to avoid instantiation hit during gameplay
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.SetActive(false);
            inactivePool.Push(obj);
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject obj = inactivePool.Count > 0 ? inactivePool.Pop() : Object.Instantiate(prefab, parent);

        Transform t = obj.transform;
        t.SetPositionAndRotation(position, rotation);
        t.localScale = scale;

        obj.SetActive(true);
        return obj;
    }

    public void Release(GameObject obj)
    {
        obj.SetActive(false);
        inactivePool.Push(obj);
    }
}