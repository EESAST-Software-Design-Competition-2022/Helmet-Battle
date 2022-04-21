using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class AutoStartHost : MonoBehaviour
{
    public static AutoStartHost instance;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (ConnectManager.instance.HostActive)
        {
            gameObject.SetActive(false);
        }
    }
    private void OnEnable()
    {
        if (ConnectManager.instance.HostActive)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (NetworkManager.IsSceneActive("LobbyOffline") && !ConnectManager.instance.HostActive)
            {
                ConnectManager.instance.StartLocalHost();
                gameObject.SetActive(false);
            }
        }
    }

}

