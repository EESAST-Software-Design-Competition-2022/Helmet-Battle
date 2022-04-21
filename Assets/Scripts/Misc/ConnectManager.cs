using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;
using Mirror;
using kcp2k;
using Mirror.Discovery;
using MyDiscovery;
using System.Net.Sockets;
using System.Net.NetworkInformation;

/*
 * 自动配置端口，提供连接服务
 * 
 * 切换场景
 * 
 * 配置NetworkDiscovery的端口
 * 处理DiscoveryHUD发送过来的信息
 */
public class ConnectManager : MonoBehaviour
{
    public static ConnectManager instance;
    public KcpTransport kcpTransport;
    public MyNetworkDiscovery myNetworkDiscovery;
    public string HostName { get { return myNetworkDiscovery.HostName; } set { myNetworkDiscovery.SetHostName(value); } }
    public readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
    public readonly  Dictionary<long,MyDiscoveryResponse> myDiscoveredServers = new Dictionary<long, MyDiscoveryResponse>();

    public bool HostActive => NetworkServer.active && NetworkClient.isConnected;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartLocalHost()
    {
        StopConnect();
        kcpTransport.Port = (ushort)GetAvailablePort();
        NetworkManager.singleton.StartHost();
        NetworkManager.singleton.ServerChangeScene("LobbyOffline");
    }
    public void StartGameHost()
    {
        StopConnect();
        kcpTransport.Port = (ushort)GetAvailablePort();
        NetworkManager.singleton.StartHost();
        NetworkManager.singleton.ServerChangeScene("LobbyReady");
        discoveredServers.Clear();
        myDiscoveredServers.Clear();
        myNetworkDiscovery.AdvertiseServer();
    }
    public void SearchServer()
    {
        discoveredServers.Clear();
        myDiscoveredServers.Clear();
        myNetworkDiscovery.StartDiscovery();
    }
    public void StopDiscovery()
    {
        myNetworkDiscovery.StopDiscovery();
    }
    public void StopConnect()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
    }
    public void StartClient(Uri uri)
    {
        StopConnect();
        NetworkManager.singleton.StartClient(uri);
    }
    
    public void OnDiscoveredServer(ServerResponse info)
    {
        discoveredServers[info.serverId] = info;
        DiscoveryHUD.instance.ClearServerInfo();
        foreach(var i in discoveredServers.Values)
        DiscoveryHUD.instance.ButtonServerInfoCreate(i);
    }

    public void OnDiscoveredServer(MyDiscoveryResponse info)
    {
        myDiscoveredServers[info.serverId] = info;
        DiscoveryHUD.instance.ClearServerInfo();
        foreach(var i in myDiscoveredServers.Values)
        {
            DiscoveryHUD.instance.ButtonServerInfoCreate(i);
        }
    }
    /// <summary>
    /// 返回可用端口号
    /// </summary>
    /// <returns></returns>                
    public static int GetAvailablePort()
    {
        int BeginPort = 50000;              //开始端口
        int EndPort = 65535;                //结束端口

        Process p = new Process();
        p.StartInfo = new ProcessStartInfo("netstat", "-an");
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();


        List<int> ports = new List<int>();
        string line = null;
        Regex reg = new Regex("\\s+");

        while ((line = p.StandardOutput.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("UDP", StringComparison.OrdinalIgnoreCase))
            {
                line = reg.Replace(line, ",");
                string[] arr = line.Split(',');
                string soc = arr[1];
                int pos = soc.LastIndexOf(':');
                int port = int.Parse(soc.Substring(pos + 1));
                //大于开始端口才记录
                if (port >= BeginPort)
                    ports.Add(port);
            }
        }
        p.Close();

        int result = BeginPort;
        for (int i = BeginPort; i < EndPort; i++)
        {
            if (ports.FindIndex(a => a == i) > -1)
                continue;
            else
            {
                result = i;
                break;
            }
        }
        return result;
    }
    public static string GetIP()
    {
        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;  //无线局域网适配器 

            if ((item.NetworkInterfaceType == _type1) && item.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        output = ip.Address.ToString();
                    }
                }
            }
        }
        return output;
    }
}
