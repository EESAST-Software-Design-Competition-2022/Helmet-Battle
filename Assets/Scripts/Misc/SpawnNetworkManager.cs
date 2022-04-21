using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnNetworkManager : MonoBehaviour
{
    public GameObject networkManager;
    // Start is called before the first frame update
    void Start()
    {
        if(NetworkManager.singleton == null)
        {
            Instantiate(networkManager);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
