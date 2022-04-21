using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SwordBullet : BulletLowSpeed
{
    protected override void Hit(Collider2D collision)
    {
        base.Hit(collision);
    }
}
