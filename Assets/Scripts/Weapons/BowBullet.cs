using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowBullet : BulletPhysics
{
    // Update is called once per frame
    void Update()
    {
        transform.right = rb.velocity.normalized;
    }
}
