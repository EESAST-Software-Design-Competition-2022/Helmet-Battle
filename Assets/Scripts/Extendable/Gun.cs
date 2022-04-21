using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Gun : Weapon
{
    [SerializeField]
    protected Transform firePos;
    [SerializeField]
    protected GameObject bullet;
    [SerializeField]
    protected bool isInfiniteBullet;
    [SerializeField]
    protected int bulletCount;
    [SerializeField]
    protected float coolDownDuration;

    protected float coolDownTime;

    protected void Awake()
    {
        coolDownTime = 0;
    }
    [Server]
    public override void Attack(WeaponID weaponID)
    {
        switch (weaponID)
        {
            default:
                if (Time.time > coolDownTime)
                {
                    if (isInfiniteBullet || bulletCount > 0)
                    {
                        GameObject bul = Instantiate(bullet, firePos.position, transform.rotation);
                        bul.GetComponent<Bullet>().userID = userID;
                        user.GetComponent<PlayerEvent>().OnWeaponAttack(this);
                        NetworkServer.Spawn(bul);

                        coolDownTime = Time.time + coolDownDuration;
                        if (!isInfiniteBullet)
                        {
                            bulletCount--;
                        }
                    }
                    else //Ïú»ÙÎäÆ÷
                    {
                        user.GetComponent<PlayerController>().SetWeaponID(WeaponID.HandAttack);
                    }
                }
                break;
        }
    }
}
