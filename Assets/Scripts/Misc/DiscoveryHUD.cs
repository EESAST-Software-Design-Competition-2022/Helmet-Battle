using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Discovery;
using MyDiscovery;
public class DiscoveryHUD : MonoBehaviour
{
    
    public static DiscoveryHUD instance;
    public BoxTrigger boxFindServer;
    public BoxTrigger boxStartHost;
    public Button buttonServerInfoPrefab;
    private void Awake()
    {
        instance = this;
    }
    public void OnButtonStartHost()
    {

        ConnectManager.instance.StopConnect();
        UIController.instance.InputHostName.text = ConnectManager.instance.HostName;
        UIController.instance.PanelSetName.SetActive(true);
    }
    public void OnButtonSetHostName()
    {
        ConnectManager.instance.HostName = UIController.instance.InputHostName.text;
        UIController.instance.PanelSetName.SetActive(false);
        FunctionsWithNoParaAndReturn f = delegate { ConnectManager.instance.StartGameHost(); };
        boxStartHost.EventEndFly += f;
        boxStartHost.anim.Play("Fly");
    }
    public void OnButtonFindServer()
    {
        ConnectManager.instance.StopConnect();
        UIController.instance.PanelSearchServer.SetActive(true);
        ConnectManager.instance.SearchServer();

    }
    public void OnButtonServerInfo(ServerResponse info)
    {
        UIController.instance.PanelSearchServer.SetActive(false);
        FunctionsWithNoParaAndReturn f = delegate () { ConnectToServer(info); };
        boxFindServer.EventEndFly += f;
        boxFindServer.anim.Play("Fly");
    }
    public void OnButtonServerInfo(MyDiscoveryResponse info)
    {
        UIController.instance.PanelSearchServer.SetActive(false);
        ConnectManager.instance.myNetworkDiscovery.SetHostName(info.hostname);
        FunctionsWithNoParaAndReturn f = delegate () { ConnectToServer(info); };
        boxFindServer.EventEndFly += f;
        boxFindServer.anim.Play("Fly");
    }
    public void ConnectToServer(ServerResponse info)
    {
        ConnectManager.instance.StopConnect();
        ConnectManager.instance.StopDiscovery();
        ConnectManager.instance.StartClient(info.uri);
    }
    public void ConnectToServer(MyDiscoveryResponse info)
    {
        ConnectManager.instance.StopConnect();
        ConnectManager.instance.StopDiscovery();
        ConnectManager.instance.StartClient(info.uri);
    }
    public void ButtonServerInfoCreate(ServerResponse info)
    {
            var button = Instantiate(buttonServerInfoPrefab, UIController.instance.Content);
            button.onClick.AddListener(delegate
            {
                OnButtonServerInfo(info);
            });
            button.GetComponentInChildren<Text>().text = info.EndPoint.Address.ToString();
    }
    public void ButtonServerInfoCreate(MyDiscoveryResponse info)
    {
        var button = Instantiate(buttonServerInfoPrefab, UIController.instance.Content);
        button.onClick.AddListener(delegate
        {
            OnButtonServerInfo(info);
        });
        button.GetComponentInChildren<Text>().text = info.hostname;
    }
    public void ClearServerInfo()
    {
        var content = UIController.instance.Content;
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

}
