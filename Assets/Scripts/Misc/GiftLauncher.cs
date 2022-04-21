using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GiftLauncher : NetworkBehaviour
{
    [SerializeField]
    GameObject GiftPrefab;

    [SerializeField]
    Transform leftPoint;
    float leftPositionX;
    [SerializeField]
    Transform rightPoint;
    float rightPositionX;
    public float speed = 10;
    bool faceRight;

    [SerializeField]
    Animator anim;
    private void Awake()
    {
        leftPositionX = leftPoint.position.x;
        rightPositionX = rightPoint.position.x;
        Destroy(leftPoint.gameObject);
        Destroy(rightPoint.gameObject);
        faceRight = true;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.x > rightPositionX)
        {
            faceRight = false;
        }
        else if(transform.position.x < leftPositionX)
        {
            faceRight = true;
        }
        transform.Translate((faceRight ? 1 : -1) * speed * Time.deltaTime,0,0);
    }
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet") && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            anim.Play("CoolDown");
            ReleaseGift();
        }
    }
    [Server]
    void ReleaseGift()
    {
        var gift = Instantiate(GiftPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(gift);
    }
}
