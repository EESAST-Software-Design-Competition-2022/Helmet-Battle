using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlatformSpawner : NetworkBehaviour
{
    public GameObject LongPlatform;
    public GameObject[] PlatformPrefabs;
    public float SpawnTime;
    public int SpawnRangeY;
    public float SpawnRangeMax;
    public IEnumerator c;
    float lastSpawnY;
    public override void OnStartServer()
    {
        c = SpawnPlatform(SpawnTime);
        StartCoroutine(c);
    }

    IEnumerator SpawnPlatform(float time)
    {
        var w = new WaitForSeconds(time);
        while (true)
        {
            if (PlatformPrefabs.Length > 0)
            {
                if(SpawnRangeY + lastSpawnY < SpawnRangeMax && lastSpawnY - SpawnRangeY > -SpawnRangeMax)
                {
                    lastSpawnY += Random.Range(-SpawnRangeY, SpawnRangeY);
                }
                else if(SpawnRangeY + lastSpawnY >= SpawnRangeMax)
                {
                    lastSpawnY += Random.Range(-SpawnRangeY,0);
                }
                else
                {
                    lastSpawnY += Random.Range(0,SpawnRangeY);
                }
                var p = Instantiate(PlatformPrefabs[Random.Range(0,PlatformPrefabs.Length)], transform.position + Vector3.up * lastSpawnY, Quaternion.identity);
                NetworkServer.Spawn(p);
            }
            yield return w;
        }
    }
}
