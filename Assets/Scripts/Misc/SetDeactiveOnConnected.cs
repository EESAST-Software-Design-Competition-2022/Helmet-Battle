using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SetDeactiveOnConnected : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(NetworkServer.active || NetworkClient.isConnected)
        {
            gameObject.SetActive(false);
        }
    }
}
