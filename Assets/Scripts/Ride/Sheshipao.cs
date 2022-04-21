using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Sheshipao : Ride
{
    public Transform firePoint;
    public GameObject StonePrefab;
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
