using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CircleTrap : NetworkBehaviour
{
    [SerializeField]
    Vector3 offset;
    [SerializeField]
    float scale = 4;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            
            HitEffect(collision.transform);
        }
    }
    [Server]
    void HitEffect(Transform transform)
    {
        if (!isClient)
        {
            transform.localScale *= scale;
            var rb = transform.GetComponent<Rigidbody2D>();
            var normal = (Vector2)(transform.position - this.transform.position).normalized;
            rb.velocity = Vector2.Reflect(rb.velocity, normal);
        }
        RpcHitEffect(transform.GetComponent<NetworkIdentity>().netId);
    }
    [ClientRpc]
    void RpcHitEffect(uint netId)
    {
        NetworkClient.spawned.TryGetValue(netId,out NetworkIdentity identity);

        if(identity != null)
        {
            var transform = identity.GetComponent<Transform>();
            transform.localScale *= scale;
            var rb = identity.GetComponent<Rigidbody2D>();
            var normal = (Vector2)(transform.position - this.transform.position).normalized;
            rb.velocity = Vector2.Reflect(rb.velocity, normal);
        }

    }
}
