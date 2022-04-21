using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BulletPhysics : Bullet
{
    protected Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }
    private void Start()
    {
        rb.isKinematic = false;
        if (rb != null)
        {
            rb.velocity = transform.right * bulletSpeed;
        }
    }
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Hit(collision);
    }
    /// <summary>
    /// 发生碰撞时调用，服务器函数
    /// </summary>
    /// <param name="collision"></param>
    protected override void Hit(Collider2D collision)
    {
        base.Hit(collision);
        if (collision.CompareTag("Player"))
        {
            if (ignoreSelf && collision.GetComponent<NetworkIdentity>().netId == userID)
            {
                return;
            }
            collision.GetComponent<PlayerEvent>().OnHitByBullet(this);
            NetworkServer.Destroy(gameObject);
        }else
        if (collision.CompareTag("Ground"))
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}
