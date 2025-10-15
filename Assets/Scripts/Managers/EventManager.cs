using System;
using System.Collections.Generic;
using UnityEngine;


public static class EventManager
{
    #region Variables & Properties

    #region Local
    public static Action<List<Vector2Int>, int, List<Vector2Int>, int> SpawnEntities;
    public static Action<Vector2> ClearDestructible;

    public static Action<Transform> AddTargetToCameraGroup;
    public static Action PlayerDeath;
    public static Action EnemyDeath;

    public static Action<bool> PauseToggled;
    public static Action<bool> GameOver;

    public static Func<int, int> GetRandomEnemyCount;
    #endregion

    #endregion
}
