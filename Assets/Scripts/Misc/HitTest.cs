using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitTest : MonoBehaviour
{
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Bullet"))
        {
            var bul = collision.GetComponent<Bullet>();
            Debug.Log("HitTest");
#if UNITY_EDITOR
            if(bul == null)
            {
                Debug.LogError("no Bullet script: " + collision.name);
            }
#endif
            switch (bul.bulletID)
            {
                case BulletID.StoneBullet:
                    rb.AddForce(bul.impulse * (transform.position - bul.transform.position), ForceMode2D.Impulse);
                    break;
            }
        }
    }
}
