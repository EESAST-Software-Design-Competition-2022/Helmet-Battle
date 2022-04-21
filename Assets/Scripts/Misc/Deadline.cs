using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Deadline : MonoBehaviour
{
    private void Start()
    {
        if (HelmetManage.HelmetManager.instance == null)
        {
            Debug.LogError("Helmet Manager is null!");
        }
    }
    //[ServerCallback]
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.TryGetComponent<HelmetManage.Helmet>(out var helmet))
    //    {
    //        helmet.DestroyOnSelf();
    //        HelmetManage.HelmetManager.instance.SpawnHelmet();
    //    }
    //}
}
