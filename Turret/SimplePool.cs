using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool cực kỳ đơn giản cho prefab. Nếu không muốn pool, thay pool.Get() bằng Instantiate.
/// </summary>
public class SimplePool
{
    private GameObject prefab;
    private Transform parent;
    private Queue<GameObject> pool = new Queue<GameObject>();

    public SimplePool(GameObject prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < initialSize; i++)
        {
            var go = Object.Instantiate(prefab, parent);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            var go = pool.Dequeue();
            if (go == null)
            {
                // nếu bị null (bị destroy), tạo mới
                go = Object.Instantiate(prefab, parent);
            }
            return go;
        }
        else
        {
            var go = Object.Instantiate(prefab, parent);
            go.SetActive(false);
            return go;
        }
    }

    public void Return(GameObject go)
    {
        go.SetActive(false);
        pool.Enqueue(go);
    }
}
