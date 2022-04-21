using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public float forceMultiple;

    public Animator anim;

    private void Awake()
    {
        instance = this;
    }
    public void ChangeLobby(bool moveRight)
    {

        if(anim.GetCurrentAnimatorStateInfo(0).IsName("MoveToConnectLobby") && !moveRight)
        {
            anim.enabled = true;
            anim.Play("MoveToStartLobby");
        }
        else if(moveRight)
        {
            anim.enabled = true;
            anim.Play("MoveToConnectLobby");
        }
    }

    public void stopAnim()
    {
        anim.enabled = false;
    }
}
