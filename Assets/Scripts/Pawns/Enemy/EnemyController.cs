using UnityEngine;


public class EnemyController : MonoBehaviour, IDamageable
{
    #region Variables & Properties

    #region Local
    Rigidbody2D rb;
    CircleCollider2D col;
    AudioSource source;
    AnimatedSpriteRenderer asr;

    Vector2 dir;
    Vector2 newDir;

    bool isDead;
    #endregion

    #region SerializeField
    [Header("Movement")]
    [SerializeField] float speed;

    [Space(10), SerializeField] LayerMask wallsDetectionMask;
    [SerializeField] float detectionDistance;

    [Header("Damage")]
    [SerializeField] LayerMask damageMask;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        source = GetComponent<AudioSource>();
        asr = GetComponentInChildren<AnimatedSpriteRenderer>();
    }


    private void Start()
    {
        asr.SetAnimationState((int)E_PlayerAnimStates.DOWN, true);
        GetNewRandomDirection();
    }


    private void Update()
    {
        CheckForObstacles();
    }


    private void FixedUpdate()
    {
        Vector2 pos = rb.position;
        Vector2 translation = dir * speed * Time.fixedDeltaTime;

        rb.MovePosition(pos + translation);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((damageMask.value & (1 << collision.gameObject.layer)) > 0)
        {
            if (collision.gameObject.TryGetComponent(out IDamageable iDamage))
                iDamage.ApplyDamage();
        }
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        if ((damageMask.value & (1 << collision.gameObject.layer)) > 0)
        {
            Vector2 collisionDirection = new Vector2(Mathf.Round(collision.transform.position.x - transform.position.x),
                Mathf.Round(collision.transform.position.y - transform.position.y)).normalized;

            if (dir == collisionDirection)
                GetNewRandomDirection();

            if (collision.gameObject.TryGetComponent(out IDamageable iDamage))
                iDamage.ApplyDamage();
        }
    }
    #endregion


    #region Methods
    /// <summary>
    /// Randomizes a new direction to be picked
    /// </summary>
    private void GetNewRandomDirection()
    {
        Vector2 previousDir = dir;

        do
        {
            int random = Random.Range(0, 4);
            switch (random)
            {
                case 0:
                    newDir = Vector2.up;
                    break;

                case 1:
                    newDir = Vector2.down;
                    break;

                case 2:
                    newDir = Vector2.right;
                    break;

                case 3:
                    newDir = Vector2.left;
                    break;
            }
        } while (newDir == previousDir);

        if (!Physics2D.CircleCast(transform.position, col.radius - .1f, newDir, detectionDistance, wallsDetectionMask))
            SetDirection(newDir);
        else
            SetDirection(Vector2.zero);
    }


    /// <summary>
    /// Checks for obstacles in the current direction the enemy is facing
    /// </summary>
    private void CheckForObstacles()
    {
        if (Physics2D.CircleCast(transform.position, col.radius - .1f, dir, detectionDistance, wallsDetectionMask) || dir == Vector2.zero)
            GetNewRandomDirection();
    }


    /// <summary>
    /// Sets the new given direction
    /// </summary>
    /// <param name="newDir">The new direction</param>
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
    /// Applies damage to the enemy
    /// </summary>
    public void ApplyDamage()
    {
        if (isDead) return;
        isDead = true;

        enabled = false;
        col.enabled = false;
        rb.velocity = Vector2.zero;

        asr.SetAnimationState((int)E_PlayerAnimStates.DEAD);
        asr.SetLoop(false);

        source.Play();
        Invoke(nameof(OnDeath), 1.25f);
    }


    /// <summary>
    /// Notifies the Game Manager on enemy death
    /// </summary>
    private void OnDeath()
    {
        EventManager.EnemyDeath?.Invoke();
        Destroy(gameObject);
    }
    #endregion
}
