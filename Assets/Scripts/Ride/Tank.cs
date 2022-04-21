using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Tank : Ride
{
    public Transform FirePoint;
    public BoxCollider2D hitCollider;
    public GameObject TankBulletPrefab;
    public float CoolTime;
    float CoolTimer;
    public override void ReleaseSkill()
    {
        if(Time.time > CoolTimer)
        {
            rider.CmdRideAttack();
            CoolTimer = Time.time + CoolTime;
        }
    }
}
