using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TileCollide : MonoBehaviour
{
    GridMove rootGridMove;
    private void Awake()
    {
        rootGridMove = transform.GetComponentInParent<GridMove>();
    }
    [ServerCallback]
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            rootGridMove.OnBulletHit(collision.transform.position,collision.GetComponent<Bullet>().bulletID);
        }else if(collision.CompareTag("Ride")){
            rootGridMove.OnRideHit(collision.GetComponent<Ride>().collidePosition.position);
        }
    }
}
