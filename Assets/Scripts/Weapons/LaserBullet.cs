using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LaserBullet : BulletHighSpeed
{
    protected override void Hit(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (ignoreSelf && collision.GetComponent<NetworkIdentity>().netId == userID)
            {
                return;
            }
            collision.GetComponent<PlayerEvent>().OnHitByBullet(this);
        }
    }
}
