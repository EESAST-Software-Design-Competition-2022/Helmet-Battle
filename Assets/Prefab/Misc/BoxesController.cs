using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class BoxesController : NetworkBehaviour
{
    public static BoxesController singleton;
    public BoxTrigger[] boxes;
    public SceneScript sceneScript;
    bool isEndFly = false;
    private void Awake()
    {
        singleton = this;
    }
    [ClientRpc]
    public void FlyAllBoxes()
    {
        foreach(var box in boxes)
        {
            box.anim.Play("Fly");
        }
    }
    [ServerCallback]
    public void EndFly()
    {
        if (!isEndFly)
        {
            isEndFly = true;
            sceneScript.ServerGotoScene("MapRandom");
        }
    }
}
