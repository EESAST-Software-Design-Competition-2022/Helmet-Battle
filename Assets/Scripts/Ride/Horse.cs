using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horse : Ride
{
    Animator anim;
    protected void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        anim.SetBool("Running",Mathf.Abs(rider.rb.velocity.x) > 0.1f);
    }

}
