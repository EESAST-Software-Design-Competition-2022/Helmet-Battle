using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
public delegate void FunctionsWithNoParaAndReturn();
public delegate void FunctionsParaFloat(float f);
public delegate void StartGame(int seed);
public delegate void EndGame(uint playerId);
/*
 task 220320
设计升级机制：升级后可选择武器和坐骑，并升级到相应的时代。
等级在玩家头顶显示。(get)
武器在0,1，3,4，6,7，9,10，12,13，15,16级时获取(get)。
1级：staff,stone
4级：sword,bow
7级：powder,powdergun
10级：pistol,rocket
13级：hangrenade,missile
16级：lightsword,laser
按下f可以切换近战和远程武器。ui左下角显示武器图标和冷却时间。
坐骑在2，5，8，11，14，17级时获取(get)。
2:马
5:战马
8:射石炮
11:坦克
14:信息化自行车
17:光速飞船
按下e可以唤出坐骑(get)。若有坐骑，按下e可以使用技能（若有技能的话）。
坐骑的图标和冷却时间在ui右下角显示。
获取光速飞船并向上飞出屏幕宣布游戏胜利，显示：{color} is the winner! 。然后弹窗显示所有人的分数。
 */
public class SceneScript : NetworkBehaviour
{
    public static SceneScript instance;
    [SyncVar(hook = nameof(OnTimeScaleChanged))]
    [HideInInspector]
    public float TimeScale;
    [SyncVar(hook = nameof(OnTextChanged))]
    [HideInInspector]
    public string strSeconds;
    [SyncVar(hook = nameof(OnTextTimeLeftChanged))]
    [HideInInspector]
    public string strTimeLeft;
    [SyncVar(hook = nameof(OnTextColorChanged))]
    [HideInInspector]
    public Color textColor;
    [HideInInspector]
    //server
    public int playerAliveCount;

    [HideInInspector]
    public Dictionary<uint, PlayerController> players = new Dictionary<uint, PlayerController>();

    public string[] SceneList;

    public event StartGame OnGameStart;
    public event EndGame OnGameEnd;
#if UNITY_EDITOR
    [SerializeField]
    bool SingleDebug;
#endif
    float PauseSeconds = 3f;
    float StartSeconds = 3f;
    float GameSeconds = 30f;
    private void Awake()
    {
        instance = this;
        OnGameStart += SetSeed;
    }
    private void Start()
    {
        //SceneManager.activeSceneChanged += OnSceneChanged;
        if (SceneManager.GetActiveScene().name == "LobbyOffline" || SceneManager.GetActiveScene().name == "LobbyReady")
        {
            if (isServer)
            {
                Respawn();
                SetTimeScale(1f);
            }
        }
        else
        {
            SoundManager.instance.StopMusic();
            if (isServer)
            {
                ConnectManager.instance.myNetworkDiscovery.StopDiscovery();
                Respawn();
                SetTimeScale(1f);
                StartGame();
            }
        }
    }
    public override void OnStartServer()
    {

    }
    public override void OnStartClient()
    {
        StartCoroutine(DataCheck());
    }
    public void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log($"Old scene: {oldScene.name}, new scene: {newScene.name}");
        if (oldScene.name != "" && newScene.name == "LobbyOffline")
        {
            SoundManager.instance.PlayMusicWaiting();
        }
        else if (oldScene.name == "MapRandom" && newScene.name == "LobbyReady")
        {
            SoundManager.instance.StopMusic();
            SoundManager.instance.PlayMusicWaiting();
        }
        else if (oldScene.name == "LobbyOffline" && newScene.name == "LobbyReady")
        {
            SoundManager.instance.StopMusic();
            //PlayMusicWaiting();
        }
    }
    [Server]
    public void RandomChangeSceneToStartGame()
    {
        NetworkManager.singleton.ServerChangeScene(SceneList[Random.Range(0, SceneList.Length)]);
    }
    [Server]
    public void ServerGotoScene(string str)
    {
        NetworkManager.singleton.ServerChangeScene(str);
    }
    //调用玩家的Respawn,重置玩家状态
    [Server]
    public void Respawn()
    {
        playerAliveCount = NetworkServer.connections.Count;
    }
    [Server]
    public void UpdateAliveNumber()
    {
        playerAliveCount = NetworkServer.connections.Count;
        foreach (var playerController in players.Values)
        {
            if (playerController.isDead)
            {
                playerAliveCount--;
            }
        }
    }
    [Server]
    public void StartGame()
    {
        StartCoroutine(WaitAndStartGame(StartSeconds));
        StartCoroutine(CountRealtimeSeconds(
            StartSeconds,
            delegate (float seconds) { strSeconds = seconds.ToString(); },
            delegate
            {
                strSeconds = "";
                //StartCoroutine(CountSeconds(GameSeconds, delegate (float seconds)
                //{
                //    strTimeLeft = seconds.ToString();
                //}, delegate { FindTheWinner(); }));
            }));
    }
    IEnumerator WaitAndStartGame(float seconds)
    {
        yield return null;
        SetTimeScale(0f);
        yield return new WaitForSecondsRealtime(seconds);
#if UNITY_EDITOR
        if (SingleDebug)
        {
            RpcStartGame(10);
        }
        else
        {

            RpcStartGame((int)System.DateTime.Now.Ticks);
        }
#else
        RpcStartGame((int)System.DateTime.Now.Ticks);
#endif
        SetTimeScale(1f);
    }
    [ClientRpc]
    void RpcStartGame(int seed)
    {
        SoundManager.instance.PlayMusicGaming();
        SetSeed(seed);
    }
    [ClientRpc]
    void RpcEndGame(uint playerId)
    {
        OnGameEnd(playerId);
    }
    void SetSeed(int seed)
    {
        TileCreator.instance.SetSeed(seed);
        SetInitialGrid();
    }
    void SetInitialGrid()
    {
        TileCreator.instance.SetInitialGrid();
    }
    IEnumerator CountRealtimeSeconds(float seconds, FunctionsParaFloat InvokeEverySecond, FunctionsParaFloat InvokeAfterCountSeconds)
    {
        textColor = Color.white;
        var wait = new WaitForSecondsRealtime(1f);
        while (seconds > 0)
        {
            if (InvokeEverySecond != null)
            {
                InvokeEverySecond(seconds);
            }
            seconds--;
            yield return wait;
        }
        if (InvokeAfterCountSeconds != null)
        {
            InvokeAfterCountSeconds(seconds);
        }
    }
    IEnumerator CountSeconds(float seconds, FunctionsParaFloat InvokeEverySecond, FunctionsParaFloat InvokeAfterCountSeconds)
    {
        var wait = new WaitForSeconds(1f);
        while (seconds > 0)
        {
            if (InvokeEverySecond != null)
            {
                InvokeEverySecond(seconds);
            }
            seconds--;
            yield return wait;
        }
        if (InvokeAfterCountSeconds != null)
        {
            InvokeAfterCountSeconds(seconds);
        }
    }
    [Server]
    public void PauseGame(Color color)
    {
        SetTimeScale(0f);
        StartCoroutine(WaitAndChangeToNextScene(PauseSeconds));
        StartCoroutine(PauseGameForSeconds(PauseSeconds, color));
    }
    IEnumerator WaitAndChangeToNextScene(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        RpcSetMusicWait();
        ServerGotoScene("LobbyReady");
    }
    [ClientRpc]
    void RpcSetMusicWait()
    {
        SoundManager.instance.PlayMusicWaiting();
    }
    IEnumerator PauseGameForSeconds(float seconds, Color color)
    {
        textColor = color;
        strSeconds = "Good Game";
        yield return new WaitForSecondsRealtime(seconds);
        strSeconds = "";
    }

    [Server]
    public void DistributeColor(uint netId)
    {
        var i = Color.red;
        foreach (var player in players.Values)
        {
            if (player.color != Color.white && player.color == i)
            {
                i++;
            }
        }
        //分配颜色
        if (i > Color.yellow)
        {
            i = Color.white;
#if UNITY_EDITOR
            Debug.LogError("Out of range: " + nameof(DistributeColor));
#endif
        }

        players[netId].color = i;
    }
    [Server]
    public void SetTimeScale(float timeScale)
    {
        TimeScale = timeScale;
    }
    IEnumerator DataCheck()
    {
        var wait = new WaitForSecondsRealtime(1f);
        while (gameObject.activeSelf)
        {
            if (Time.timeScale != TimeScale)
            {
                Time.timeScale = TimeScale;
            }
            if (UIController.instance.textWinner.text != strSeconds)
            {
                UIController.instance.textWinner.text = strSeconds;
            }
            if (UIController.instance.textWinner.color != GetColor(textColor))
            {
                UIController.instance.textWinner.color = GetColor(textColor);
            }
            yield return wait;
        }
    }
    void OnTimeScaleChanged(float oldTimeScale, float newTimeScale)
    {
        Time.timeScale = newTimeScale;
    }
    void OnTextChanged(string oldText, string newText)
    {
        UIController.instance.textWinner.text = strSeconds;
    }
    void OnTextColorChanged(Color oldColor, Color newColor)
    {
        UIController.instance.textWinner.color = GetColor(newColor);
    }
    void OnTextTimeLeftChanged(string oldText, string newText)
    {
        UIController.instance.textTimeLeft.text = newText;
    }
    public UnityEngine.Color GetColor(Color color)
    {

        switch (color)
        {
            case Color.white:
                return UnityEngine.Color.white;
            case Color.blue:
                return UnityEngine.Color.blue;
            case Color.green:
                return UnityEngine.Color.green;
            case Color.red:
                return UnityEngine.Color.red;
            case Color.yellow:
                return UnityEngine.Color.yellow;
            default:
                return UnityEngine.Color.white;
        }
    }
    [Server]
    bool CheckIsAllReady()
    {
        foreach (var player in players.Values)
        {
            if (!player.IsReady)
            {
                return false;
            }
        }
        return true;
    }
    [Server]
    public void TryToStartGame()
    {
        var boxesController = FindObjectOfType<BoxesController>();
        if (CheckIsAllReady() && players.Count > 1)
        {
#if UNITY_EDITOR
            if (boxesController == null)
                Debug.LogError("no BoxesController");
#endif

            StartCoroutine(CountRealtimeSeconds(StartSeconds,
                delegate (float seconds) { strSeconds = seconds.ToString(); },
                delegate { strSeconds = ""; boxesController.FlyAllBoxes(); }));
        }
#if UNITY_EDITOR
        if (SingleDebug)
        {
            StartCoroutine(CountRealtimeSeconds(StartSeconds,
               delegate (float seconds) { strSeconds = seconds.ToString(); },
               delegate { strSeconds = ""; boxesController.FlyAllBoxes(); }));
        }
#endif
    }
    [Server]
    public void FindTheWinner()
    {
        uint? playerId = null;
        foreach (KeyValuePair<uint, PlayerController> kvp in players)
        {
            if (playerId == null)
            {
                playerId = kvp.Key;
                continue;
            }
            if (players[(uint)playerId].score < kvp.Value.score)
            {
                playerId = kvp.Value.netId;
            }
        }
        if (playerId != null)
        {
            PauseGame(players[(uint)playerId].color);
        }
        else
        {
            Debug.Log("playerId = null.Maybe there's no player in the dictionary.");
        }
    }
    [Server]
    public void WinGame(uint playerId)
    {
        if (players.ContainsKey(playerId))
        {
            PauseGame(players[playerId].color);
            //RpcEndGame(playerId);
        }
    }
}
