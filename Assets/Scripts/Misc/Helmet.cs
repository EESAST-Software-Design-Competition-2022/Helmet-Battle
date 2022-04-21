using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
namespace HelmetManage {
public class Helmet : NetworkBehaviour
{
    private void Awake()
    {
        GetComponent<Rigidbody2D>().isKinematic = true;
    }
    public override void OnStartServer()
    {
            GetComponent<Rigidbody2D>().isKinematic = false;
    }

    [Server]
    public void DestroyOnSelf()
    {
        NetworkServer.Destroy(gameObject);
    }
        [ServerCallback]
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Deadline"))
            {
                HelmetManager.instance.SpawnHelmet();
                DestroyOnSelf();
            }
        }
    }
}