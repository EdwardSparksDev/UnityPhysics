public static class GameInstance
{
    #region Variables & Properties

    #region Local
    static bool dataLoaded;

    static int mapWidth;
    static int mapHeight;
    static int softBlocksSpawnProbability;
    static int spawnsWidth;
    static bool spawnProtection;
    static int itemsDropRate;
    static int enemyAmount;
    static int playersCount;
    #endregion

    #region Properties
    public static bool DataLoaded => dataLoaded;
    #endregion

    #endregion


    #region Methods
    /// <summary>
    /// Saves the given settings to later be loaded in other scenes
    /// </summary>
    /// <param name="width">The map width</param>
    /// <param name="height">The map height</param>
    /// <param name="spawnProb">The soft blocks spawn probability</param>
    /// <param name="spawnsSafeZone">The spawns length</param>
    /// <param name="spawnProt">If spawn protection should be enabled</param>
    /// <param name="itemsDrop">The items drop chance</param>
    /// <param name="enemyCount">The enemy amount</param>
    /// <param name="playersAmount">The players amount</param>
    public static void SaveGameSettings(int width, int height, int spawnProb, int spawnsSafeZone, bool spawnProt, int itemsDrop, int enemyCount, int playersAmount)
    {
        if (!dataLoaded)
            dataLoaded = true;

        mapWidth = width;
        mapHeight = height;
        softBlocksSpawnProbability = spawnProb;
        spawnsWidth = spawnsSafeZone;
        spawnProtection = spawnProt;
        itemsDropRate = itemsDrop;
        enemyAmount = enemyCount;
        playersCount = playersAmount;
    }


    /// <summary>
    /// Loads settings in the given variables
    /// </summary>
    /// <param name="width">The map width</param>
    /// <param name="height">The map height</param>
    /// <param name="spawnProb">The soft blocks spawn probability</param>
    /// <param name="spawnsSafeZone">The spawns length</param>
    /// <param name="spawnProt">If spawn protection should be enabled</param>
    /// <param name="itemsDrop">The items drop chance</param>
    /// <param name="enemyCount">The enemy amount</param>
    /// <param name="playersAmount">The players amount</param>
    public static void LoadGameSettings(out int width, out int height, out int spawnProb, out int spawnsSafeZone, out bool spawnProt, out int itemsDrop, out int enemyCount, out int playersAmount)
    {
        width = mapWidth;
        height = mapHeight;
        spawnProb = softBlocksSpawnProbability;
        spawnsSafeZone = spawnsWidth;
        spawnProt = spawnProtection;
        itemsDrop = itemsDropRate;
        enemyCount = enemyAmount;
        playersAmount = playersCount;
    }
    #endregion
}
