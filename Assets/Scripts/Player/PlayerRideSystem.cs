using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerRideSystem : NetworkBehaviour
{
    public Transform TransRide;
    [SyncVar(hook = nameof(OnRideChanged))]
    RideID rideId;
    public RideID RideId => rideId;
    Ride ride;
    [HideInInspector]
    public bool isRideActive = false;
    [HideInInspector]
    public bool isCooled = true;
    public Ride Ride
    {
        get
        {
            if (ride == null) { Debug.LogError("trying to approach the ride but it is null."); }
            return ride;
        }
    }
    [HideInInspector]
    [SyncVar]
    public int rideHealth;
    [HideInInspector]
    [SyncVar(hook = nameof(OnRideTextChanged))]
    public string rideText;
    public TextMesh RideText;
    PlayerController playerController;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerController.playerRideSystem = this;
    }
    private void Update()
    {
        if (isRideActive)
        {
            RideText.text = "ride health: " + rideHealth;
        }
        else if(isCooled)
        {
            RideText.text = "";
        }
    }
    void OnRideChanged(RideID oldRideId, RideID newRideId)
    {
        //如果old=new，是否会调用此函数？不会
        SetRide();
    }
    void OnRideTextChanged(string oldtext, string newtext)
    {
        RideText.text = newtext;
    }
    void SetRide()
    {
        StartCoroutine(ChangedRide());
    }
    IEnumerator ChangedRide()
    {
        while (TransRide.childCount > 0)
        {
            Destroy(TransRide.GetChild(0).gameObject);
            yield return null;
        }
        ride = Instantiate(Data.instance.RidePrefabs[(int)rideId], TransRide).GetComponent<Ride>();
        ride.rider = playerController;
        if (isLocalPlayer)
        {
            playerController.transform.position += Vector3.up * ride.height;
        }
        ride.transform.localPosition = -ride.RideLocalPosition();
        rideHealth = ride.Health;
        OnRideSpawned();
    }
    [Command]
    public void CmdSetRide(RideID rideId)
    {
        SetRide(rideId);
    }
    [Server]
    public void SetRide(RideID rideId)
    {
        if (this.rideId == rideId)
        {
            if (isRideActive)
            {
                SetRideActive(false);
            }else if (isCooled)
            {
                SetRideActive(true);
            }
        }
        else
        {
            this.rideId = rideId;
        }
    }
    [Server]
    public void RideHurt()
    {
        rideHealth--;
        if (rideHealth <= 0)
        {
            SetRideActive(false);
        }
    }
    [ClientRpc]
    public void SetRideActive(bool state)
    {

        if (state)
        {
            if (ride != null)
            {
                ride.gameObject.SetActive(state);
                if (isLocalPlayer)
                {
                    playerController.transform.position += ride.height * Vector3.up;
                }
                ride.transform.localPosition = -ride.RideLocalPosition();
                rideHealth = ride.Health;
            }
#if UNITY_EDITOR
            else
            {
                Debug.Log($"there's no ride in {name}, but you're trying to invoke {nameof(SetRideActive)}.\n At {new System.Diagnostics.StackTrace().GetFrame(0).ToString()}");
            }
#endif
        }
        else
        {
            if (isRideActive)
            {
                isCooled = false;
                StartCoroutine(CountSeconds(ride.CoolDownTime, UpdateRideText, AfterCountSeconds));
                ride.gameObject.SetActive(state);
            }
        }
        OnRideActiveChanged(state);
    }
    void UpdateRideText(float seconds)
    {
        rideText = "RideCool: " + seconds.ToString();
    }
    void AfterCountSeconds(float seconds)
    {
        rideText = "";
        isCooled = true;
    }
    IEnumerator CountSeconds(float StartSeconds, FunctionsParaFloat invokeDuring, FunctionsParaFloat invokeAfter)
    {
        var wait = new WaitForSeconds(1f);
        while (StartSeconds > 0)
        {
            invokeDuring(StartSeconds);
            StartSeconds--;
            yield return wait;
        }
        if (invokeAfter != null)
        {
            invokeAfter(StartSeconds);
        }
    }
    [ServerCallback]
    public void OnHitByBullet(Bullet bul)
    {
        if (bul.ignoreSelf && bul.userID == playerController.netId)
        {
            return;
        }
        bul.DestroyOnSelf();
        RideHurt();
    }
    public void OnRideSpawned()
    {
        isRideActive = true;
        if (isLocalPlayer)
        {
            ride.SetEffect(true);
        }
    }
    public void OnRideActiveChanged(bool state)
    {
        isRideActive = state;
        if (isLocalPlayer)
        {
            ride.SetEffect(state);
        }
    }
    public void OnRideDestroy()
    {
        isRideActive = false;
        if (isLocalPlayer)
        {
            ride.SetEffect(false);
        }
    }
}
