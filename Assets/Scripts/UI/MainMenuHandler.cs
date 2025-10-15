using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuHandler : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    const int minMapWidth = 5;
    const int maxMapWidth = 50;
    const int minMapHeight = 5;
    const int maxMapHeight = 50;
    const int minSpawnsWidth = 2;
    const int maxSpawnsWidth = 100;
    const int minPlayers = 1;
    const int maxPlayers = 4;

    int mapWidth;
    int mapHeight;
    int softBlocksSpawnProbability;
    int spawnsWidth;
    bool spawnProtection;
    int itemsDropRate;
    int enemyAmount;
    int playersCount;
    #endregion

    #region SerializeField
    [Header("Settings")]
    [Range(0, 1), SerializeField] float gamepadsRumbleSpeed;
    [SerializeField] float gamepadsRumbleDuration;

    [Header("References")]
    [SerializeField] GameObject settingsMenu;
    [SerializeField] Image img_LoadingFill;

    [Space(10), SerializeField] TMP_InputField if_MapWidth;
    [SerializeField] TMP_InputField if_MapHeight;

    [Space(10), SerializeField] Slider sl_SoftBlocksSpawnProb;
    [SerializeField] TMP_Text txt_SoftBlocksSpawnProb;
    [SerializeField] TMP_InputField if_SpawnsWidth;
    [SerializeField] Toggle tg_SpawnProtection;

    [Space(10), SerializeField] Slider sl_ItemsDropRate;
    [SerializeField] TMP_Text txt_ItemsDropRate;
    [SerializeField] TMP_Dropdown dd_EnemyAmount;

    [Space(10), SerializeField] TMP_InputField if_PlayersNumber;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        LoadSettingsFromSave();
    }


    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnCheckGamepadsConnections;
    }


    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnCheckGamepadsConnections;
    }
    #endregion


    #region Methods
    /// <summary>
    /// Loads settings from Game Instance
    /// </summary>
    private void LoadSettingsFromSave()
    {
        if (GameInstance.DataLoaded)
        {
            GameInstance.LoadGameSettings(out mapWidth, out mapHeight, out softBlocksSpawnProbability, out spawnsWidth, out spawnProtection, out itemsDropRate,
                out enemyAmount, out playersCount);

            if_MapWidth.text = mapWidth.ToString();
            if_MapHeight.text = mapHeight.ToString();

            sl_SoftBlocksSpawnProb.value = softBlocksSpawnProbability / 100f;
            txt_SoftBlocksSpawnProb.text = $"{softBlocksSpawnProbability}%";

            if_SpawnsWidth.text = spawnsWidth.ToString();
            tg_SpawnProtection.isOn = spawnProtection;

            sl_ItemsDropRate.value = itemsDropRate / 100f;
            txt_ItemsDropRate.text = $"{itemsDropRate}%";

            dd_EnemyAmount.value = enemyAmount;
            if_PlayersNumber.text = playersCount.ToString();
            CheckActivePlayersCount();
        }
        else
            playersCount = minPlayers;
    }


    /// <summary>
    /// Checks and clamps map width input
    /// </summary>
    /// <param name="width">The inserted map width</param>
    public void CheckMapWidthRange(string width)
    {
        if (width != "")
            if_MapWidth.text = $"{Mathf.Clamp(int.Parse(width), minMapWidth, maxMapWidth)}";
    }


    /// <summary>
    /// Checks and clamps map heaight input
    /// </summary>
    /// <param name="height">The inserted map height</param>
    public void CheckMapHeightRange(string height)
    {
        if (height != "")
            if_MapHeight.text = $"{Mathf.Clamp(int.Parse(height), minMapHeight, maxMapHeight)}";
    }


    /// <summary>
    /// Checks and clamps map spawns width input
    /// </summary>
    /// <param name="width">The inserted map spawns width</param>
    public void CheckSpawnsWidthRange(string width)
    {
        if (width != "")
            if_SpawnsWidth.text = $"{Mathf.Clamp(int.Parse(width), minSpawnsWidth, maxSpawnsWidth)}";
    }


    /// <summary>
    /// Updates soft blocks slider text
    /// </summary>
    /// <param name="value">The soft blocks slider value</param>
    public void UpdateSoftBlocksSlider(float value)
    {
        txt_SoftBlocksSpawnProb.text = $"{(int)(value * 100)}%";
    }


    /// <summary>
    /// Updates the items drop rate slider text
    /// </summary>
    /// <param name="value">The items drop rate slider value</param>
    public void UpdateItemsDropRateSlider(float value)
    {
        txt_ItemsDropRate.text = $"{(int)(value * 100)}%";
    }


    /// <summary>
    /// Updates the current active players count
    /// </summary>
    /// <param name="increase">The player count increase</param>
    public void UpdatePlayersCount(int increase)
    {
        if (playersCount + increase > minPlayers + Gamepad.all.Count) return;

        playersCount = Mathf.Clamp(playersCount + increase, minPlayers, maxPlayers);
        if_PlayersNumber.text = playersCount.ToString();
    }


    /// <summary>
    /// Checks for disconnected gamepads
    /// </summary>
    /// <param name="device">The device changed</param>
    /// <param name="status">The device status</param>
    private void OnCheckGamepadsConnections(InputDevice device, InputDeviceChange status)
    {
        if (status == InputDeviceChange.Disconnected)
            CheckActivePlayersCount();
    }


    /// <summary>
    /// Checks if the current player count is updated
    /// </summary>
    private void CheckActivePlayersCount()
    {
        if (playersCount > minPlayers + Gamepad.all.Count)
        {
            playersCount = Mathf.Clamp(minPlayers + Gamepad.all.Count, minPlayers, maxPlayers);
            if_PlayersNumber.text = playersCount.ToString();
        }
    }


    /// <summary>
    /// Starts loading the game scene after saving the required settings
    /// </summary>
    public void StartGame()
    {
        int width;
        if (if_MapWidth.text != "")
            width = int.Parse(if_MapWidth.text);
        else
            width = Random.Range(minMapWidth, maxMapWidth + 1);

        int height;
        if (if_MapHeight.text != "")
            height = int.Parse(if_MapHeight.text);
        else
            height = Random.Range(minMapHeight, maxMapHeight + 1);

        int spawn;
        if (if_SpawnsWidth.text != "")
            spawn = int.Parse(if_SpawnsWidth.text);
        else
            spawn = Random.Range(minSpawnsWidth, maxSpawnsWidth + 1);

        GameInstance.SaveGameSettings(width, height,
            int.Parse(txt_SoftBlocksSpawnProb.text.Remove(txt_SoftBlocksSpawnProb.text.Length - 1)), spawn, tg_SpawnProtection.isOn,
            int.Parse(txt_ItemsDropRate.text.Remove(txt_ItemsDropRate.text.Length - 1)), dd_EnemyAmount.value, int.Parse(if_PlayersNumber.text));

        StartCoroutine(LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1));
    }


    /// <summary>
    /// Loads the game scene asynchronously
    /// </summary>
    /// <param name="sceneIndex">The scene index to be loaded</param>
    /// <returns></returns>
    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        settingsMenu.gameObject.SetActive(false);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);

        float progress;
        while (!op.isDone)
        {
            progress = Mathf.Clamp01(op.progress);
            img_LoadingFill.fillAmount = progress;

            yield return null;
        }
    }


    /// <summary>
    /// Quits the game
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
    #endregion
}
