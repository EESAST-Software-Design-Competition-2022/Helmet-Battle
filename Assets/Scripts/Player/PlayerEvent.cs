using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using HelmetManage;
//事件写在这里
public class PlayerEvent : NetworkBehaviour
{
    //变量定义
    #region

    PlayerController playerController;
    //Rigidbody2D rb;

    #endregion
    //-----------------------------------------------------------------------------------------------------
    //初始化
    #region
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerController.playerEvent = this;
        //rb = GetComponent<Rigidbody2D>();
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //客户端
    #region
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //客户端操作服务器
    #region
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //服务器操作客户端
    #region
    [ClientRpc]
    void RpcCameraMove(float strength)
    {
        CameraShake.instance.Shake(strength);
    }
    [ClientRpc]
    void RpcOnAttack(WeaponID weaponID)
    {
        switch (weaponID)
        {
            case WeaponID.HandAttack:
                SoundManager.instance.HandAttackAudio();
                break;
            case WeaponID.Staff:
                SoundManager.instance.StaffAttackAudio();
                break;
            case WeaponID.Stone:
                SoundManager.instance.StoneAttackAudio();
                break;
            case WeaponID.Sword:
                ((Sword)playerController.weapon).anim.Play("Attack_Sword");
                SoundManager.instance.SwordAttackAudio();
                break;
            case WeaponID.Bow:
                SoundManager.instance.BowAttackAudio();
                break;
            case WeaponID.PowderGun:
                SoundManager.instance.FirespearAttackAudio();
                break;
            case WeaponID.Pistol:
                SoundManager.instance.PistolAttackAudio();
                break;
            case WeaponID.Rocket:
                SoundManager.instance.RocketAttackAudio();
                break;
            case WeaponID.Handgrenade:
                SoundManager.instance.HandgrenadeAttackAudio();
                break;
            case WeaponID.Missile:
                SoundManager.instance.MissleAttackAudio();
                break;
            case WeaponID.LightSword:
                ((LightSword)playerController.weapon).anim.Play("Attack_Sword");
                SoundManager.instance.LightSwordAttackAudio();
                break;
            case WeaponID.Laser:
                SoundManager.instance.LaserAttackAudio();
                break;
        }
    }
    [ClientRpc]
    void RpcOnBulletHit(BulletID bulletID,Vector3 position, Vector3 right)
    {
        var vfx = PoolManager.Release(playerController.HitVFX, position, Quaternion.identity);
        vfx.transform.right = right;
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //服务器
    #region
    /// <summary>
    /// 碰撞事件
    /// </summary>
    /// <param name="collision"></param>
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            if (playerController.GetWeaponID() == WeaponID.HandAttack)
            {
                var weapon = collision.GetComponent<SceneWeapon>();
                //拾取武器
                playerController.SetWeaponID(weapon.GetWeaponID());
                weapon.DestroyOnSelf();
            }
        }
        else if (collision.CompareTag("Bullet"))  //接触子弹
        {
            //OnBulletHit(collision.GetComponent<Bullet>());
        }
        else if (collision.CompareTag("Deadline"))     //掉出场景，可根据游戏地图的不同自己设定
        {
            playerController.health = 0;
            playerController.Die();
        }else if (collision.CompareTag("Helmet"))
        {
            OnHelmetCollect(collision.GetComponent<Helmet>());
        }else if (collision.CompareTag("Ride"))
        {
            OnHitByRide(collision.GetComponent<Ride>());
        }
    }
    [ServerCallback]
    public void OnHelmetCollect(Helmet helmet)
    {
        playerController.HasHelmet = true;
        helmet.DestroyOnSelf();
    }
    [Server]
    /// <summary>
    /// 当武器发动攻击时触发该事件
    /// </summary>
    /// <param name="weapon"></param>
    public void OnWeaponAttack(Weapon weapon)
    {
        RpcOnAttack(playerController.GetWeaponID());
        switch (playerController.GetWeaponID())
        {
            case WeaponID.HandAttack:
                playerController.TargetDepriveControlForAWhile(0.1f);
                playerController.AddImpulse((Vector2) weapon.transform.right * -weapon.backForce);
                RpcCameraMove(1f);
                break;
            case WeaponID.Staff:
                playerController.TargetDepriveControlForAWhile(0.1f);
                playerController.AddImpulse((Vector2)weapon.transform.right * -weapon.backForce);
                RpcCameraMove(1f);
                break;
            case WeaponID.Sword:
                playerController.TargetDepriveControlForAWhile(0.1f);
                playerController.AddImpulse((Vector2)weapon.transform.right * -weapon.backForce);
                RpcCameraMove(1f);
                break;
            case WeaponID.SquareGun:
                playerController.TargetDepriveControlForAWhile(0.1f);
                playerController.AddImpulse((Vector2) weapon.transform.right * -weapon.backForce);
                break;
            case WeaponID.Knife:
                playerController.TargetDepriveControlForAWhile(1f);
                playerController.anim.Play("Hide");
                playerController.SetPlayerVelocity((Vector2)weapon.transform.right * -weapon.backForce);
                break;
            case WeaponID.PowderGun:
                playerController.TargetDepriveControl();
                playerController.AddImpulse((Vector2)weapon.transform.right * -weapon.backForce);
                break;
            default:
                playerController.TargetDepriveControlForAWhile(0.1f);
                playerController.AddImpulse((Vector2)weapon.transform.right * -weapon.backForce);
                break;
        }
    }
    [Server]
    /// <summary>
    /// 当被子弹击中时触发该事件
    /// </summary>
    /// <param name="bul"></param>
    public void OnHitByBullet(Bullet bul)
    {
        if (bul.ignoreSelf && bul.userID == playerController.playerIdentity.netId)
        {
            return;
        }
        switch (bul.bulletID) {
            case BulletID.HandAttackBullet:
                playerController.TargetDepriveControl();
                playerController.AddImpulse(bul.transform.right * bul.impulse);
                playerController.Hurt(bul.userID);
                playerController.RpcHurtVFX(transform.position, bul.transform.rotation);
                break;
            case BulletID.SquareGunBullet:
                playerController.TargetDepriveControl();
                playerController.AddImpulse(bul.transform.right * bul.impulse);
                playerController.Hurt(bul.userID);
                playerController.RpcHurtVFX(transform.position, bul.transform.rotation);
                break;
            case BulletID.RocketBullet:
                playerController.TargetDepriveControl();
                playerController.AddImpulse(((Vector2)transform.position - (Vector2)bul.transform.position).normalized * bul.impulse);
                playerController.Hurt(bul.userID);
                playerController.RpcHurtVFX(transform.position, Quaternion.FromToRotation(Vector3.right, ((Vector2)transform.position - (Vector2)bul.transform.position).normalized));

                break;
            case BulletID.TankBullet:
                playerController.TargetDepriveControl();
                playerController.AddImpulse(((Vector2)transform.position - (Vector2)bul.transform.position).normalized * bul.impulse);
                playerController.Hurt(bul.userID);
                playerController.RpcHurtVFX(transform.position, Quaternion.FromToRotation(Vector3.right, ((Vector2)transform.position - (Vector2)bul.transform.position).normalized));

                break;
            case BulletID.PowderBullet:
                playerController.TargetDepriveControl();
                playerController.AddImpulse(((Vector2)transform.position - (Vector2)bul.transform.position).normalized * bul.impulse);
                playerController.Hurt(bul.userID);
                playerController.RpcHurtVFX(transform.position, Quaternion.FromToRotation(Vector3.right, ((Vector2)transform.position - (Vector2)bul.transform.position).normalized));

                break;
            case BulletID.HandgrenadeBullet:
                playerController.TargetDepriveControl();
                playerController.AddImpulse(((Vector2)transform.position - (Vector2)bul.transform.position).normalized * bul.impulse);
                playerController.Hurt(bul.userID);
                playerController.RpcHurtVFX(transform.position, Quaternion.FromToRotation(Vector3.right, ((Vector2)transform.position - (Vector2)bul.transform.position).normalized));
                break;
            case BulletID.StaffBullet:
                playerController.TargetDepriveControl();
                playerController.AddImpulse(bul.transform.right * bul.impulse);
                playerController.Hurt(bul.userID);
                playerController.RpcHurtVFX(transform.position, bul.transform.rotation);
                break;
            case BulletID.StoneBullet:
                playerController.TargetDepriveControl();
                playerController.AddImpulse(((Vector2)transform.position - (Vector2)bul.transform.position).normalized * bul.impulse);
                playerController.Hurt(bul.userID);
                playerController.RpcHurtVFX(transform.position, Quaternion.FromToRotation(Vector3.right, ((Vector2)transform.position - (Vector2)bul.transform.position).normalized));


                break;
            default:
                playerController.Hurt(bul.userID);
                playerController.RpcHurtVFX(transform.position, bul.transform.rotation);
                break;
        }
    }
    [Server]
    public void OnHitByRide(Ride ride)
    {
        Debug.Log("hit by ride");
        if (ride.TryGetComponent<WarHorse>(out var warHorse))
        {
            playerController.TargetDepriveControl();
            playerController.AddImpulse(warHorse.hitImpulse * warHorse.transform.right);
        }
    }
    public void OnBulletHit(Bullet bul)
    {
        switch (bul.bulletID)
        {
            case BulletID.LightSwordBullet:
                break;
        }
    }
    #endregion
}
