using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
[System.Serializable] public class Pool
{
    public GameObject prefab
    {
        get
        {
            return Prefab;
        } 
    }

    [SerializeField]
    GameObject Prefab;
    [SerializeField]
    int size = 1;

    Transform parent;

    Queue<GameObject> queue;

    public void Initialize(Transform parent)
    {
        this.parent = parent;
        queue = new Queue<GameObject>();
        for(var i = 0; i < size; i++)
        {
            queue.Enqueue(Copy());
        }
    }
    GameObject Copy()
    {
        var copy = GameObject.Instantiate(prefab, parent);

        copy.SetActive(false);

        return copy;
    }
    GameObject AvailableObject()
    {
        GameObject availableObject = null;
        if (queue.Count > 0 && !queue.Peek().activeSelf)
        {
         availableObject = queue.Dequeue();
        }
        else
        {
            availableObject = Copy();
        }

        queue.Enqueue(availableObject);

        return availableObject;
    }

    public GameObject PreparedObject()
    {
        GameObject preparedObject = AvailableObject();

        preparedObject.SetActive(true);

        return preparedObject;
    }
    public GameObject PreparedObject(Vector3 position, Quaternion rotation)
    {
        GameObject preparedObject = AvailableObject();
        preparedObject.transform.position = position;
        preparedObject.transform.rotation = rotation;
        preparedObject.SetActive(true);

        return preparedObject;
    }

    public  GameObject PreparedObject(Vector3 position, Quaternion rotation, Vector3 localScale)
    {
        GameObject preparedObject = AvailableObject();
        preparedObject.transform.position = position;
        preparedObject.transform.rotation = rotation;
        preparedObject.transform.localScale = localScale;

        preparedObject.SetActive(true);

        return preparedObject;
    }
}
