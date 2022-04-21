using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LightSwordBullet : BulletLowSpeed
{
    protected override void Hit(Collider2D collision)
    {
        if (breakOtherBullet)
        {
            //if (collision.CompareTag("Bullet"))
            //{
            //    var bul = collision.GetComponent<Bullet>();
            //    uint id = bul.userID;
            //    bul.DestroyOnSelf();
            //    if(SceneScript.instance.players.ContainsKey(id) && SceneScript.instance.players.ContainsKey(userID))
            //    {
            //        SceneScript.instance.players[userID].TargetSetPosition(SceneScript.instance.players[id].transform.position + Vector3.up * 2);
            //    }
            //}
        }
            if (collision.CompareTag("Player"))
            {
                if (ignoreSelf && collision.GetComponent<NetworkIdentity>().netId == userID)
                {
                    return;
                }
                collision.GetComponent<PlayerEvent>().OnHitByBullet(this);
            if (SceneScript.instance.players.ContainsKey(userID))
            {
                SceneScript.instance.players[userID].TargetSetPosition(collision.transform.position + Vector3.up * 2);
            }
            }
    }
}
