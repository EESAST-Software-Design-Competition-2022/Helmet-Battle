using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RocketBlow : BulletLowSpeed
{

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        StartCoroutine(DestroyAfterAWhile());
    }
    
    IEnumerator DestroyAfterAWhile()
    {

        yield return new WaitForSeconds(bulletLife);
        NetworkServer.Destroy(gameObject);
    }
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
            if (ignoreSelf && collision.GetComponent<NetworkIdentity>().connectionToClient.connectionId == userID)
            {
                return;
            }
            collision.GetComponent<PlayerEvent>().OnHitByBullet(this);
        }
    }
}
