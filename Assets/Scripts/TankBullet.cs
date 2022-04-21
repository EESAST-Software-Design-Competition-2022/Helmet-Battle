using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TankBullet : BulletPhysics
{
    [SerializeField]
    private float blowSpeed;
    [SerializeField]
    private GameObject BlowVFX;
    [SerializeField]
    private GameObject PowderBlow;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = false;
        if (rb != null)
        {
            rb.velocity = transform.right * bulletSpeed;
        }
    }

    public override void OnStartServer()
    {
        Invoke(nameof(Blow), bulletLife);
    }
    [Server]
    void Blow()
    {
        var blow = Instantiate(PowderBlow, transform.position, Quaternion.identity);
        blow.GetComponent<Bullet>().userID = userID;
        NetworkServer.Spawn(blow);
        RpcSetBlowFX();
        StartCoroutine(DestroyAfterTime(0.1f));
    }
    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        DestroyOnSelf();
    }
    [Server]
    protected override void Hit(Collider2D collision)
    {
        if(collision.CompareTag("Player") || collision.CompareTag("Ride") || collision.CompareTag("Ground"))
        {
            Blow();
        }
    }
    [ClientRpc]
    void RpcSetBlowFX()
    {
        SoundManager.instance.BoomAudio();
        if (BlowVFX != null)
            PoolManager.Release(BlowVFX, transform.position, Quaternion.identity);
    }
}
