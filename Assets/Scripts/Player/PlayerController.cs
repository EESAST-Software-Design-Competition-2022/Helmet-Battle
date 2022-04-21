using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;
using HelmetManage;
public enum Color
{
    white,
    red,
    blue,
    green,
    yellow
}

public class PlayerController : NetworkBehaviour
{

    //-----------------------------------------------------------------------------------------------------
    //变量定义
    #region
    public GameObject hand;
    public Transform ceiling;
    [SerializeField]
    public float speed;
    [HideInInspector]
    public float actualSpeed;
    public float moveForceMultiple;
    [SerializeField]
    public float jumpSpeed;
    [HideInInspector]
    public float actualJumpSpeed;
    public int healthMax;
    public LayerMask ground;
    public Transform bottom;
    public TextMesh healthBar;
    public TextMesh respawnTimeText;
    public Transform man;
    public Animator anim;
    public Collider2D standing;
    public Collider2D sitting;
    //特效
    public GameObject HitVFX;
    public GameObject DeathVFX;

    public GameObject Head;
    public GameObject Body;
    public GameObject LegLeft;
    public GameObject LegRight;
    public GameObject Helmet;
    public float HelmetCooltime;
    float helmetCoolTimer;
    [HideInInspector]
    public NetworkIdentity playerIdentity;
    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public PlayerEvent playerEvent;
    [HideInInspector]
    public PlayerRideSystem playerRideSystem;
    [HideInInspector]
    public PlayerLevelSystem playerLevelSystem;
    //标识player状态，动画切换、物理运动均从这里获取人物状态
    [HideInInspector]
    public bool isPaused = false;
    //用于客户端判断自身状态，服务器不能设置
    bool jumpPressed;
    bool isJump;
    bool crouchPressed;
    bool isCrouch;
    public float faceDirection { get; private set; } //按键左右的方向，取-1,0,1
    int jumpCount;//记录剩余跳跃次数，实现多段跳
    //仅由服务器设置，客户端不应该设置
    bool isControlledByLocalPlayer = true;
    public float respawnTime = 3f;
    [SyncVar(hook = nameof(OnColorChanged))]
    [HideInInspector]
    public Color color = Color.white;
    [SyncVar]
    [HideInInspector]
    public bool isDead = false;
    [HideInInspector]
    bool isReady = false;
    public bool IsReady => isReady;
    [SyncVar]
    [HideInInspector]
    public int health;
    [HideInInspector]
    public bool isTouchingGround;
    [HideInInspector]
    public bool isTouchingHead;
    //attack
    [SyncVar(hook = nameof(OnWeaponIDChanged))]
    //获取时使用GetWeaponID,修改时使用SetWeaponID
    WeaponID activeID;
    //Weapon无法用SyncVar同步，由OnWeaponIDChanged保证同步
    public Weapon weapon { get; private set; }
    [SyncVar(hook = nameof(OnHelmetChanged))]
    bool hasHelmet = false;
    public bool HasHelmet { get { return hasHelmet; } set { hasHelmet = value; if (value) { helmetCoolTimer = Time.time + HelmetCooltime; } } }
    [HideInInspector]
    [SyncVar]
    public float score = 0f;
    [SerializeField]
    public float ScoreSpeed { get; private set; } = 1;
    [HideInInspector]
    public float actualScoreSpeed;
    [HideInInspector]
    [SyncVar(hook = nameof(OnRespawnTextChanged))]
    public string respawnText = "";
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //初始化
    #region
    private void Start()
    {
        actualSpeed = speed;
        actualJumpSpeed = jumpSpeed;
        actualScoreSpeed = ScoreSpeed;
        health = healthMax;
        SetWeaponID(activeID); //客户端和服务器初始化，将Inspector中设置的ID对应的武器添加到玩家手中
        Helmet.SetActive(HasHelmet);
        if (SceneManager.GetActiveScene().name != "LobbyOffline")
        {
            if (!SceneScript.instance.players.ContainsKey(playerIdentity.netId))
            {
                SceneScript.instance.players.Add(playerIdentity.netId, this);
            }
            if (isLocalPlayer)
            {
                rb.isKinematic = false;
                CmdDistributeColor(playerIdentity.netId);
            }
            else
            {
                rb.isKinematic = true;
            }
        }

        //设置是否暂停
        if(UIController.instance.PanelOptions.activeSelf || UIController.instance.PanelMenu.activeSelf)
        {
            isPaused = true;
        }
    }

    private void OnDestroy()
    {
        if (SceneManager.GetActiveScene().name != "LobbyOffline" && SceneScript.instance != null)
        {
            if (SceneScript.instance.players.ContainsKey(playerIdentity.netId))
            {
                SceneScript.instance.players.Remove(netId);
                if (isServer && NetworkServer.active)
                {
                    SceneScript.instance.UpdateAliveNumber();
                }
            }
            if (HelmetManager.instance != null && HasHelmet)  //重新生成Helmet
            {
                if (isServer && NetworkServer.active)
                {
                    HelmetManager.instance.SpawnHelmet();
                }
            }
        }

    }
    public override void OnStartServer()
    {

    }
    public override void OnStartLocalPlayer()
    {
        //本地玩家初始化
        hand.GetComponent<LookAt>().enabled = true;
        rb.isKinematic = false;

        UIController.instance.localPlayer = this;
    }

    void Awake()
    {
        //简单的绑定
        playerIdentity = GetComponent<NetworkIdentity>();
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;

    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //实时刷新
    #region
    void FixedUpdate()
    {
        if (isServer)
        {

            ScoreCount();

        }
        if (isLocalPlayer) //虽然人物运动都在本地执行，但是因为有NetworkTransform组件，
                           //客户端Player的position和rotation会被同步到服务器以及其他玩家那里
        {
            //落地判定要写在Jump之前(这里写在FixedStatusDetect里了)，先判断是否触地再判断跳跃次数，否则跳跃次数会多一次
            FixedStatusDetect();
            ClientStatusDetect();
            if (isControlledByLocalPlayer)
            {
                Jump();
                Crouch();
                Move();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (health >= 0)
        {
            healthBar.text = new string('-', health); //实时显示生命值
            //scoreText.text = score.ToString();//实时显示分数值
        }
        if (isLocalPlayer)
        {
            if (!isPaused && !isDead && Time.timeScale > 0)
            {
                InputDetect();
                if (isControlledByLocalPlayer)
                {
                    RemoteControl();
                }
            }
            SwitchAnim();
        }
    }
    public void SetColor(Color color)
    {
        UnityEngine.Color colorRender;
        switch (color)
        {
            case Color.blue:
                colorRender = new UnityEngine.Color(0.4588236f, 0.4588236f, 0.9960785f);
                break;
            case Color.red:
                colorRender = new UnityEngine.Color(0.9960785f, 0.4588236f, 0.4588236f);
                break;
            case Color.yellow:
                colorRender = UnityEngine.Color.yellow;
                break;
            case Color.green:
                colorRender = new UnityEngine.Color(0.4588236f, 0.9960785f, 0.4588236f);
                break;
            default:
                colorRender = UnityEngine.Color.white;
                break;
        }
        //设置为当前颜色
        Head.GetComponent<SpriteRenderer>().color = colorRender;
        Body.GetComponent<SpriteRenderer>().color = colorRender;
        LegLeft.GetComponent<SpriteRenderer>().color = colorRender;
        LegRight.GetComponent<SpriteRenderer>().color = colorRender;
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //客户端操作
    #region
    /// <summary>
    /// 检测输入状态，输入发生在客户端
    /// </summary>
    void InputDetect()
    {
        //朝向
        faceDirection = Input.GetAxisRaw("Horizontal");
        //跳跃按键检测，因为getButtonDown,getKeyDown是每一帧调用一次，所以这里应该写在Update中
        jumpPressed = Input.GetButtonDown("Jump");
        //爬行检测
        crouchPressed = Input.GetButton("Crouch");
        if (jumpPressed && jumpCount > 0) //如果符合跳跃的条件
        {
            isJump = true;
        }
        //jump是一次性动作，crouch是持续性动作，所以处理方式不同；同时，crouch有遇到头顶障碍物则“锁定”的机制
        if (isCrouch) //如果已经crouch
        {
            isCrouch = isTouchingGround && (crouchPressed || isTouchingHead); //在地面上，且按下按键和头顶有物满足其一
        }
        else //如果未进入crouch
        {
            isCrouch = isTouchingGround && crouchPressed;  //在地面上，且按下按键
        }
    }
    /// <summary>
    /// 检测FixedUpdate中的状态(服务器函数)
    /// </summary>
    void FixedStatusDetect()
    {
        if (playerRideSystem.isRideActive)
        {
            isTouchingGround = false;
            foreach (var g in playerRideSystem.Ride.GroundPoints)
            {
                if (Physics2D.OverlapCircle(g.position, 0.1f, ground))
                {
                    isTouchingGround = true;
                    break;
                }
            }
        }
        else
        {
            isTouchingGround = Physics2D.OverlapCircle(bottom.position, 0.1f, ground);
            isTouchingHead = Physics2D.OverlapCircle(ceiling.position, 0.1f, ground);
        }

    }
    [Server]
    void ScoreCount()
    {
        if (HasHelmet || (playerRideSystem.RideId == RideID.Itcycle && playerRideSystem.isRideActive))
        {
            score += Time.fixedDeltaTime * actualScoreSpeed;
        }
    }
    /// <summary>
    /// 这里将检测从jump中分离的目的是保证无论是否被deprivedControl，碰撞检测都能进行(客户端函数)
    /// </summary>
    void ClientStatusDetect()
    {
        if (isTouchingGround) //如果在地面上
        {
            jumpCount = 2;
        }
    }
    /// <summary>
    /// Jump应该放在FixedUpdate中，因为涉及到物理运动，而FixedUpdate和物理系统的频率是同步的
    /// </summary>
    void Jump()
    {
        //跳跃
        if (isJump)
        {
            if (jumpCount > 0)  //如果跳跃次数大于0
            {
                rb.velocity = new Vector2(rb.velocity.x, actualJumpSpeed);
                jumpCount--;
                //将isJump设置为false，表示在Update中检测到的按键已经在FixedUpdate中处理完毕
                isJump = false;
            }
        }
    }
    /// <summary>
    /// crouch也放在FixedUpdate中
    /// </summary>
    void Crouch()
    {
        if (isCrouch)
        {
            standing.enabled = false;
            sitting.enabled = true;

        }
        else
        {
            standing.enabled = true;
            sitting.enabled = false;
        }
    }
    /// <summary>
    /// 玩家移动
    /// </summary>
    void Move()
    {
        rb.AddForce(Vector2.right * (faceDirection * actualSpeed - rb.velocity.x) * moveForceMultiple);
        //rb.velocity = new Vector2(faceDirection * speed, rb.velocity.y);
    }
    /// <summary>
    /// 武器丢弃、开火，这些动作是客户端发起，服务器执行，因此在其中调用Cmd函数
    /// </summary>
    void RemoteControl()
    {
        //开火
        if (Input.GetButtonDown("Fire1"))
        {
            if (!weapon.enableLongClick)
            {
                CmdAttack();
            }
        }
        if (Input.GetButton("Fire1"))
        {
            if (weapon.enableLongClick)
            {
                CmdAttack();
            }
        }
        if (Input.GetButtonDown("DesertWeapon"))
        {
            CmdSetWeaponID(WeaponID.HandAttack);
        }
        //切换武器
        if (Input.GetButtonDown("SwitchWeapon"))
        {
            SwitchWeapon();
        }
        //切换坐骑
        if (Input.GetButtonDown("SwitchRide"))
        {
            SwitchRide();
        }
        //坐骑技能
        if (Input.GetButtonDown("RideSkill"))
        {
            RideSkill();
        }
    }
    /// <summary>
    /// 播放动画，也发生在本地，但是有NetworkAnimator组件保证服务器和其他玩家也能看到一样的动画
    /// </summary>
    void SwitchAnim()
    {
        //朝向
        if (faceDirection != 0f)
        {
            man.localScale = new Vector3(-faceDirection, 1, 1);
        }

        //跑动
        anim.SetFloat("running", Mathf.Abs(faceDirection));
        //跳跃
        if (isTouchingGround)  //接触地面
        {
            anim.SetBool("jumping", false);
            anim.SetBool("falling", false);
        }
        else if (rb.velocity.y > 0.1f) //速度向上
        {
            anim.SetBool("falling", false);
            anim.SetBool("jumping", true);
        }
        else if (rb.velocity.y < -0.1f) //速度向下
        {
            anim.SetBool("falling", true);
            anim.SetBool("jumping", false);
        }
        //爬行，因为爬行表状态，所以可以直接设置
        anim.SetBool("crouching", isCrouch);
    }

    void SwitchWeapon()
    {
        switch (playerLevelSystem.level)
        {
            case 0:
                CmdSetWeaponID(WeaponID.Staff);
                break;
            case 1:
                if (activeID == WeaponID.Staff)
                {
                    CmdSetWeaponID(WeaponID.Stone);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Staff);
                }
                break;
            case 2:
                if (activeID == WeaponID.Staff)
                {
                    CmdSetWeaponID(WeaponID.Stone);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Staff);
                }
                break;
            case 3:
                if (activeID == WeaponID.Sword)
                {
                    CmdSetWeaponID(WeaponID.Stone);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Sword);
                }
                break;
            case 4:
                if (activeID == WeaponID.Sword)
                {
                    CmdSetWeaponID(WeaponID.Bow);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Sword);
                }
                break;
            case 5:
                if (activeID == WeaponID.Sword)
                {
                    CmdSetWeaponID(WeaponID.Bow);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Sword);
                }
                break;
            case 6:
                if (activeID == WeaponID.Powder)
                {
                    CmdSetWeaponID(WeaponID.Bow);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Powder);
                }
                break;
            case 7:
                if (activeID == WeaponID.Powder)
                {
                    CmdSetWeaponID(WeaponID.PowderGun);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Powder);
                }
                break;
            case 8:
                if (activeID == WeaponID.Powder)
                {
                    CmdSetWeaponID(WeaponID.PowderGun);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Powder);
                }
                break;
            case 9:
                if (activeID == WeaponID.Pistol)
                {
                    CmdSetWeaponID(WeaponID.PowderGun);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Pistol);
                }
                break;
            case 10:
                if (activeID == WeaponID.Pistol)
                {
                    CmdSetWeaponID(WeaponID.Rocket);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Pistol);
                }
                break;
            case 11:
                if (activeID == WeaponID.Pistol)
                {
                    CmdSetWeaponID(WeaponID.Rocket);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Pistol);
                }
                break;
            case 12:
                if (activeID == WeaponID.Handgrenade)
                {
                    CmdSetWeaponID(WeaponID.Rocket);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Handgrenade);
                }
                break;
            case 13:
                if (activeID == WeaponID.Handgrenade)
                {
                    CmdSetWeaponID(WeaponID.Missile);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Handgrenade);
                }
                break;
            case 14:
                if (activeID == WeaponID.Handgrenade)
                {
                    CmdSetWeaponID(WeaponID.Missile);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.Handgrenade);
                }
                break;
            case 15:
                if (activeID == WeaponID.LightSword)
                {
                    CmdSetWeaponID(WeaponID.Missile);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.LightSword);
                }
                break;
            case 16:
                if (activeID == WeaponID.LightSword)
                {
                    CmdSetWeaponID(WeaponID.Laser);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.LightSword);
                }
                break;
            case 17:
                if (activeID == WeaponID.LightSword)
                {
                    CmdSetWeaponID(WeaponID.Laser);
                }
                else
                {
                    CmdSetWeaponID(WeaponID.LightSword);
                }
                break;
        }
    }
    void RideSkill()
    {
        if(playerRideSystem.RideId != RideID.Null && playerRideSystem.isRideActive)
        {
            playerRideSystem.Ride.ReleaseSkill();
        }
    }
    void SwitchRide()
    {
        if (playerLevelSystem.level >= 0 && playerLevelSystem.level < 2)
        {
            return;
        }
        else if (playerLevelSystem.level >= 2 && playerLevelSystem.level < 5)
        {
            //if (playerRideSystem.RideId == RideID.Horse && playerRideSystem.isRideActive)
            //{
                
            //    return;
            //}
            playerRideSystem.CmdSetRide(RideID.Horse);
        }
        else if (playerLevelSystem.level >= 5 && playerLevelSystem.level < 8)
        {
            //if (playerRideSystem.RideId == RideID.WarHorse && playerRideSystem.isRideActive)
            //{
            //    playerRideSystem.Ride.ReleaseSkill();
            //    return;
            //}
            playerRideSystem.CmdSetRide(RideID.WarHorse);
        }
        else if (playerLevelSystem.level >= 8 && playerLevelSystem.level < 11)
        {

            //if (playerRideSystem.RideId == RideID.Sheshipao && playerRideSystem.isRideActive)
            //{
            //    playerRideSystem.Ride.ReleaseSkill();
            //    return;
            //}
            playerRideSystem.CmdSetRide(RideID.Sheshipao);
        }
        else if (playerLevelSystem.level >= 11 && playerLevelSystem.level < 14)
        {
            //if (playerRideSystem.RideId == RideID.Tank && playerRideSystem.isRideActive)
            //{
            //    playerRideSystem.Ride.ReleaseSkill();
            //    return;
            //}
            playerRideSystem.CmdSetRide(RideID.Tank);
        }
        else if (playerLevelSystem.level >= 14 && playerLevelSystem.level < 17)
        {
            //if (playerRideSystem.RideId == RideID.Itcycle && playerRideSystem.isRideActive)
            //{
            //    playerRideSystem.Ride.ReleaseSkill();
            //    return;
            //}
            playerRideSystem.CmdSetRide(RideID.Itcycle);
        }
        else
        {
            //if (playerRideSystem.RideId == RideID.SpaceShip && playerRideSystem.isRideActive)
            //{
            //    playerRideSystem.Ride.ReleaseSkill();
            //    return;
            //}
            playerRideSystem.CmdSetRide(RideID.SpaceShip);
        }
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //客户端操作服务器
    #region
    [Command]
    void CmdWinGame()
    {
        SceneScript.instance.WinGame(netId);
    }
    [Command]
    void CmdAttack()
    {
        weapon.Attack(activeID);
    }
    [Command]
    public void CmdRideAttack()
    {
        if(playerRideSystem.RideId == RideID.Tank)
        {
            Tank tank = (Tank)playerRideSystem.Ride;
            var bul = Instantiate(tank.TankBulletPrefab, tank.FirePoint.position, Quaternion.identity);
            bul.transform.right = new Vector3(Mathf.Sign(-man.transform.localScale.x), 0, 0);
            NetworkServer.Spawn(bul);
            bul.GetComponent<Bullet>().userID = netId;
        }else if(playerRideSystem.RideId == RideID.Sheshipao)
        {
            Sheshipao s = (Sheshipao)playerRideSystem.Ride;
            var stone = Instantiate(s.StonePrefab, s.firePoint.position, Quaternion.identity);
            stone.transform.right = new Vector3(-s.rider.man.localScale.x, 0, 0);
            stone.GetComponent<Bullet>().userID = s.rider.netId;
            NetworkServer.Spawn(stone);
        }
        
    }
    /// <summary>
    /// 从客户端修改服务器的WeaponID时调用此函数
    /// </summary>
    /// <param name="id"></param>
    [Command]
    public void CmdSetWeaponID(WeaponID id)
    {
        SetWeaponID(id);
    }
    [Command]
    void CmdDistributeColor(uint netId)
    {
        SceneScript.instance.DistributeColor(netId);
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //服务器操作客户端
    #region
    /// <summary>
    /// 将武器绑定到手中，为了节省代码行数而写，不要在别的地方调用（请读者忽略这个函数）
    /// </summary>
    /// <param name="weaponPrefab"></param>
    void CollectWeapon(GameObject weaponPrefab)
    {
        hand.transform.rotation = Quaternion.identity;//重置旋转方向

        var obj = Instantiate(weaponPrefab, hand.transform.position, weaponPrefab.transform.rotation, hand.transform);
        weapon = obj.GetComponent<Weapon>();
        weapon.user = gameObject;
        if (isServer)
        {
            weapon.userID = playerIdentity.netId;
        }
        obj.transform.localPosition = weaponPrefab.transform.localPosition;
    }
    /// <summary>
    /// 清除原来的武器并调用CollectWeapon设置新的武器
    /// </summary>
    /// <param name="newID"></param>
    /// <returns></returns>
    IEnumerator ChangeWeapon(WeaponID newID)
    {
        while (hand.transform.childCount > 0)
        {
            Destroy(hand.transform.GetChild(0).gameObject);
            yield return null;
        }
        CollectWeapon(Data.instance.WeaponPrefabs[(int)newID]);
    }
    /// <summary>
    /// 将Server的WeaponID同步到Client，调用ChangeWeapon
    /// </summary>
    /// <param name="oldID"></param>
    /// <param name="newID"></param>
    public void OnWeaponIDChanged(WeaponID oldID, WeaponID newID)
    {
        SoundManager.instance.ChangeWeaponAudio();
        StartCoroutine(ChangeWeapon(newID));
    }
    /// <summary>
    /// 设置当前玩家速度(仅用于服务器向客户端通讯，使用时请用SetPlayerVelocity代替)
    /// </summary>
    /// <param name="v"></param>
    [TargetRpc]
    void TargetSetPlayerVelocity(Vector2 v)
    {
        if (playerRideSystem.RideId == RideID.Null)
        {
            rb.velocity = v;
        }
    }
    [TargetRpc]
    void TargetAddForce(Vector2 Force)
    {
        rb.AddForce(Force);
    }
    [TargetRpc]
    void TargetAddImpulse(Vector2 impulse)
    {
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }
    /// <summary>
    /// 将isHurt修改为true，经过HurtTime秒后修改为false(服务器函数)
    /// </summary>
    /// <param name="HurtTime"></param>
    [TargetRpc]
    public void TargetDepriveControlForAWhile(float HurtTime)
    {
        StartCoroutine(DepriveControl(HurtTime));
    }
    IEnumerator DepriveControl(float HurtTime)
    {
        isControlledByLocalPlayer = false;
        yield return new WaitForSeconds(HurtTime);
        //while (faceDirection == 0 && !jumpPressed)
        //{
        //    yield return new WaitForEndOfFrame();
        //}
        isControlledByLocalPlayer = true;
    }
    [TargetRpc]
    /// <summary>
    /// 将isHurt修改为true，经过0.1秒后修改为false
    /// </summary>
    /// <param name="HurtTime"></param>
    public void TargetDepriveControl()
    {
        StartCoroutine(DepriveControl());
    }
    public IEnumerator DepriveControl()
    {
        isControlledByLocalPlayer = false;
        yield return new WaitForSeconds(0.1f);
        while (faceDirection == 0 && !jumpPressed)
        {
            yield return new WaitForEndOfFrame();
        }
        isControlledByLocalPlayer = true;
    }
    [ClientRpc]
    public void RpcDieAction()
    {
        if (isLocalPlayer)
        {
            anim.Play("Die");
            rb.velocity = Vector2.zero;
        }
        var deathVFX = PoolManager.Release(DeathVFX, transform.position, Quaternion.FromToRotation(Vector3.right,-transform.position));
        ParticleSystem.MainModule mainModule = deathVFX.GetComponentInChildren<ParticleSystem>().main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(Head.GetComponent<SpriteRenderer>().color);
        SoundManager.instance.DieAudio();
        CameraShake.instance.Shake(1.5f);
    }
    [ClientRpc]
    public void RpcRespawnAction()
    {
        if (isLocalPlayer)
        {
            anim.Play("Idle");
            transform.position = NetworkManager.singleton.GetStartPosition().position;
        }
        //PoolManager.Release(...);
    }
    [TargetRpc]
    public void TargetSetPosition(Vector3 p)
    {
        if (playerRideSystem.RideId == RideID.Null)
        {
            transform.position = p;
        }
    }
    [ClientRpc]
    public void RpcHurtVFX(Vector3 position, Quaternion rotation)
    {
        var hitVFX = PoolManager.Release(HitVFX, position, rotation);
        ParticleSystem.MainModule mainModule = hitVFX.GetComponentInChildren<ParticleSystem>().main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(Head.GetComponent<SpriteRenderer>().color);
        SoundManager.instance.HurtAudio();
    }
    void OnColorChanged(Color oldColor, Color newColor)
    {
        SetColor(newColor);
    }
    void OnHelmetChanged(bool oldState, bool newState)
    {
        if (newState)
        {
            SoundManager.instance.HelmetAudio();
        }
        Helmet.SetActive(newState);
    }
    void OnRespawnTextChanged(string oldText, string newText)
    {
        respawnTimeText.text = newText;
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //服务器操作
    #region
    [ServerCallback]
    public void SetReady(bool state)
    {
        isReady = state;
        SceneScript.instance.TryToStartGame();
    }
    /// <summary>
    /// 服务器修改自身ID时调用此函数
    /// </summary>
    /// <param name="id"></param>
    public void SetWeaponID(WeaponID id)
    {
        activeID = id;
        StartCoroutine(ChangeWeapon(id));  //修改服务器的武器
    }
    /// <summary>
    /// 设置此玩家速度，持续0.1秒
    /// </summary>
    /// <param name="v">速度</param>
    public void SetPlayerVelocity(Vector2 v)
    {
        if (playerRideSystem.RideId == RideID.Null)
        {
            TargetDepriveControl();
            TargetSetPlayerVelocity(v);
        }
    }
    /// <summary>
    /// 设置此玩家速度，持续time秒
    /// </summary>
    /// <param name="v">速度</param>
    /// <param name="time">持续事件</param>
    public void SetPlayerVelocity(Vector2 v, float time)
    {
        if (playerRideSystem.RideId == RideID.Null)
        {
            TargetDepriveControlForAWhile(time);
            TargetSetPlayerVelocity(v);
        }
    }
    public void AddForce(Vector2 Force)
    {
        TargetAddForce(Force);
    }
    public void AddImpulse(Vector2 impulse)
    {
        TargetAddImpulse(impulse);
    }
    [Server]
    public void Hurt(uint BulletUserID)
    {
        if (!isDead)
        {
            if (playerRideSystem.RideId != RideID.Null && playerRideSystem.rideHealth > 0)
            {
                playerRideSystem.RideHurt();
                return;
            }
            health--;
            if (HasHelmet)
            {
                if(Time.time > helmetCoolTimer)
                {
                    HasHelmet = false;
                    if (!SceneScript.instance.players.ContainsKey(BulletUserID))
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("The User of this bullet is missing!");
#endif
                        HasHelmet = false;
                        if (HelmetManager.instance != null)
                        {
                            
                            HelmetManager.instance.SpawnHelmet();
                        }
                    }
                    else
                    {
                        SceneScript.instance.players[BulletUserID].HasHelmet = true;
                    }
                }else
                {
#if UNITY_EDITOR
                    Debug.Log("remaining time to collect: " +( helmetCoolTimer - Time.time));
                        #endif
                }
            }
            if (health <= 0)
            {
                var player = SceneScript.instance.players[BulletUserID];
                if(player.score < score * 0.8f)
                    player.score = score * 0.8f;
                Die();
            }
        }
    }
    [Server]
    public void Die()
    {
        if (!isDead)
        {
            isControlledByLocalPlayer = false;
            isDead = true;
            SetWeaponID(WeaponID.HandAttack);
            if (playerRideSystem.RideId != RideID.Null)
            {
                playerRideSystem.SetRideActive(false);
            }
            RpcDieAction();
            SceneScript.instance.UpdateAliveNumber();
            //死亡则重生,头盔回到原位
            if (HelmetManager.instance != null && HasHelmet)
            {
                HasHelmet = false;
                HelmetManager.instance.SpawnHelmet();
            }
            StartCoroutine(CountSeconds(respawnTime, Respawn));
        }
    }
    [Server]
    public void Respawn()
    {
        isControlledByLocalPlayer = true;
        isDead = false;
        health = healthMax;
        SetWeaponID(WeaponID.HandAttack);
        RpcRespawnAction();
        SceneScript.instance.UpdateAliveNumber();
    }

    #endregion
    //-----------------------------------------------------------------------------------------------------
    //其他
    #region
    /// <summary>
    /// 在其他文件中，想要获取activeID就调用这个方法
    /// </summary>
    /// <returns></returns>
    public WeaponID GetWeaponID()
    {
        return activeID;
    }
    IEnumerator CountSeconds(float StartSeconds, FunctionsWithNoParaAndReturn invokeAfterCountSeconds)
    {
        var wait = new WaitForSeconds(1f);
        while (StartSeconds > 0)
        {
            respawnText = StartSeconds.ToString();
            StartSeconds--;
            yield return wait;
        }
        respawnText = "";
        if (invokeAfterCountSeconds != null)
        {
            invokeAfterCountSeconds();
        }
    }
    //localPlayer

    #endregion
}