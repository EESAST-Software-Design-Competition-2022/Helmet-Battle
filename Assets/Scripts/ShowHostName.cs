using UnityEngine;
using Mirror.Discovery;
using Mirror;
public class ShowHostName : MonoBehaviour
{
    private void Start()
    {

            GetComponent<TextMesh>().text = ConnectManager.instance.myNetworkDiscovery.HostName;
    }


}
