using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    PlayerInput playerInput;
    PlayerState playerState;
    BombsController bombsController;

    Rigidbody2D rb;
    CircleCollider2D col;
    AnimatedSpriteRenderer asr;

    Vector2 dir;

    float cosPiFourths = Mathf.Sqrt(2) / 2;
    #endregion

    #region SerializeField
    [Header("Movement")]
    [SerializeField] float speed;

    [Header("Power Ups")]
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;

    [Space(10), SerializeField] int minBombsAmount;
    [SerializeField] int maxBombsAmount;

    [Space(10), SerializeField] int minBombBlastRadius;
    [SerializeField] int maxBombBlastRadius;

    [Space(10), SerializeField] int maxLives;
    #endregion

    #region Properties
    public bool IsDead => playerState.IsDead;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        asr = GetComponentInChildren<AnimatedSpriteRenderer>();

        playerState = GetComponent<PlayerState>();
        bombsController = GetComponent<BombsController>();
    }


    private void Start()
    {
        asr.SetAnimationState((int)E_PlayerAnimStates.DOWN, true);
    }


    private void OnEnable()
    {
        TogglePlayerControls(true);
    }


    private void OnDisable()
    {
        TogglePlayerControls(false);
    }


    private void FixedUpdate()
    {
        Vector2 pos = rb.position;
        Vector2 translation = dir * speed * Time.fixedDeltaTime;

        rb.MovePosition(pos + translation);
    }
    #endregion


    #region Methods
    /// <summary>
    /// Binds the give PlayerInput to this player controller
    /// </summary>
    /// <param name="newInput">The PlayerInput to be bound</param>
    /// <param name="isGamepad">Is the player using a gamepad</param>
    /// <param name="pad">The used gamepad reference</param>
    public void BindPlayerControls(PlayerInput newInput, bool isGamepad = false, Gamepad pad = null)
    {
        playerInput = newInput;
        TogglePlayerControls(true);
        bombsController.BindControls(playerInput);
        playerState.OnDeviceBound(isGamepad, pad);
        playerInput.UI.Enable();
    }


    /// <summary>
    /// Toggles the player input action map
    /// </summary>
    /// <param name="enable">Should the player input action map be enabled or disabled</param>
    public void TogglePlayerActionMap(bool enable)
    {
        if (enable)
            playerInput.Player.Enable();
        else
            playerInput.Player.Disable();
    }


    /// <summary>
    /// Toggles the players controls
    /// </summary>
    /// <param name="enable">Should the player controls be enabled or disabled</param>
    private void TogglePlayerControls(bool enable)
    {
        if (playerInput == null) return;

        if (enable)
        {
            playerInput.Player.Enable();
            playerInput.Player.Move.performed += OnMovePerformed;
            playerInput.Player.Move.canceled += OnMoveCancelled;
        }
        else
        {
            playerInput.Player.Disable();
            playerInput.Player.Move.performed -= OnMovePerformed;
            playerInput.Player.Move.canceled -= OnMoveCancelled;
        }
    }


    /// <summary>
    /// Allows the player to move
    /// </summary>
    /// <param name="context">The input context</param>
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (input == Vector2.zero)
            return;

        float up = Vector2.Dot(input, Vector2.up);
        float right = Vector2.Dot(input, Vector2.right);

        if (up >= cosPiFourths)
            SetDirection(Vector2.up);
        else if (up <= -cosPiFourths)
            SetDirection(Vector2.down);
        else if (right >= cosPiFourths)
            SetDirection(Vector2.right);
        else
            SetDirection(Vector2.left);
    }


    /// <summary>
    /// Sets the player direction to idle on movement stop
    /// </summary>
    /// <param name="context">The input context</param>
    private void OnMoveCancelled(InputAction.CallbackContext context)
    {
        SetDirection(Vector2.zero);
    }


    /// <summary>
    /// Sets the picked player direction
    /// </summary>
    /// <param name="newDir">The new player direction</param>
    private void SetDirection(Vector2 newDir)
    {
        dir = newDir;

        if (newDir == Vector2.zero)
            asr.SetIdle(true);
        else if (newDir == Vector2.up)
            asr.SetAnimationState((int)E_PlayerAnimStates.UP);
        else if (newDir == Vector2.down)
            asr.SetAnimationState((int)E_PlayerAnimStates.DOWN);
        else if (newDir == Vector2.right)
            asr.SetAnimationState((int)E_PlayerAnimStates.RIGHT);
        else
            asr.SetAnimationState((int)E_PlayerAnimStates.LEFT);
    }


    /// <summary>
    /// Kills the player and plays death animation
    /// </summary>
    public void OnDeath()
    {
        enabled = false;
        playerState.enabled = false;
        bombsController.enabled = false;
        col.enabled = false;
        rb.velocity = Vector2.zero;

        asr.SetAnimationState((int)E_PlayerAnimStates.DEAD);
        asr.SetLoop(false);

        Invoke(nameof(DisablePlayer), 1.25f);
    }


    /// <summary>
    /// Notifies the Game Manager of a player death
    /// </summary>
    private void DisablePlayer()
    {
        EventManager.PlayerDeath?.Invoke();
        gameObject.SetActive(false);
    }


    /// <summary>
    /// Applies item effects on pickup
    /// </summary>
    /// <param name="item">The picked up item</param>
    public void OnItemPickup(ItemData item)
    {
        CheckItemMovementEffects(item);
        CheckItemBombsEffects(item);
        CheckItemHealthEffects(item);
    }


    /// <summary>
    /// Applies item movement buffs/debuffs
    /// </summary>
    /// <param name="item">The picked up item</param>
    private void CheckItemMovementEffects(ItemData item)
    {
        if (item.SpeedIncrease != 0)
            speed = Mathf.Clamp(speed + item.SpeedIncrease, minSpeed, maxSpeed);
    }


    /// <summary>
    /// Applies the item bomb buffs/debuffs
    /// </summary>
    /// <param name="item">The picked up item</param>
    private void CheckItemBombsEffects(ItemData item)
    {
        if (item.BombsAmountIncrease != 0)
            bombsController.ApplyBombAmountIncrease(item.BombsAmountIncrease, minBombsAmount, maxBombsAmount);

        if (item.BlastRadiusIncrease != 0)
            bombsController.ApplyExplosionRadiusIncrease(item.BlastRadiusIncrease, minBombBlastRadius, maxBombBlastRadius);

        if (item.EnableBlastThroughSoftBlocks)
            bombsController.ApplyBlastThroughSoftBlocks(true);
        else if (item.DisableBlastThroughSoftBlocks)
            bombsController.ApplyBlastThroughSoftBlocks(false);
    }


    /// <summary>
    /// Applies the item health buffs/debuffs
    /// </summary>
    /// <param name="item">The picked up item</param>
    private void CheckItemHealthEffects(ItemData item)
    {
        if (item.MaxLivesIncrease > 0)
            playerState.AddMaxLives(item.MaxLivesIncrease, maxLives);

        if (item.LivesIncrease > 0)
            playerState.AddLives(item.LivesIncrease);

        if (item.EnableInvincibility)
            playerState.EnableInvincibility(new WaitForSeconds(item.InvincibilityDuration));
    }
    #endregion
}
