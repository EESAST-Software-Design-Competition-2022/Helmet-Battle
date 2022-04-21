using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public enum BoxType
{
    SearchServer,
    StartHost,
    StartServer,
    Ready
}
public class BoxTrigger : MonoBehaviour
{

    public BoxType boxType;
    [HideInInspector]
    public Animator anim;
    [HideInInspector]
    public Rigidbody2D rb;
    public event FunctionsWithNoParaAndReturn EventEndFly;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Close") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Fly"))
        {

            if (boxType == BoxType.Ready)
            {
                collision.GetComponent<PlayerController>().SetReady(true);
            }
            anim.Play("Close");
        }
    }

    public void EndFly()
    {
        switch (boxType)
        {
            case BoxType.SearchServer:
                EventEndFly();
                break;
            case BoxType.StartHost:
                EventEndFly();
                break;
            case BoxType.Ready:
                BoxesController.singleton.EndFly();
                break;
        }
    }
    public void EndClose()
    {
        switch (boxType)
        {
            case BoxType.SearchServer:
                DiscoveryHUD.instance.OnButtonFindServer();
                break;
            case BoxType.StartHost:
                DiscoveryHUD.instance.OnButtonStartHost();
                break;
            case BoxType.StartServer:
                break;
            case BoxType.Ready:
                break;
        }
    }
}

