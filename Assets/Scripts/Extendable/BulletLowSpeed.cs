using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BulletLowSpeed : Bullet
{
    // Update is called once per frame
    void Update()
    {
        transform.Translate(bulletSpeed * Time.deltaTime, 0, 0);
    }
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Hit(collision);
    }
    [Server]
    protected override void Hit(Collider2D collision)
    {
        base.Hit(collision);
        if (collision.CompareTag("Player"))
        {
            if (ignoreSelf && collision.TryGetComponent<NetworkIdentity>(out var neti) && neti.netId == userID)
            {
                return;
            }
            collision.GetComponent<PlayerEvent>().OnHitByBullet(this);
            DestroyOnSelf();
        }
        else if (collision.CompareTag("Ground"))
        {
            //if(collision.transform.root.TryGetComponent<GridMove>(out var grid))
            //{
            //    grid.OnBulletHit(collision.ClosestPoint(transform.position));
            //}
            DestroyOnSelf();
        }
    }
}
