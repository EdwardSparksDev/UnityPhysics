using UnityEngine;


public class AudioManager : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    AudioSource[] sources;
    #endregion

    #region SerializeField
    [Header("Audio")]
    [SerializeField] AudioClip ac_Pause;
    [SerializeField] AudioClip ac_Unpause;

    [Space(10), SerializeField] AudioClip ac_Victory;
    [SerializeField] AudioClip ac_Defeat;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        sources = GetComponents<AudioSource>();
    }


    private void OnEnable()
    {
        EventManager.PauseToggled += OnTogglePause;
        EventManager.GameOver += OnGameOver;
    }


    private void OnDisable()
    {
        EventManager.PauseToggled -= OnTogglePause;
        EventManager.GameOver -= OnGameOver;
    }
    #endregion


    #region Methods
    /// <summary>
    /// Plays the pause sound effect based on game status
    /// </summary>
    /// <param name="enable">Has pause been enabled</param>
    private void OnTogglePause(bool enable)
    {
        if (enable)
            sources[1].clip = ac_Pause;
        else
            sources[1].clip = ac_Unpause;

        sources[1].Play();
    }


    /// <summary>
    /// Plays the correct game over sound effect based on victory conditions
    /// </summary>
    /// <param name="victory">Have the players won</param>
    private void OnGameOver(bool victory)
    {
        if (victory)
            sources[2].clip = ac_Victory;
        else
            sources[2].clip = ac_Defeat;

        sources[2].Play();
    }
    #endregion
}
