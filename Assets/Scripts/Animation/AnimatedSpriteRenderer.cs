using UnityEngine;


public class AnimatedSpriteRenderer : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    SpriteRenderer spriteRenderer;

    int activeAnimIndex;
    int animationFrame;
    bool idle;
    #endregion

    #region SerializeField
    [Header("Settings")]
    [SerializeField] float animationTime;
    [SerializeField] bool loop = true;

    [Space(15), Header("Animations")]
    [SerializeField] SerializableSpritesMatrix animStatesSprites;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void OnEnable()
    {
        spriteRenderer.enabled = true;
    }


    private void OnDisable()
    {
        spriteRenderer.enabled = false;
    }


    private void Start()
    {
        if (animStatesSprites.Length > 0 && animStatesSprites[activeAnimIndex] != null)
            InvokeRepeating(nameof(NextFrame), animationTime, animationTime);
        else
            Debug.LogError("Animation matrix not initialized!");
    }
    #endregion


    #region Methods
    /// <summary>
    /// Sets the sprites array to be used for animating
    /// </summary>
    /// <param name="index">The sprites array to be used</param>
    /// <param name="idle">Should the animation be set to idle</param>
    public void SetAnimationState(int index, bool idle = false)
    {
        if (activeAnimIndex == index && this.idle == idle)
            return;

        activeAnimIndex = index;
        animationFrame = 0;
        this.idle = idle;

        if (animStatesSprites[activeAnimIndex] != null)
            spriteRenderer.sprite = animStatesSprites[activeAnimIndex][0];
    }


    /// <summary>
    /// Sets the current sprites array to be played in idle mode
    /// </summary>
    /// <param name="enable">Should idle mode be on/off</param>
    public void SetIdle(bool enable)
    {
        idle = enable;
    }


    /// <summary>
    /// Sets the current sprites array to be played in loop
    /// </summary>
    /// <param name="enable">Should loop mode be on/off</param>
    public void SetLoop(bool enable)
    {
        loop = enable;
    }


    /// <summary>
    /// Cycles through the current active sprites array a plays the corresponding animation
    /// </summary>
    private void NextFrame()
    {
        animationFrame++;

        if (loop && animationFrame >= animStatesSprites[activeAnimIndex].Length)
            animationFrame = 0;

        if (idle)
            spriteRenderer.sprite = animStatesSprites[activeAnimIndex, 0];
        else if (animationFrame >= 0 && animationFrame < animStatesSprites.GetLength(activeAnimIndex))
            spriteRenderer.sprite = animStatesSprites[activeAnimIndex, animationFrame];
    }
    #endregion
}
