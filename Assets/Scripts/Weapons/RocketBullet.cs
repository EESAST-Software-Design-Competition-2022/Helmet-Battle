using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RocketBullet : BulletPhysics
{
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private float blowSpeed;
    [SerializeField]
    private GameObject BlowVFX;
    [SerializeField]
    private GameObject RocketBlow;
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
        var blow = Instantiate(RocketBlow,transform.position,Quaternion.identity);
        blow.GetComponent<RocketBlow>().userID = userID;
        NetworkServer.Spawn(blow);
        RpcSetBlowFX();
        RpcShakeCamera(3f);
        StartCoroutine(DestroyAfterTime(0.1f));
    }
    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        DestroyOnSelf();
    }
    public override void DestroyOnSelf()
    {
        NetworkServer.Destroy(gameObject);
    }
    protected override void Hit(Collider2D collision)
    {

    }
    [ClientRpc]
    void RpcSetBlowFX()
    {
        PoolManager.Release(BlowVFX,transform.position,Quaternion.identity);
        SoundManager.instance.BoomAudio();
    }
}
