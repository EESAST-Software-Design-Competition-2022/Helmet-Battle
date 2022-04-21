using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShip : Ride
{
    bool isFlying;
    private void OnEnable()
    {
        isFlying = false;
    }
    public override void ReleaseSkill()
    {
        if (!isFlying)
        {
            StartCoroutine(AddVelocity());
        }
    }
    IEnumerator AddVelocity()
    {
        isFlying = true;
        while (gameObject.activeSelf)
        {
            rider.rb.velocity =new Vector2(rider.rb.velocity.x, 20f);
            yield return new WaitForFixedUpdate();
        }
    }
}
