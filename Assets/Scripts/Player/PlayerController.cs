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
    //��������
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
    //��Ч
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
    //��ʶplayer״̬�������л��������˶����������ȡ����״̬
    [HideInInspector]
    public bool isPaused = false;
    //���ڿͻ����ж�����״̬����������������
    bool jumpPressed;
    bool isJump;
    bool crouchPressed;
    bool isCrouch;
    public float faceDirection { get; private set; } //�������ҵķ���ȡ-1,0,1
    int jumpCount;//��¼ʣ����Ծ������ʵ�ֶ����
    //���ɷ��������ã��ͻ��˲�Ӧ������
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
    //��ȡʱʹ��GetWeaponID,�޸�ʱʹ��SetWeaponID
    WeaponID activeID;
    //Weapon�޷���SyncVarͬ������OnWeaponIDChanged��֤ͬ��
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
    //��ʼ��
    #region
    private void Start()
    {
        actualSpeed = speed;
        actualJumpSpeed = jumpSpeed;
        actualScoreSpeed = ScoreSpeed;
        health = healthMax;
        SetWeaponID(activeID); //�ͻ��˺ͷ�������ʼ������Inspector�����õ�ID��Ӧ��������ӵ��������
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

        //�����Ƿ���ͣ
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
            if (HelmetManager.instance != null && HasHelmet)  //��������Helmet
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
        //������ҳ�ʼ��
        hand.GetComponent<LookAt>().enabled = true;
        rb.isKinematic = false;

        UIController.instance.localPlayer = this;
    }

    void Awake()
    {
        //�򵥵İ�
        playerIdentity = GetComponent<NetworkIdentity>();
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;

    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //ʵʱˢ��
    #region
    void FixedUpdate()
    {
        if (isServer)
        {

            ScoreCount();

        }
        if (isLocalPlayer) //��Ȼ�����˶����ڱ���ִ�У�������Ϊ��NetworkTransform�����
                           //�ͻ���Player��position��rotation�ᱻͬ�����������Լ������������
        {
            //����ж�Ҫд��Jump֮ǰ(����д��FixedStatusDetect����)�����ж��Ƿ񴥵����ж���Ծ������������Ծ�������һ��
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
            healthBar.text = new string('-', health); //ʵʱ��ʾ����ֵ
            //scoreText.text = score.ToString();//ʵʱ��ʾ����ֵ
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
        //����Ϊ��ǰ��ɫ
        Head.GetComponent<SpriteRenderer>().color = colorRender;
        Body.GetComponent<SpriteRenderer>().color = colorRender;
        LegLeft.GetComponent<SpriteRenderer>().color = colorRender;
        LegRight.GetComponent<SpriteRenderer>().color = colorRender;
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //�ͻ��˲���
    #region
    /// <summary>
    /// �������״̬�����뷢���ڿͻ���
    /// </summary>
    void InputDetect()
    {
        //����
        faceDirection = Input.GetAxisRaw("Horizontal");
        //��Ծ������⣬��ΪgetButtonDown,getKeyDown��ÿһ֡����һ�Σ���������Ӧ��д��Update��
        jumpPressed = Input.GetButtonDown("Jump");
        //���м��
        crouchPressed = Input.GetButton("Crouch");
        if (jumpPressed && jumpCount > 0) //���������Ծ������
        {
            isJump = true;
        }
        //jump��һ���Զ�����crouch�ǳ����Զ��������Դ���ʽ��ͬ��ͬʱ��crouch������ͷ���ϰ������������Ļ���
        if (isCrouch) //����Ѿ�crouch
        {
            isCrouch = isTouchingGround && (crouchPressed || isTouchingHead); //�ڵ����ϣ��Ұ��°�����ͷ������������һ
        }
        else //���δ����crouch
        {
            isCrouch = isTouchingGround && crouchPressed;  //�ڵ����ϣ��Ұ��°���
        }
    }
    /// <summary>
    /// ���FixedUpdate�е�״̬(����������)
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
    /// ���ｫ����jump�з����Ŀ���Ǳ�֤�����Ƿ�deprivedControl����ײ��ⶼ�ܽ���(�ͻ��˺���)
    /// </summary>
    void ClientStatusDetect()
    {
        if (isTouchingGround) //����ڵ�����
        {
            jumpCount = 2;
        }
    }
    /// <summary>
    /// JumpӦ�÷���FixedUpdate�У���Ϊ�漰�������˶�����FixedUpdate������ϵͳ��Ƶ����ͬ����
    /// </summary>
    void Jump()
    {
        //��Ծ
        if (isJump)
        {
            if (jumpCount > 0)  //�����Ծ��������0
            {
                rb.velocity = new Vector2(rb.velocity.x, actualJumpSpeed);
                jumpCount--;
                //��isJump����Ϊfalse����ʾ��Update�м�⵽�İ����Ѿ���FixedUpdate�д������
                isJump = false;
            }
        }
    }
    /// <summary>
    /// crouchҲ����FixedUpdate��
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
    /// ����ƶ�
    /// </summary>
    void Move()
    {
        rb.AddForce(Vector2.right * (faceDirection * actualSpeed - rb.velocity.x) * moveForceMultiple);
        //rb.velocity = new Vector2(faceDirection * speed, rb.velocity.y);
    }
    /// <summary>
    /// ����������������Щ�����ǿͻ��˷��𣬷�����ִ�У���������е���Cmd����
    /// </summary>
    void RemoteControl()
    {
        //����
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
        //�л�����
        if (Input.GetButtonDown("SwitchWeapon"))
        {
            SwitchWeapon();
        }
        //�л�����
        if (Input.GetButtonDown("SwitchRide"))
        {
            SwitchRide();
        }
        //���＼��
        if (Input.GetButtonDown("RideSkill"))
        {
            RideSkill();
        }
    }
    /// <summary>
    /// ���Ŷ�����Ҳ�����ڱ��أ�������NetworkAnimator�����֤���������������Ҳ�ܿ���һ���Ķ���
    /// </summary>
    void SwitchAnim()
    {
        //����
        if (faceDirection != 0f)
        {
            man.localScale = new Vector3(-faceDirection, 1, 1);
        }

        //�ܶ�
        anim.SetFloat("running", Mathf.Abs(faceDirection));
        //��Ծ
        if (isTouchingGround)  //�Ӵ�����
        {
            anim.SetBool("jumping", false);
            anim.SetBool("falling", false);
        }
        else if (rb.velocity.y > 0.1f) //�ٶ�����
        {
            anim.SetBool("falling", false);
            anim.SetBool("jumping", true);
        }
        else if (rb.velocity.y < -0.1f) //�ٶ�����
        {
            anim.SetBool("falling", true);
            anim.SetBool("jumping", false);
        }
        //���У���Ϊ���б�״̬�����Կ���ֱ������
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
    //�ͻ��˲���������
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
    /// �ӿͻ����޸ķ�������WeaponIDʱ���ô˺���
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
    //�����������ͻ���
    #region
    /// <summary>
    /// �������󶨵����У�Ϊ�˽�ʡ����������д����Ҫ�ڱ�ĵط����ã�����ߺ������������
    /// </summary>
    /// <param name="weaponPrefab"></param>
    void CollectWeapon(GameObject weaponPrefab)
    {
        hand.transform.rotation = Quaternion.identity;//������ת����

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
    /// ���ԭ��������������CollectWeapon�����µ�����
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
    /// ��Server��WeaponIDͬ����Client������ChangeWeapon
    /// </summary>
    /// <param name="oldID"></param>
    /// <param name="newID"></param>
    public void OnWeaponIDChanged(WeaponID oldID, WeaponID newID)
    {
        SoundManager.instance.ChangeWeaponAudio();
        StartCoroutine(ChangeWeapon(newID));
    }
    /// <summary>
    /// ���õ�ǰ����ٶ�(�����ڷ�������ͻ���ͨѶ��ʹ��ʱ����SetPlayerVelocity����)
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
    /// ��isHurt�޸�Ϊtrue������HurtTime����޸�Ϊfalse(����������)
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
    /// ��isHurt�޸�Ϊtrue������0.1����޸�Ϊfalse
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
    //����������
    #region
    [ServerCallback]
    public void SetReady(bool state)
    {
        isReady = state;
        SceneScript.instance.TryToStartGame();
    }
    /// <summary>
    /// �������޸�����IDʱ���ô˺���
    /// </summary>
    /// <param name="id"></param>
    public void SetWeaponID(WeaponID id)
    {
        activeID = id;
        StartCoroutine(ChangeWeapon(id));  //�޸ķ�����������
    }
    /// <summary>
    /// ���ô�����ٶȣ�����0.1��
    /// </summary>
    /// <param name="v">�ٶ�</param>
    public void SetPlayerVelocity(Vector2 v)
    {
        if (playerRideSystem.RideId == RideID.Null)
        {
            TargetDepriveControl();
            TargetSetPlayerVelocity(v);
        }
    }
    /// <summary>
    /// ���ô�����ٶȣ�����time��
    /// </summary>
    /// <param name="v">�ٶ�</param>
    /// <param name="time">�����¼�</param>
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
            //����������,ͷ���ص�ԭλ
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
    //����
    #region
    /// <summary>
    /// �������ļ��У���Ҫ��ȡactiveID�͵����������
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