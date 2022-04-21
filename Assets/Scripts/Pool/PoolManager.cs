using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField]
    Pool[] playerProjectilePools;

    static Dictionary<GameObject, Pool> dictionary;

    void Start()
    {
        dictionary = new Dictionary<GameObject, Pool>();
        Initialize(playerProjectilePools);
    }
    void Initialize(Pool[] pools)
    {
        foreach(var pool in pools)
        {
#if UNITY_EDITOR
            if (dictionary.ContainsKey(pool.prefab))
            {
                Debug.LogError("Same prefab in multiple pools! Prefab:" + pool.prefab.name);
                
                continue;
            }
#endif
            dictionary.Add(pool.prefab, pool);

            Transform poolParent =  new GameObject("Pool:" + pool.prefab.name).transform;
            
            poolParent.parent = transform;
            pool.Initialize(poolParent);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns>
    /// <para></para>
    /// </returns>
    public static GameObject Release(GameObject prefab)
    {
#if UNITY_EDITOR
        if (!dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager could not find the prefab:" + prefab.name);
            return null;
        }
#endif
        return dictionary[prefab].PreparedObject();
    }
    public static GameObject Release(GameObject prefab,Vector3 position,Quaternion rotation)
    {
#if UNITY_EDITOR
        if (!dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager could not find the prefab:" + prefab.name);
            return null;
        }
#endif
        return dictionary[prefab].PreparedObject(position,rotation);
    }
    public static GameObject Release(GameObject prefab, Vector3 position, Quaternion rotation,Vector3 localScale)
    {
#if UNITY_EDITOR
        if (!dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager could not find the prefab:" + prefab.name);
            return null;
        }
#endif
        return dictionary[prefab].PreparedObject(position, rotation,localScale);
    }
    public static T Release<T>(T original) where T : MonoBehaviour
    {
#if UNITY_EDITOR
        if (!dictionary.ContainsKey(original.gameObject))
        {
            Debug.LogError("Pool Manager could not find the prefab:" + original.gameObject.name);
            return null;
        }
#endif
        return dictionary[original.gameObject].PreparedObject().GetComponent<T>();
    }
    public static T Release<T>(T original, Vector3 position, Quaternion rotation) where T : MonoBehaviour
    {
#if UNITY_EDITOR
        if (!dictionary.ContainsKey(original.gameObject))
        {
            Debug.LogError("Pool Manager could not find the prefab:" + original.name);
            return null;
        }
#endif
        return dictionary[original.gameObject].PreparedObject(position, rotation).GetComponent<T>();
    }
    public static T Release<T>(T original, Vector3 position, Quaternion rotation, Vector3 localScale) where T: MonoBehaviour
    {
#if UNITY_EDITOR
        if (!dictionary.ContainsKey(original.gameObject))
        {
            Debug.LogError("Pool Manager could not find the prefab:" + original.name);
            return null;
        }
#endif
        return dictionary[original.gameObject].PreparedObject(position, rotation, localScale).GetComponent<T>();
    }
}
