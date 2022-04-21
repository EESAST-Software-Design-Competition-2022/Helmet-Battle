using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Gift : NetworkBehaviour
{
    [SerializeField]
    GameObject sceneWeapon;
    [SerializeField]
    GameObject BlowVFX;
    int weaponCount;
    // Start is called before the first frame update
    private void Awake()
    {
        GetComponent<Rigidbody2D>().isKinematic = true;
    }
    void Start()
    {
        weaponCount = Data.instance.WeaponPrefabs.Length;
    }
    public override void OnStartServer()
    {
        GetComponent<Rigidbody2D>().isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trap"))
        {
            GiftExplode();
        }
    }
    [Server]
    void GiftExplode()
    {
        var sceneWeapon = Instantiate(this.sceneWeapon, transform.position, transform.rotation);
        NetworkServer.Spawn(sceneWeapon);
        sceneWeapon.GetComponent<SceneWeapon>().SetWeaponID((WeaponID)Random.Range(1, weaponCount));
        RpcReleaseBlowVFX();
        StartCoroutine(DestroyAfterTime(0.1f));
    }
    [ClientRpc]
    void RpcReleaseBlowVFX()
    {
        PoolManager.Release(BlowVFX, transform.position, Quaternion.identity);
    }
    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(gameObject);
    }
}
