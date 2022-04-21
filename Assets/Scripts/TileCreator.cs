using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
/*
 * 地形生成器：
 * 思路：先生成四层，再生成障碍物，最后生成功能方块，即三轮循环。
 * 
 */
public enum BlockType : byte
{
    Ground,
    Blank,
    Wall1,
    Wall2
}

public enum TileID: byte
{
    Null,
    TileGround,
    WoodBox
}
public class TileCreator : MonoBehaviour
{
    public static TileCreator instance;
    public GridMove[] gridsDic;
    public Tile tileGround;
    public Tile tileWoodBox;
    public const Tile tileNull = null;
    public int[] height;
    public int width;
    public GridMove Grid;
    System.Random rnd;
    public int[] proportion;
    int[] range;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        range = new int[proportion.Length];
#if UNITY_EDITOR
        if (range.Length == 0)
        {
            Debug.LogError("proportion is not set!");
        }
#endif
        for (int i = 0; i < range.Length; i++)
        {
            if (i == 0)
            {
                range[i] = proportion[i];
            }
            else
            {
                range[i] = range[i - 1] + proportion[i];
            }
        }
    }
    IEnumerator MakeGrid(float timeInterval)
    {
        Debug.Log("MakeGridStart");
        var w = new WaitForSeconds(timeInterval);
        var last_index = 3;
        yield return w;
        while (gameObject.activeSelf)
        {
            Debug.Log($"Set Grid {last_index}");
            gridsDic[last_index].tilemap.ClearAllTiles();
            SetGrid(gridsDic[last_index]);
            gridsDic[last_index].transform.position = gridsDic[(last_index + 1) % 4].transform.position + Vector3.right * width;
            last_index = (last_index + 3) % 4;
            yield return w;
        }
    }
    public void SetGrid(GridMove grid)
    {
        Debug.Log($"Set single grid {grid.GridId}");
        SetTiles(grid.tilemap);
    }
    public void SetSeed(int seed)
    {

        rnd = new System.Random(unchecked(seed));
        //Random.InitState(seed);
    }
    public void SetInitialGrid()
    {
        Debug.Log("SetInitialGrid");
        for (int i = 0; i < 4; i++)
        {
            SetGrid(gridsDic[i]);
        }
        StartCoroutine(MakeGrid(width / Grid.speed));
    }
    void SetTiles(Tilemap tilemap)
    {
        BlockType[,] mapInfo = new BlockType[height.Length, width];
        //string s = "";
        for (int h = 0; h < height.Length; h++)
        {
            for (int w = 0; w < width; w++)
            {
                var id = rnd.Next(0, range[range.Length - 1]);
                var l = GetLayer(id, range);
                //s += id.ToString() + ",";
                switch (l)
                {
                    case 0:
                        mapInfo[h, w] = BlockType.Ground;
                        break;
                    case 1:
                        mapInfo[h, w] = BlockType.Blank;
                        break;
                    case 2:
                        mapInfo[h, w] = BlockType.Wall1;
                        break;
                    case 3:
                        mapInfo[h, w] = BlockType.Wall2;
                        break;
                }
            }
        }
        //UIController.instance.textDebug.text = $"Seed: , SetTiles : {s}";
        SetInitBlock(tilemap);
        SetBlocks1(tilemap, mapInfo);
        SetBlocks2(tilemap, mapInfo);
    }
    void SetInitBlock(Tilemap tilemap)
    {
        for (int h = 0; h < height.Length; h++)
        {
            for (int w = 0; w < width; w++)
            {
                tilemap.SetTile(new Vector3Int(w, height[h], 0), tileGround);
            }
        }
    }
    //生成障碍
    void SetBlocks1(Tilemap tilemap, BlockType[,] mapInfo)
    {
        //Debug.Log($"h: {mapInfo.GetUpperBound(0)}, w: {mapInfo.GetUpperBound(1)}");
        for (int h = 0; h <= mapInfo.GetUpperBound(0); h++)
        {
            for (int w = 0; w <= mapInfo.GetUpperBound(1); w++)
            {

                BlockType tileId = mapInfo[h, w];
                Vector3Int tilePosition = new Vector3Int(w, height[h], 0);
                switch (tileId)
                {
                    case BlockType.Ground:
                        break;
                    case BlockType.Blank:
                        tilemap.SetTile(tilePosition, tileNull);
                        if (w > 0 && (mapInfo[h, w - 1] <= BlockType.Blank))
                            tilemap.SetTile(tilePosition + Vector3Int.left, tileNull);
                        if (w < mapInfo.GetUpperBound(1) && (mapInfo[h, w + 1] <= BlockType.Blank))
                            tilemap.SetTile(tilePosition + Vector3Int.right, tileNull);
                        break;
                    case BlockType.Wall1:
                        if (!(h+1 >= height.Length || h < 0))
                            if (mapInfo[h + 1, w] == BlockType.Ground)
                                for (var i = height[h] + 1; i < height[h + 1]; i++)
                                {
                                    tilePosition.y = i;
                                    tilemap.SetTile(tilePosition, tileWoodBox);
                                }
                        break;
                    case BlockType.Wall2:
                        if (!(h + 1 >= height.Length || h < 0))
                                for (var i = height[h] + 1; i < (height[h + 1] + height[h])/2; i++)
                                {
                                    tilePosition.y = i;
                                    tilemap.SetTile(tilePosition, tileWoodBox);
                                }
                        break;
                }
            }
        }

    }
    //生成功能方块
    void SetBlocks2(Tilemap tilemap, BlockType[,] mapInfo)
    {

    }
    int GetLayer(int a,int[] array)
    {
        int i = 0;
        for (i = 0; i < array.Length - 1; i++)
        {
            if (a < array[i])
            {
                return i;
            }
        }
        return i;
    }
    public Tile GetTileById(TileID tileID)
    {
        switch (tileID)
        {
            case TileID.Null:
                return tileNull;
            case TileID.WoodBox:
                return tileWoodBox;
            case TileID.TileGround:
                return tileGround;
            default:
                return tileNull;
        }
    }
}
