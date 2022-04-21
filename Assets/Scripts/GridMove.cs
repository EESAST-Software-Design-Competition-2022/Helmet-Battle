using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

/*
 * 设置为NetworkIdentity，作为sceneObject使用
 * 当到达场景左端时，自动移至场景右端，并要求TileCreator设置Tile
 */
public class GridMove : NetworkBehaviour
{
    public float speed;
    public Tilemap tilemap;
    public float endX;
    public byte GridId;
    public GameObject CrateVFX;

    private void FixedUpdate()
    {
        transform.Translate(-speed * Time.fixedDeltaTime, 0, 0);
    }
    [ClientRpc]
    void RpcResetTile()
    {
        tilemap.ClearAllTiles();
        TileCreator.instance.SetGrid(this);
        transform.position = TileCreator.instance.gridsDic[(GridId + 1) % 4].transform.position + Vector3.right * TileCreator.instance.width;
    }
    [Server]
    public void OnRideHit(Vector3 position)
    {
        Vector3 v = position - transform.position;

        Vector3Int hitPosition = new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        TileBase tilehit = tilemap.GetTile(hitPosition);
        if (tilehit == TileCreator.instance.tileWoodBox)
        {
            RpcSetTile(hitPosition, TileID.Null);
            RpcSetCrateVFX(position, Quaternion.identity);
        }
        var hitDetect = hitPosition;
                for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                hitDetect = hitPosition + new Vector3Int(i, j, 0);
                if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                {
                    RpcSetTile(hitDetect, TileID.Null);
                    RpcSetCrateVFX(position, Quaternion.identity);
                }
                hitDetect = hitPosition + new Vector3Int(-i, -j, 0);
                if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                {
                    RpcSetTile(hitDetect, TileID.Null);
                    RpcSetCrateVFX(position, Quaternion.identity);
                }
            }
        }
        //if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
        //{
        //    RpcSetTile(hitDetect, TileID.Null);
        //    RpcSetCrateVFX(position, Quaternion.identity);
        //}
        //hitDetect += Vector3Int.up;
        //if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
        //{
        //    RpcSetTile(hitDetect, TileID.Null);
        //    RpcSetCrateVFX(position, Quaternion.identity);
        //}
        //hitDetect = hitPosition + Vector3Int.down;
        //if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
        //{
        //    RpcSetTile(hitDetect, TileID.Null);
        //    RpcSetCrateVFX(position, Quaternion.identity);
        //}
        //hitDetect += Vector3Int.down;
        //if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
        //{
        //    RpcSetTile(hitDetect, TileID.Null);
        //    RpcSetCrateVFX(position, Quaternion.identity);
        //}
    }
    [Server]
    public void OnBulletHit(Vector3 position, BulletID bulletID)
    {
        Debug.Log("OnBulletHit: " + bulletID);
        Vector3 v = position - transform.position;

        Vector3Int hitPosition = new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        TileBase tilehit = tilemap.GetTile(hitPosition);
        if (tilehit == TileCreator.instance.tileWoodBox)
        {
            RpcSetTile(hitPosition, TileID.Null);
            RpcSetCrateVFX(position, Quaternion.identity);
        }
        var hitDetect = hitPosition + Vector3Int.up;
        switch (bulletID)
        {
            case BulletID.RocketBullet:
                for(int i = 0; i < 5; i++)
                {
                    for(int j = 0;j < 5; j++)
                    {
                        hitDetect = hitPosition + new Vector3Int(i, j, 0);
                        if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                        {
                            RpcSetTile(hitDetect, TileID.Null);
                            RpcSetCrateVFX(position, Quaternion.identity);
                        }
                        hitDetect = hitPosition + new Vector3Int(-i, -j, 0);
                        if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                        {
                            RpcSetTile(hitDetect, TileID.Null);
                            RpcSetCrateVFX(position, Quaternion.identity);
                        }
                    }
                }
                break;
            case BulletID.PowderBullet:
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        hitDetect = hitPosition + new Vector3Int(i, j, 0);
                        if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                        {
                            RpcSetTile(hitDetect, TileID.Null);
                            RpcSetCrateVFX(position, Quaternion.identity);
                        }
                        hitDetect = hitPosition + new Vector3Int(-i, -j, 0);
                        if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                        {
                            RpcSetTile(hitDetect, TileID.Null);
                            RpcSetCrateVFX(position, Quaternion.identity);
                        }
                    }
                }
                break;
            case BulletID.HandgrenadeBullet:
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        hitDetect = hitPosition + new Vector3Int(i, j, 0);
                        if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                        {
                            RpcSetTile(hitDetect, TileID.Null);
                            RpcSetCrateVFX(position, Quaternion.identity);
                        }
                        hitDetect = hitPosition + new Vector3Int(-i, -j, 0);
                        if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                        {
                            RpcSetTile(hitDetect, TileID.Null);
                            RpcSetCrateVFX(position, Quaternion.identity);
                        }
                    }
                }
                break;
            case BulletID.StoneBullet:
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        hitDetect = hitPosition + new Vector3Int(i, j, 0);
                        if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                        {
                            RpcSetTile(hitDetect, TileID.Null);
                            RpcSetCrateVFX(position, Quaternion.identity);
                        }
                        hitDetect = hitPosition + new Vector3Int(-i, -j, 0);
                        if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                        {
                            RpcSetTile(hitDetect, TileID.Null);
                            RpcSetCrateVFX(position, Quaternion.identity);
                        }
                    }
                }
                break;
            case BulletID.LightSwordBullet:
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        hitDetect = hitPosition + new Vector3Int(i, j, 0);
                        if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                        {
                            RpcSetTile(hitDetect, TileID.Null);
                            RpcSetCrateVFX(position, Quaternion.identity);
                        }
                        hitDetect = hitPosition + new Vector3Int(-i, -j, 0);
                        if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                        {
                            RpcSetTile(hitDetect, TileID.Null);
                            RpcSetCrateVFX(position, Quaternion.identity);
                        }
                    }
                }
                break;
            default:
                if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                {
                    RpcSetTile(hitDetect, TileID.Null);
                    RpcSetCrateVFX(position, Quaternion.identity);
                }
                hitDetect = hitPosition + Vector3Int.down;
                if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                {
                    RpcSetTile(hitDetect, TileID.Null);
                    RpcSetCrateVFX(position, Quaternion.identity);
                }
                hitDetect = hitPosition + Vector3Int.right;
                if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                {
                    RpcSetTile(hitDetect, TileID.Null);
                    RpcSetCrateVFX(position, Quaternion.identity);
                }
                hitDetect = hitPosition + Vector3Int.left;
                if (tilemap.GetTile(hitDetect) == TileCreator.instance.tileWoodBox)
                {
                    RpcSetTile(hitDetect, TileID.Null);
                    RpcSetCrateVFX(position, Quaternion.identity);
                }
                break;

        }


    }
    [ClientRpc]
    void RpcSetTile(Vector3Int vector3Int, TileID tileId)
    {
        tilemap.SetTile(vector3Int, TileCreator.instance.GetTileById(tileId));
    }
    [ClientRpc]
    void RpcSetCrateVFX(Vector3 position, Quaternion quaternion)
    {
        PoolManager.Release(CrateVFX, position, quaternion);
    }
}
