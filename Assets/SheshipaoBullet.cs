using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class SheshipaoBullet : BulletLowSpeed
{
    protected override void Hit(Collider2D collision)
    {
        if (breakOtherBullet)
        {
            if (collision.CompareTag("Bullet"))
            {
                collision.GetComponent<Bullet>().DestroyOnSelf();
            }
        }
        if (collision.CompareTag("Player"))
        {
            if (ignoreSelf && collision.TryGetComponent<NetworkIdentity>(out var neti) && neti.netId == userID)
            {
                return;
            }
            collision.GetComponent<PlayerEvent>().OnHitByBullet(this);
        }
    }
}
