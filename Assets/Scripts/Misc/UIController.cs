using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 * 切换到bulidindex = 0, 1的场景时会隐藏textInfo
 */
public class UIController : MonoBehaviour
{
    public static UIController instance;
    public GameObject PanelSearchServer;
    public Transform Content;
    public GameObject PanelMenu;
    public GameObject PanelOptions;
    public GameObject PanelHUD;
    public GameObject PanelSetName;
    public GameObject PanelInfo;
    public Text textWinner;
    public Text textTimeLeft;
    public Text textInfo;
    public Text textDebug;
    public Text textCoolTime;
    public InputField InputHostName;
    [HideInInspector]
    public PlayerController localPlayer;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnSceneChanged(Scene current,Scene next)
    {
        if (next.buildIndex == 0 || next.buildIndex == 1)
        {
            textInfo.gameObject.SetActive(false);
        }
        else
        {
            textInfo.gameObject.SetActive(true);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PanelMenu.activeSelf || PanelOptions.activeSelf || PanelInfo.activeSelf)
            {
                PanelMenu.SetActive(false);
                PanelOptions.SetActive(false);
                PanelInfo.SetActive(false);
                if(localPlayer != null)
                    localPlayer.isPaused = false;
            }
            else
            {
                PanelMenu.SetActive(true);

                if (localPlayer != null)
                    localPlayer.isPaused = true;
            }
        }
    }
    public void OnMenuButton()
    {
        ConnectManager.instance.StopConnect();
        SceneManager.LoadScene("LobbyOffline");

        PanelMenu.SetActive(false);
        PanelSearchServer.SetActive(false);
        PanelOptions.SetActive(false);
        PanelSetName.SetActive(false);
        PanelInfo.SetActive(false);
        StopAllCoroutines();

        textWinner.text = "";
        textWinner.color = UnityEngine.Color.white;

        Time.timeScale = 1f;
        AutoStartHost.instance.gameObject.SetActive(true);
        ConnectManager.instance.StopDiscovery();
    }
    public void OnResumeButton()
    {
        PanelMenu.SetActive(false);

        if (localPlayer != null)
            localPlayer.isPaused = false;
    }
    public void OnOptionsButton()
    {
        PanelOptions.SetActive(true);
        PanelMenu.SetActive(false);
    }
    public void OnOptionsBackButton()
    {
        PanelOptions.SetActive(false);
        PanelMenu.SetActive(true);
    }
    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void OnSliderMusicChanged(float f)
    {
        Debug.Log("changed Music volume:" + f);
        SoundManager.instance.SetMusicVolume(f);
    }
    public void OnSliderSoundChanged(float f)
    {
        Debug.Log("changed Sound volume:" + f);
        SoundManager.instance.SetSoundVolume(f);
    }
    public void OnInfoButton()
    {
        PanelInfo.SetActive(true);
        PanelMenu.SetActive(false);
    }
    public void OnInfoBackButton()
    {
        PanelInfo.SetActive(false);
        PanelMenu.SetActive(true);
    }
    public void OnOkayButton()
    {
        if (DiscoveryHUD.instance != null)
        DiscoveryHUD.instance.OnButtonSetHostName();
    }
}
