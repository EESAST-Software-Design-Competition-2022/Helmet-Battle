using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponSpawner : NetworkBehaviour
{
    enum SpawnMethod:byte {
        roundRobin,
        random
    }
    [SerializeField]
    float spawnDuration = 30f;
    [SerializeField]
    Transform[] spawnPoint;
    [SerializeField]
    SpawnMethod spawnMethod = SpawnMethod.roundRobin; 
    [SerializeField]
    GameObject sceneWeaponPrefab;

    float spawnTime = 0f;
    int weaponCount;
    int spawnPositionCount;
    [SerializeField]
    int spawnPositionIndex = 0;
    private void Awake()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
        weaponCount = Data.instance.WeaponPrefabs.Length;
        spawnPositionCount = spawnPoint.Length;
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            if (Time.time > spawnTime)
            {
                SpawnWeapon();
                spawnTime = Time.time + spawnDuration;
            }
        }
    }
    [Server]
    void SpawnWeapon()
    {
        switch (spawnMethod) {
            case SpawnMethod.roundRobin:
                spawnPositionIndex = (spawnPositionIndex + 1 == spawnPositionCount ? 0 : spawnPositionIndex + 1);
                break;
            case SpawnMethod.random:
                spawnPositionIndex = Random.Range(0, spawnPositionCount);
                break;
        }
        var sceneWeapon = Instantiate(this.sceneWeaponPrefab, spawnPoint[spawnPositionIndex].position, Quaternion.identity);
        NetworkServer.Spawn(sceneWeapon);
        sceneWeapon.GetComponent<SceneWeapon>().SetWeaponID((WeaponID)Random.Range(1, weaponCount));
    }
}
