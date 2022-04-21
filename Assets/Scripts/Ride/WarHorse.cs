using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarHorse : Horse
{
    public float impulse;
    public float hitImpulse;
    private void Start()
    {
    }
    //¿Í»§¶Ëº¯Êý
    public override void ReleaseSkill()
    {
        StartCoroutine(rider.DepriveControl());
        //StartCoroutine(SetHit(1f));
        rider.rb.AddForce(Vector2.right * (-rider.man.transform.localScale.x) * impulse, ForceMode2D.Impulse);
    }
    //IEnumerator SetHit(float time)
    //{
    //    hit.enabled = true;
    //    yield return new WaitForSeconds(time);
    //    hit.enabled = false;
    //}
}
