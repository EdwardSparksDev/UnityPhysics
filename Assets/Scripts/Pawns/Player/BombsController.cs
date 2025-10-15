using UnityEngine;
using UnityEngine.InputSystem;


public class BombsController : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    PlayerInput playerInput;
    WaitForSeconds fuseDelay;

    int bombsRemaining;
    bool enableBlastThroughSoftBlocks;
    #endregion

    #region SerializeField
    [Header("Bomb")]
    [SerializeField] Bomb pf_Bomb;
    [SerializeField] LayerMask bombPlacementBlockingMask;
    [SerializeField] float fuseTime;
    [SerializeField] int bombsAmount;
    [SerializeField] float maxBombSlideDistance;

    [Space(25), Header("Explosion")]
    [SerializeField] Explosion pf_Explosion;
    [SerializeField] LayerMask defaultExplosionBlockingMask;
    [SerializeField] LayerMask passThroughExplosionMask;
    [SerializeField] float explosionDuration;
    [SerializeField] int explosionRadius;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        fuseDelay = new WaitForSeconds(fuseTime);

        bombsRemaining = bombsAmount;
        passThroughExplosionMask = defaultExplosionBlockingMask & ~passThroughExplosionMask;
    }


    private void OnEnable()
    {
        if (playerInput != null)
            playerInput.Player.DropBomb.started += OnDropBomb;
    }


    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.Player.DropBomb.started -= OnDropBomb;
    }
    #endregion


    #region Methods
    /// <summary>
    /// Binds the playerController controls to this BombsController
    /// </summary>
    /// <param name="input">The PlayerInput to be bound</param>
    public void BindControls(PlayerInput input)
    {
        playerInput = input;
        playerInput.Player.DropBomb.started += OnDropBomb;
    }


    /// <summary>
    /// Drops a bomb on input
    /// </summary>
    /// <param name="context">The input context</param>
    private void OnDropBomb(InputAction.CallbackContext context)
    {
        if (bombsRemaining > 0)
            DropBomb();
    }


    /// <summary>
    /// Spawns a bomb on the ground an initializes it
    /// </summary>
    private void DropBomb()
    {
        Vector2 pos = transform.position;
        pos.x = Mathf.Round(pos.x);
        pos.y = Mathf.Round(pos.y);

        if ((Physics2D.OverlapCircle(pos, .4f, bombPlacementBlockingMask)) != null)
            return;

        Bomb bomb = Instantiate(pf_Bomb, pos, Quaternion.identity);

        ST_BombSettings bombSettings = new ST_BombSettings()
        {
            explosionRadius = explosionRadius,
            explosionDuration = explosionDuration,
            maxBombSlideDistance = maxBombSlideDistance,
            fuseDelay = fuseDelay,
            bypassSoftBlocks = enableBlastThroughSoftBlocks,
            defaultExplosionBlockingMask = defaultExplosionBlockingMask,
            passThroughExplosionMask = passThroughExplosionMask
        };

        bomb.InitializeExplosion(this, bombSettings);
        bombsRemaining--;
    }


    /// <summary>
    /// Recovers an exploded bomb
    /// </summary>
    public void RecoverBomb()
    {
        bombsRemaining++;
    }


    /// <summary>
    /// Applies an explosion radius buff/debuff
    /// </summary>
    /// <param name="increase">The increase amount</param>
    /// <param name="min">The minimum explosion radius</param>
    /// <param name="max">The maximum explosion radius</param>
    #region PowerUps
    public void ApplyExplosionRadiusIncrease(int increase, int min, int max)
    {
        explosionRadius = Mathf.Clamp(explosionRadius + increase, min, max);
    }


    /// <summary>
    /// Applies a bomb count buff/debuff
    /// </summary>
    /// <param name="increase">The increase amount</param>
    /// <param name="min">The minimum bomb count</param>
    /// <param name="max">The maximum bomb count</param>
    public void ApplyBombAmountIncrease(int increase, int min, int max)
    {
        bombsAmount = Mathf.Clamp(bombsAmount + increase, min, max);
        bombsRemaining = Mathf.Clamp(bombsRemaining + increase, min, max);
    }


    /// <summary>
    /// Applies a bomb blast through buff/debuff
    /// </summary>
    /// <param name="enable">Should the bomb blast through be enabled or disabled</param>
    public void ApplyBlastThroughSoftBlocks(bool enable)
    {
        enableBlastThroughSoftBlocks = enable;
    }
    #endregion

    #endregion
}
