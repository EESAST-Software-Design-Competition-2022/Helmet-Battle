using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MissileBullet : BulletLowSpeed
{
    [SyncVar(hook = nameof(OnTargetChanged))]
    uint targetID;
    Transform targetTransform;
    Vector3 targetPosition = Vector3.zero;
    [SerializeField]
    private GameObject BlowVFX;
    [SerializeField]
    private GameObject RocketBlow;
    private void Awake()
    {
        targetID = userID;
    }
    private void FixedUpdate()
    {
        if (isServer)
        {
            foreach(var id in SceneScript.instance.players.Keys)
            {
                if(id != userID)
                {
                    if (!SceneScript.instance.players.ContainsKey(targetID))
                    {
                        targetID = id;
                    }
                    else
                    {
                        if(!SceneScript.instance.players[id].isDead)
                        if (Vector3.Distance(transform.position,SceneScript.instance.players[targetID].transform.position)
                            > Vector3.Distance(transform.position, SceneScript.instance.players[id].transform.position))
                        {
                            targetID = id;
                        }
                }
                }
            }
        }
    }
    [Server]
    void Blow()
    {
        var blow = Instantiate(RocketBlow, transform.position, Quaternion.identity);
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
    [ClientRpc]
    void RpcSetBlowFX()
    {
        PoolManager.Release(BlowVFX, transform.position, Quaternion.identity);
        SoundManager.instance.BoomAudio();
    }
    void Update()
    {
        if (targetTransform != null)
        {
            targetPosition = targetTransform.position;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition,targetPosition, bulletSpeed * Time.deltaTime);
            transform.right = ((targetPosition - transform.position)).normalized;
        }

    }
    void OnTargetChanged(uint oldID,uint newID)
    {
        targetTransform = SceneScript.instance.players[targetID].transform;
    }
    public override void DestroyOnSelf()
    {
        Blow();
        base.DestroyOnSelf();
    }
}
