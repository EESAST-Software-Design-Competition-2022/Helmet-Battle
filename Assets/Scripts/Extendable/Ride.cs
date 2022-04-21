using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HelmetManage;
/*
 * 问题：子弹碰撞检测(get)
 */
public class Ride : MonoBehaviour
{
    public Transform RidePoint;
    public float height;
    public Transform[] GroundPoints;
    public int Health;
    public float CoolDownTime;
    [HideInInspector]
    public PlayerController rider;
    public float speed;
    public float jumpSpeed;
    public Transform collidePosition;
    public Vector3 RideLocalPosition()
    {
        return RidePoint.localPosition;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnHit(collision);
    }
    private void OnDestroy()
    {
        if (rider != null)
        {
            rider.playerRideSystem.OnRideDestroy();
        }
    }
    public virtual void SetEffect(bool state)
    {
        if (state)
        {
            rider.actualSpeed = speed;
            rider.actualJumpSpeed = jumpSpeed;
        }
        else
        {
            rider.actualSpeed = rider.speed;
            rider.actualJumpSpeed = rider.jumpSpeed;
        }
    }
    //客户端调用
    public virtual void ReleaseSkill()
    {

    }
    public virtual void OnHit(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (transform.root.TryGetComponent<PlayerRideSystem>(out var rideSystem))
            {
                rideSystem.OnHitByBullet(collision.GetComponent<Bullet>());
            }
        }
        else if (collision.TryGetComponent<Helmet>(out var helmet))
        {
            if (transform.root.TryGetComponent<PlayerEvent>(out var playerEvent))
            {
                playerEvent.OnHelmetCollect(helmet);
            }
        }
    }
}
