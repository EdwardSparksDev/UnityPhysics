using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MapHandler : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    List<Vector2Int> playersSpawns = new List<Vector2Int>();
    List<Vector2Int> freeTiles = new List<Vector2Int>();

    int length;
    int height;
    int softBlockFillProbability;
    int spawnLength;
    bool spawnProtection;
    int itemsDropRate;

    bool isXEven;
    bool isYEven;
    #endregion

    #region SerializeField
    [Header("Settings")]
    [SerializeField] int enemySpawnsMaxChecks;

    [Header("References")]
    [SerializeField] Tilemap tm_Indestructible;
    [SerializeField] Tilemap tm_Destructible;
    [SerializeField] TileBase[] tiles;

    [Space(25), SerializeField] Destructible pf_Destructible;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        GameInstance.LoadGameSettings(out length, out height, out softBlockFillProbability, out spawnLength, out spawnProtection, out itemsDropRate, out int enemyAmount, out int playersCount);

        length += 2;
        height += 2;

        GenerateMap(enemyAmount);

        EventManager.SpawnEntities?.Invoke(playersSpawns, playersCount, freeTiles, enemyAmount);
    }


    private void OnEnable()
    {
        EventManager.ClearDestructible += OnClearDestructible;
    }


    private void OnDisable()
    {
        EventManager.ClearDestructible -= OnClearDestructible;
    }
    #endregion


    #region Methods
    /// <summary>
    /// Generates the map based on picked settings (generation complexity: O(n^2))
    /// </summary>
    /// <param name="enemyAmount">The number of enemies to be spawned</param>
    private void GenerateMap(int enemyAmount)
    {
        Vector2 center = new Vector2(length / 2, height / 2);

        isXEven = length % 2 == 0;
        isYEven = height % 2 == 0;

        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!GenerateIndestructibleTilemap(i, j))
                {
                    if (!GenerateDestructibleTilemap(i, j) && !IsSpawn(i, j))
                        freeTiles.Add(new Vector2Int(i, j));
                }
            }
        }

        FillPlayersSpawns();
        CheckEnemySpawnsAvailability(enemyAmount);
    }


    /// <summary>
    /// Generates the indestructible section of the map (hard blocks + ground)
    /// </summary>
    /// <param name="i">The x coordinate</param>
    /// <param name="j">The y coordinate</param>
    /// <returns>True = an hard block has spawned, False = couldn't spawn an hard block</returns>
    private bool GenerateIndestructibleTilemap(int i, int j)
    {
        if (IsBorder(i, j))
        {
            FillTilemapCell(true, 0, i, j);
            return true;
        }
        else if (IsHardBlockSpot(i, j))
        {
            FillTilemapCell(true, 0, i, j);
            return true;
        }
        //Spawn a grass block with shadow
        else if (j + 1 < height && (IsBorder(i, j + 1) || IsHardBlockSpot(i, j + 1)))
            FillTilemapCell(true, 3, i, j);
        //Spawn a simple grass block
        else
            FillTilemapCell(true, 2, i, j);

        return false;
    }


    /// <summary>
    /// Detects the map border based on given coordinates
    /// </summary>
    /// <param name="i">The x coordinate</param>
    /// <param name="j">The y coordinate</param>
    /// <returns>True = the selected coordinate is on the map border, False = the selected coordinate isn't on the map border</returns>
    private bool IsBorder(int i, int j)
    {
        return (i == 0 || i == length - 1 || j == 0 || j == height - 1);
    }


    /// <summary>
    /// Detects if the selected spot should spawn an hard block
    /// </summary>
    /// <param name="i">The x coordinate</param>
    /// <param name="j">The y coordinate</param>
    /// <returns>True = an hard block should be spawned, False = an hard block shouldn't be spawned</returns>
    private bool IsHardBlockSpot(int i, int j)
    {
        return (i % 2 == (isXEven ? (i < length / 2 ? 0 : 1) : 0) && j % 2 == (isYEven ? (j < height / 2 ? 0 : 1) : 0));
    }


    /// <summary>
    /// Generates the destructible section of the map (soft blocks)
    /// </summary>
    /// <param name="i">The x coordinate</param>
    /// <param name="j">The y coordinate</param>
    /// <returns>True = a soft block has spawned, False = a soft block didn't spawn</returns>
    private bool GenerateDestructibleTilemap(int i, int j)
    {
        if (IsCriticalPoint(i, j) || (Random.Range(1, 101) <= softBlockFillProbability && !IsSpawn(i, j)))
        {
            FillTilemapCell(false, 1, i, j);
            return true;
        }

        return false;
    }


    /// <summary>
    /// If the spawnProtection settings is turned on, identifies the critical spots which should be protected with a soft block spawn
    /// </summary>
    /// <param name="i">The x coordinate</param>
    /// <param name="j">The y coordinate</param>
    /// <returns>True = the spot should be protected, False = the spot is already safe</returns>
    private bool IsCriticalPoint(int i, int j)
    {
        if (!spawnProtection) return false;

        return !IsSpawn(i, j) && (IsInCriticalSidePoint(i, j, height, length) || IsInCriticalSidePoint(j, i, length, height) || IsSpawnBorder(i, j));
    }


    /// <summary>
    /// Identifies the all spawn blocks
    /// </summary>
    /// <param name="i">The x coordinate</param>
    /// <param name="j">The y coordinate</param>
    /// <returns>True = the block is in spawn range, False = the block isn't in spawn range</returns>
    private bool IsSpawn(int i, int j)
    {
        return IsSpawnCorner(i, j, length, height) || IsSpawnCorner(j, i, height, length);
    }


    /// <summary>
    /// Indentifies if the given spot is inside one of the spawns
    /// </summary>
    /// <param name="i">The x coordinate</param>
    /// <param name="j">The y coordinate</param>
    /// <param name="x">The map length</param>
    /// <param name="y">The map height</param>
    /// <returns>True = the block is inside a spawn, False = the block is not inside a spawn</returns>
    private bool IsSpawnCorner(int i, int j, int x, int y)
    {
        return (i == 1 || i == x - 2) && (j <= spawnLength || j >= y - spawnLength - 1);
    }


    /// <summary>
    /// Identifies the spawns corners
    /// </summary>
    /// <param name="i">The x coordinate</param>
    /// <param name="j">The y coordinate</param>
    /// <param name="x">The map length</param>
    /// <param name="y">The map height</param>
    /// <returns>True = the block is a spawn corner, False = the block is not a spawn corner</returns>
    private bool IsInCriticalSidePoint(int i, int j, int x, int y)
    {
        return (i == 1 || i == y - 2) && (j == spawnLength + 1 || j == x - spawnLength - 2);
    }


    /// <summary>
    /// Identifies if the given spot is inside any of the spawn borders
    /// </summary>
    /// <param name="i">The x coordinate</param>
    /// <param name="j">The y coordinate</param>
    /// <returns>True = the block is inside a spawn border, False = the block is not inside a spawn border</returns>
    private bool IsSpawnBorder(int i, int j)
    {
        return IsSpawnBorderSide(i, j, length, height) || IsSpawnBorderSide(j, i, height, length);
    }


    /// <summary>
    /// Identifies the given spot is inside one of the spawn borders
    /// </summary>
    /// <param name="i">The x coordinate</param>
    /// <param name="j">The y coordinate</param>
    /// <param name="x">The map length</param>
    /// <param name="y">The map height</param>
    /// <returns>True = the block is inside a spawn border, False = the block is not inside a spawn border</returns>
    private bool IsSpawnBorderSide(int i, int j, int x, int y)
    {
        return (i == 2 || i == x - 3) && (j <= spawnLength || j >= y - spawnLength - 1);
    }


    /// <summary>
    /// Fills the selected spot with the requested tile
    /// </summary>
    /// <param name="isIndestructible">Should the tile be placed in the indestructible tilemap or the destructible one</param>
    /// <param name="tileIndex">The tile index to be spawned</param>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    private void FillTilemapCell(bool isIndestructible, int tileIndex, int x, int y)
    {
        Vector3Int cell = tm_Indestructible.WorldToCell(new Vector2(x, y));

        if (isIndestructible)
            tm_Indestructible.SetTile(cell, tiles[tileIndex]);
        else
            tm_Destructible.SetTile(cell, tiles[tileIndex]);
    }


    /// <summary>
    /// Fills the playersSpawns list with the players spawns locations
    /// </summary>
    private void FillPlayersSpawns()
    {
        playersSpawns.Add(new Vector2Int(1, height - 2));
        playersSpawns.Add(new Vector2Int(length - 2, 1));
        playersSpawns.Add(new Vector2Int(1, 1));
        playersSpawns.Add(new Vector2Int(length - 2, height - 2));
    }


    /// <summary>
    /// Prevents enemy spawns unavailability due to soft blocks max spawn probability
    /// </summary>
    /// <param name="enemyAmount"></param>
    private void CheckEnemySpawnsAvailability(int enemyAmount)
    {
        if (freeTiles.Count != 0) return;

        //Request enemy count from GameManager
        int enemyCount = EventManager.GetRandomEnemyCount?.Invoke(enemyAmount) ?? 1;

        int x;
        int y;
        Vector2 pos;
        Vector2Int pos2D;
        Vector3Int cell;
        TileBase tile;

        for (int i = 0; i < enemySpawnsMaxChecks && enemyCount > 0; i++)
        {
            do
            {
                x = Random.Range(1, length - 1);
                y = Random.Range(1, height - 1);
            } while (IsSpawn(x, y) || IsCriticalPoint(x, y));

            pos = new Vector2(x, y);
            pos2D = new Vector2Int(x, y);
            cell = tm_Destructible.WorldToCell(pos);
            tile = tm_Destructible.GetTile(cell);

            if (tile != null && !freeTiles.Contains(pos2D))
            {
                tm_Destructible.SetTile(cell, null);
                freeTiles.Add(pos2D);
                enemyCount--;
            }
        }
    }


    /// <summary>
    /// Clears a soft block by replacing it with a destructible block
    /// </summary>
    /// <param name="pos">The replacement position</param>
    private void OnClearDestructible(Vector2 pos)
    {
        Vector3Int cell = tm_Destructible.WorldToCell(pos);
        TileBase tile = tm_Destructible.GetTile(cell);

        if (tile != null)
        {
            Instantiate(pf_Destructible, pos, Quaternion.identity).GetComponent<Destructible>().ApplyNewSpawnChance(itemsDropRate / 100f);
            tm_Destructible.SetTile(cell, null);
        }
    }
    #endregion
}
