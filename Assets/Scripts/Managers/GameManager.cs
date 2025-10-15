using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using Random = UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    PlayerInput managerInput;
    List<PlayerController> playersControllers = new List<PlayerController>();

    DateTime startingTime;

    bool isGameOver;
    int enemyCount;
    #endregion

    #region SerializeField
    [Header("Settings")]
    [SerializeField] List<Vector2Int> maxEnemyCountPerDifficulty = new List<Vector2Int>();

    [Header("References")]
    [SerializeField] List<PlayerController> pf_Players;
    [SerializeField] EnemyController pf_Enemy;

    [Header("GameOver")]
    [SerializeField] Color playersColors;
    [Space(10), SerializeField] GameObject pause;
    [Space(10), SerializeField] GameObject gameOver;
    [SerializeField] TMP_Text txt_Winner;
    [SerializeField] TMP_Text txt_TimeTaken;
    #endregion

    #endregion


    #region Mono
    private void OnEnable()
    {
        if (managerInput == null)
            managerInput = new PlayerInput();

        managerInput.UI.Enable();
        managerInput.UI.Pause.performed += OnTogglePausePerformed;
        managerInput.UI.ToggleGameOver.performed += OnToggleGameOverUIPerformed;

        EventManager.GetRandomEnemyCount += OnGetRandomEnemyCount;
        EventManager.SpawnEntities += OnSpawnEntities;
        EventManager.PlayerDeath += OnAllPlayersDeath;
        EventManager.EnemyDeath += OnEnemyDeath;
    }


    private void OnDisable()
    {
        managerInput.UI.Disable();
        managerInput.UI.Pause.performed -= OnTogglePausePerformed;
        managerInput.UI.ToggleGameOver.performed -= OnToggleGameOverUIPerformed;

        EventManager.GetRandomEnemyCount -= OnGetRandomEnemyCount;
        EventManager.SpawnEntities -= OnSpawnEntities;
        EventManager.PlayerDeath -= OnAllPlayersDeath;
        EventManager.EnemyDeath -= OnEnemyDeath;
    }
    #endregion


    #region Methods

    #region Input
    /// <summary>
    /// Toggles game pause based on current game state
    /// </summary>
    /// <param name="context">The input context</param>
    private void OnTogglePausePerformed(InputAction.CallbackContext context)
    {
        if (isGameOver) return;

        if (Time.timeScale > 0)
        {
            Time.timeScale = 0;
            pause.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            TogglePlayersControls(false);
        }
        else
        {
            pause.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            TogglePlayersControls(true);
            Time.timeScale = 1;
        }

        EventManager.PauseToggled?.Invoke(Time.timeScale == 0);
    }


    /// <summary>
    /// Toggles the game over UI
    /// </summary>
    /// <param name="context">The input context</param>
    private void OnToggleGameOverUIPerformed(InputAction.CallbackContext context)
    {
        if (!isGameOver) return;

        gameOver.SetActive(!gameOver.activeSelf);
    }
    #endregion


    /// <summary>
    /// Toggles players controls
    /// </summary>
    /// <param name="enable">Should controls be enabled or disabled</param>
    private void TogglePlayersControls(bool enable)
    {
        for (int i = 0; i < playersControllers.Count; i++)
        {
            if (!playersControllers[i].IsDead)
                playersControllers[i].TogglePlayerActionMap(enable);
        }
    }


    /// <summary>
    /// Calculates the enemyCount to be spawned after the map generation
    /// </summary>
    /// <param name="enemyAmount">The enemy count to be spawned based on difficulty</param>
    /// <returns></returns>
    private int OnGetRandomEnemyCount(int enemyAmount)
    {
        enemyCount = Random.Range(maxEnemyCountPerDifficulty[enemyAmount].x, maxEnemyCountPerDifficulty[enemyAmount].y + 1);
        return enemyCount;
    }


    /// <summary>
    /// Spawns players end enemies (all entities on the map)
    /// </summary>
    /// <param name="playersSpawns">The free players spawns</param>
    /// <param name="playersCount">The number of players to be spawned</param>
    /// <param name="freeSpawns">The free enemy spawns</param>
    /// <param name="enemyAmount">The number of enemies to be spawned</param>
    private void OnSpawnEntities(List<Vector2Int> playersSpawns, int playersCount, List<Vector2Int> freeSpawns, int enemyAmount)
    {
        if (Time.timeScale <= 0)
            Time.timeScale = 1;

        startingTime = DateTime.Now;
        Cursor.lockState = CursorLockMode.Locked;

        SpawnPlayers(playersSpawns, playersCount);
        SpawnEnemies(freeSpawns, enemyAmount);
    }


    /// <summary>
    /// Spawns all players and binds their controls to their characters
    /// </summary>
    /// <param name="playersSpawns">The free players spawns</param>
    /// <param name="playersCount">The number of players to be spawned</param>
    private void SpawnPlayers(List<Vector2Int> playersSpawns, int playersCount)
    {
        for (int i = 0; i < playersCount; i++)
        {
            PlayerController controller = Instantiate(pf_Players[i], new Vector3(playersSpawns[i].x, playersSpawns[i].y, 0), Quaternion.identity);

            PlayerInput pi = new PlayerInput();
            if (i == 0)
            {
                pi.devices = new[] { Keyboard.current };
                controller.BindPlayerControls(pi);
            }
            else
            {
                pi.devices = new[] { Gamepad.all[i - 1] };
                controller.BindPlayerControls(pi, true, Gamepad.all[i - 1]);
            }

            playersControllers.Add(controller);

            EventManager.AddTargetToCameraGroup?.Invoke(controller.transform);
        }
    }


    /// <summary>
    /// Spawns all enemies on the map
    /// </summary>
    /// <param name="freeSpawns">The available enemy spawns</param>
    /// <param name="enemyAmount">The number of enemies to be spawned</param>
    private void SpawnEnemies(List<Vector2Int> freeSpawns, int enemyAmount)
    {
        int enemies;
        //Check if enemyCount has already been preloaded by the MapHandler
        if (enemyCount > 0)
        {
            enemies = enemyCount;
            enemyCount = 0;
        }
        else
            enemies = Random.Range(maxEnemyCountPerDifficulty[enemyAmount].x, maxEnemyCountPerDifficulty[enemyAmount].y + 1);

        int maxSpawns = Mathf.Min(enemies, freeSpawns.Count);
        int selectedSpawn;

        for (int i = 0; i < maxSpawns; i++)
        {
            //Picks a random spawn point from the free ones
            selectedSpawn = Random.Range(0, freeSpawns.Count);
            Instantiate(pf_Enemy, new Vector3(freeSpawns[selectedSpawn].x, freeSpawns[selectedSpawn].y, 0), Quaternion.identity);
            freeSpawns.RemoveAt(selectedSpawn);
            enemyCount++;
        }
    }


    /// <summary>
    /// Checks if all the players are dead
    /// </summary>
    /// <returns>True = all players are dead, False = not all players are dead</returns>
    private bool AreAllPlayersDead()
    {
        for (int i = 0; i < playersControllers.Count; i++)
            if (!playersControllers[i].IsDead)
                return false;

        return true;
    }


    /// <summary>
    /// Triggers game over on all players death
    /// </summary>
    private void OnAllPlayersDeath()
    {
        if (enemyCount <= 0) return;

        if (AreAllPlayersDead())
            GameOver(false);
    }


    /// <summary>
    /// After an enemy death, checks if all enemies are dead in order to trigger the game over
    /// </summary>
    private void OnEnemyDeath()
    {
        if (AreAllPlayersDead()) return;

        enemyCount--;
        if (enemyCount <= 0)
            GameOver(true);
    }


    /// <summary>
    /// Ends the game
    /// </summary>
    /// <param name="playersWin">Have the players won</param>
    private void GameOver(bool playersWin)
    {
        isGameOver = true;
        EventManager.GameOver?.Invoke(playersWin);

        string playersAlive;
        DateTime endingTime = DateTime.Now;

        gameOver.SetActive(true);
        if (playersWin)
        {
            if (GetAlivePlayers(", ", out playersAlive) > 1)
                txt_Winner.text = "Players <#" + playersColors.ToHexString() + ">" + playersAlive + "</color> WON!";
            else
                txt_Winner.text = "Player <#" + playersColors.ToHexString() + ">" + playersAlive + "</color> WON!";
        }
        else
            txt_Winner.text = "Players LOST!";

        txt_TimeTaken.text = $"Time taken\n{(endingTime - startingTime).Hours.ToString("00")}:{(endingTime - startingTime).Minutes.ToString("00")}:" +
            $"{(endingTime - startingTime).Seconds.ToString("00")}";

        Cursor.lockState = CursorLockMode.None;
    }


    /// <summary>
    /// Returns the alive players count
    /// </summary>
    /// <param name="separator">The separator to be placed between each player index</param>
    /// <param name="text">The output string containing all the alives players indices</param>
    /// <returns></returns>
    private int GetAlivePlayers(string separator, out string text)
    {
        int alivePlayersCounter = 0;
        text = "";

        for (int i = 0; i < playersControllers.Count; i++)
        {
            if (!playersControllers[i].IsDead)
            {
                alivePlayersCounter++;

                if (text != "")
                    text += separator + (i + 1);
                else
                    text += (i + 1);
            }
        }

        return alivePlayersCounter;
    }


    /// <summary>
    /// Loads the main menu
    /// </summary>
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }


    /// <summary>
    /// Restarts the current match with a similar map
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion
}
