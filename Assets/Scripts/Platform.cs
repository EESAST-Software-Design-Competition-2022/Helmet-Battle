using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Platform : NetworkBehaviour
{
    public float Speed;
    
    private void Update()
    {
        transform.Translate(Vector3.left * Speed * Time.deltaTime);
        if (isServer)
        {
            if (transform.position.x < -50)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}
