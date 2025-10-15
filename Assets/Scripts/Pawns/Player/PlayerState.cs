using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerState : MonoBehaviour, IDamageable
{
    #region Variables & Properties

    #region Local
    PlayerController playerController;
    PlayerDisplay playerDisplay;
    Gamepad connectedGamepad;

    WaitForSeconds invincibilityDelay;
    WaitForSeconds flickeringDelay;
    SpriteRenderer spriteRenderer;
    AudioSource source;
    Coroutine invincibilityCR;

    bool isDead;
    bool isInvincible;
    int maxLives;
    bool enableGamepadRumble;
    #endregion

    #region SerializeField
    [Header("Health")]
    [SerializeField] int lives;
    [SerializeField] float hitInvincibilityWindow;

    [Header("Animation")]
    [SerializeField] float spriteFlickeringTickrate;

    [Header("Gamepad")]
    [SerializeField] float minRumblingIntensity;
    [SerializeField] float minRumblingDuration;

    [Header("Audio")]
    [SerializeField] AudioClip ac_PlayerDeath;
    #endregion

    #region Properties
    public bool IsDead => isDead;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerDisplay = GetComponent<PlayerDisplay>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        source = GetComponent<AudioSource>();

        invincibilityDelay = new WaitForSeconds(hitInvincibilityWindow);
        flickeringDelay = new WaitForSeconds(spriteFlickeringTickrate);

        maxLives = lives;
        playerDisplay.OnUpdateLives(lives, maxLives);
    }
    #endregion


    #region Methods
    /// <summary>
    /// Binds the used gamepad with the player state for later use
    /// </summary>
    /// <param name="isGamepad">Is the player using a gamepad</param>
    /// <param name="pad">The used gamepad</param>
    public void OnDeviceBound(bool isGamepad, Gamepad pad)
    {
        enableGamepadRumble = isGamepad;
        connectedGamepad = pad;
    }


    /// <summary>
    /// Applies damage to the player
    /// </summary>
    public void ApplyDamage()
    {
        if (isDead || isInvincible) return;

        lives--;
        playerDisplay.OnUpdateLives(lives, maxLives);
        if (enableGamepadRumble)
            StartCoroutine(GamepadRumbleCR(lives, maxLives));

        if (lives <= 0)
        {
            source.clip = ac_PlayerDeath;
            source.Play();

            isDead = true;
            playerController.OnDeath();
        }
        else
        {
            source.Play();

            if (invincibilityCR != null)
                StopCoroutine(invincibilityCR);
            invincibilityCR = StartCoroutine(InvincibilityCR(invincibilityDelay));
        }
    }


    /// <summary>
    /// Enables gamepad rumbling
    /// </summary>
    /// <param name="lives">The current player lives</param>
    /// <param name="maxLives">The player max lives</param>
    /// <returns></returns>
    private IEnumerator GamepadRumbleCR(int lives, int maxLives)
    {
        float rumbleIntensity = 1f - (float)lives / maxLives;
        connectedGamepad.SetMotorSpeeds(Mathf.Clamp(rumbleIntensity, minRumblingIntensity, 1f), Mathf.Clamp(rumbleIntensity, minRumblingIntensity, 1f));
        yield return new WaitForSeconds(Mathf.Clamp(rumbleIntensity / 3f, minRumblingDuration, 1f));
        connectedGamepad.SetMotorSpeeds(0f, 0f);
    }


    /// <summary>
    /// Enables player invincibility
    /// </summary>
    /// <param name="delay">The invincibility delay</param>
    /// <param name="flickerSprite">Should the player sprite flicker</param>
    /// <param name="enablePowerUp">Should invincibility be turned on by a power up</param>
    /// <returns></returns>
    private IEnumerator InvincibilityCR(WaitForSeconds delay, bool flickerSprite = true, bool enablePowerUp = false)
    {
        isInvincible = true;
        spriteRenderer.enabled = true;
        spriteRenderer.color = Color.white;

        if (enablePowerUp)
            spriteRenderer.color = Color.yellow;

        if (flickerSprite)
            StartCoroutine(SpriteFlickeringCR());

        yield return delay;

        if (enablePowerUp)
            spriteRenderer.color = Color.white;

        isInvincible = false;
    }


    /// <summary>
    /// Enables sprite flickering
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpriteFlickeringCR()
    {
        float t = 0;

        while (t < hitInvincibilityWindow)
        {
            t += spriteFlickeringTickrate;
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return flickeringDelay;
        }

        spriteRenderer.enabled = true;
    }


    /// <summary>
    /// Adds <extraLives> to the player health
    /// </summary>
    /// <param name="extraLives">The amount of lives to be added</param>
    public void AddLives(int extraLives)
    {
        lives = Mathf.Clamp(lives + extraLives, 0, maxLives);

        if (lives <= 0)
        {
            isDead = true;
            playerController.OnDeath();
        }

        playerDisplay.OnUpdateLives(lives, maxLives);
    }


    /// <summary>
    /// Adds <extraLives> to the max player health
    /// </summary>
    /// <param name="extraLives">The amount of max lives to be added</param>
    /// <param name="livesLimit">The max lives limit</param>
    public void AddMaxLives(int extraLives, int livesLimit)
    {
        maxLives = Mathf.Clamp(maxLives + extraLives, 1, livesLimit);
        AddLives(extraLives);

        playerDisplay.OnUpdateLives(lives, maxLives);
    }


    /// <summary>
    /// Enables invincibility through the use of a power up
    /// </summary>
    /// <param name="powerUpDelay">The invincibility duration</param>
    public void EnableInvincibility(WaitForSeconds powerUpDelay)
    {
        if (invincibilityCR != null)
            StopCoroutine(invincibilityCR);
        invincibilityCR = StartCoroutine(InvincibilityCR(powerUpDelay, false, true));
    }
    #endregion
}
