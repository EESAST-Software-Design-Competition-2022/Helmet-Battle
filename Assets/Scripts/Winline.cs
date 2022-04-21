using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Winline : MonoBehaviour
{
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<SpaceShip>(out var spaceShip))
        {
            SceneScript.instance.WinGame(spaceShip.rider.netId);
        }
    }
}
