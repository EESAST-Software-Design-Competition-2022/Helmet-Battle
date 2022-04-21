using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Bullet : NetworkBehaviour
{
    [SerializeField]
    public float bulletSpeed;
    [SerializeField]
    public float bulletLife;
    [Tooltip("对击中玩家的冲量")]
    [SerializeField]
    public float impulse;
    [SerializeField]
    public BulletID bulletID;
    [SerializeField]
    public bool ignoreSelf;
    [SerializeField]
    public bool breakOtherBullet;
    [HideInInspector]
    public uint userID;
    [SerializeField]
    bool ShakeCamera;
    public override void OnStartServer()
    {
        Invoke(nameof(DestroyOnSelf), bulletLife);
    }
    // set velocity for server and client. this way we don't have to sync the
    // position, because both the server and the client simulate it.
    [Server]
    public virtual void DestroyOnSelf()
    {
        if (ShakeCamera)
        {
            RpcShakeCamera(1f);
        }
        NetworkServer.Destroy(gameObject);
    }
    [ClientRpc]
    protected void RpcShakeCamera(float strength)
    {
        CameraShake.instance.Shake(strength);
    }
    [Server]
    protected virtual void Hit(Collider2D collision)
    {
            if (breakOtherBullet)
            {
                if (collision.CompareTag("Bullet"))
                {
                    collision.GetComponent<Bullet>().DestroyOnSelf();
                }
            }
    }
}
