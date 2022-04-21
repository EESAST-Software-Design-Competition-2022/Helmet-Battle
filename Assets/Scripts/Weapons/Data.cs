using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//存放ID和武器、子弹的对应关系
public enum WeaponID : byte
{
    HandAttack,
    SquareGun,
    PowderGun,
    Pistol,
    Rocket,
    Knife,
    Staff,
    Stone,
    Bow,
    Sword,
    Handgrenade,
    Missile,
    LightSword,
    Laser,
    Powder
}
public enum BulletID : byte
{
    HandAttackBullet,
    SquareGunBullet,
    PowderGunBullet,
    PistolBullet,
    RocketBullet,
    KnifeBullet,
    StaffBullet,
    StoneBullet,
    BowBullet,
    SwordBullet,
    HandgrenadeBullet,
    MissileBullet,
    LightSwordBullet,
    LaserBullet,
    PowderBullet,
    TankBullet
}

public enum RideID: byte
{
    Null,
    Horse,
    WarHorse,
    Sheshipao,
    Tank,
    Itcycle,
    SpaceShip,
}
public class Data : MonoBehaviour
{
    public GameObject[] WeaponPrefabs;
    public GameObject[] RidePrefabs;
    public static Data instance;
    private void Awake()
    {
        instance = this;   
    }
}
