using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DoorController : MonoBehaviour
{
    public  Animator anim;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("OpenDoor"))
            {
                anim.Play("OpenDoor");
            }
            CameraController.instance.ChangeLobby(collision.transform.position.x < transform.position.x);
        }
    }
}
