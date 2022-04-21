using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BulletHighSpeed : Bullet
{
    public float highSpeedDistance;
    bool isHit;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(bulletSpeed * Time.deltaTime, 0, 0);
        if (isServer)
        {
            var hit = Physics2D.Raycast(transform.position, transform.right, highSpeedDistance);
            if (hit.collider != null)
            {
                Hit(hit.collider);
            }
        }
    }
    protected override void Hit(Collider2D collision)
    {
        base.Hit(collision);
        if (!isHit)
        {
            if (collision.CompareTag("Player"))
            {
                if (ignoreSelf && collision.GetComponent<NetworkIdentity>().netId == userID)
                {
                    return;
                }
                collision.GetComponent<PlayerEvent>().OnHitByBullet(this);
                StartCoroutine(DestoryAfterAWhile(0.1f));
            }
            else if (collision.CompareTag("Ground"))
            {
                StartCoroutine(DestoryAfterAWhile(0.1f));
            }
        }
    }
    IEnumerator DestoryAfterAWhile(float t)
    {
        isHit = true;
        yield return new WaitForSeconds(t);
        DestroyOnSelf();
    }
}
