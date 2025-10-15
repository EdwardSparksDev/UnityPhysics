using System.Collections;
using UnityEngine;


public class Bomb : MonoBehaviour, IDamageable
{
    #region Variables & Properties

    #region Local
    BombsController bombsController;
    SpriteRenderer rdr;
    Rigidbody2D rb;
    CircleCollider2D col;
    AudioSource source;
    Coroutine explosionCR;

    LayerMask defaultExplosionBlockingMask;
    LayerMask passThroughExplosionMask;

    Vector3 contactPosition;

    bool bypassSoftBlocks;
    int explosionRadius;
    float explosionDuration;
    float maxSlideDistance;
    float currentSlideDistance;
    bool freeSliding;
    bool chained;
    #endregion

    #region SerializeField
    [Header("Bomb")]
    [SerializeField] LayerMask collisionEnablerMask;
    [SerializeField] LayerMask slidingMask;
    [SerializeField] float chainExplosionDelay;

    [Header("Explosion")]
    [SerializeField] Explosion pf_Explosion;
    [SerializeField] AudioClip ac_Explosion;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        source = GetComponent<AudioSource>();
    }


    private void Update()
    {
        if (freeSliding)
        {
            currentSlideDistance = (contactPosition - transform.position).sqrMagnitude;
            if (currentSlideDistance >= Mathf.Pow(maxSlideDistance, 2))
            {
                freeSliding = false;
                rb.velocity = Vector3.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((collisionEnablerMask.value & (1 << collision.gameObject.layer)) > 0)
            GetComponent<CircleCollider2D>().isTrigger = false;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((slidingMask.value & (1 << collision.gameObject.layer)) > 0)
        {
            freeSliding = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        if ((slidingMask.value & (1 << collision.gameObject.layer)) > 0)
        {
            contactPosition = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
            currentSlideDistance = 0;
            freeSliding = true;
        }
    }
    #endregion


    #region Methods
    /// <summary>
    /// Loads bomb parameters during initialization phase
    /// </summary>
    /// <param name="bombsController">The bombsController which dropped the bomb</param>
    /// <param name="bombSettings">The bombs settings to be loaded</param>
    public void InitializeExplosion(BombsController bombsController, ST_BombSettings bombSettings)
    {
        this.bombsController = bombsController;
        explosionRadius = bombSettings.explosionRadius;
        explosionDuration = bombSettings.explosionDuration;
        maxSlideDistance = bombSettings.maxBombSlideDistance;
        bypassSoftBlocks = bombSettings.bypassSoftBlocks;
        defaultExplosionBlockingMask = bombSettings.defaultExplosionBlockingMask;
        passThroughExplosionMask = bombSettings.passThroughExplosionMask;

        explosionCR = StartCoroutine(WaitForExplosion(bombSettings.fuseDelay));
    }


    /// <summary>
    /// Simply waits for the explosion to happen
    /// </summary>
    /// <param name="delay">The explosion delay</param>
    /// <returns></returns>
    private IEnumerator WaitForExplosion(WaitForSeconds delay)
    {
        yield return delay;
        StartExplosion(explosionRadius);
    }


    /// <summary>
    /// Initializes the explosion, propagating it in all 4 directions
    /// </summary>
    /// <param name="explosionRadius">The current explosion radius to be covered</param>
    private void StartExplosion(int explosionRadius)
    {
        Vector2 pos = transform.position;
        pos.x = Mathf.Round(pos.x);
        pos.y = Mathf.Round(pos.y);

        Explosion explosion = Instantiate(pf_Explosion, pos, Quaternion.identity);
        explosion.SetActiveAnimState(E_ExplosionAnimStates.START);
        explosion.DestroyAfter(explosionDuration);

        //Propagate in all 4 possible directions
        Explode(pos, Vector2.up, explosionRadius);
        Explode(pos, Vector2.down, explosionRadius);
        Explode(pos, Vector2.right, explosionRadius);
        Explode(pos, Vector2.left, explosionRadius);

        bombsController.RecoverBomb();
        StartCoroutine(DestroyBombCR());
    }


    /// <summary>
    /// The core part of the explosion which deals damage
    /// </summary>
    /// <param name="pos">The explosion position</param>
    /// <param name="dir">The explosion propagation direction</param>
    /// <param name="length">The remaining explosion length</param>
    private void Explode(Vector2 pos, Vector2 dir, int length)
    {
        if (length <= 0)
            return;

        pos += dir;

        if (Physics2D.OverlapBox(pos, Vector2.one / 2f, 0f, defaultExplosionBlockingMask))
        {
            ClearDestructible(pos);
            if (!bypassSoftBlocks || (bypassSoftBlocks && Physics2D.OverlapBox(pos, Vector2.one / 2f, 0f, passThroughExplosionMask)))
                return;
        }

        Explosion explosion = Instantiate(pf_Explosion, pos, Quaternion.identity);
        explosion.SetActiveAnimState(length > 1 ? E_ExplosionAnimStates.MIDDLE : E_ExplosionAnimStates.END);
        explosion.SetDirection(dir);
        explosion.DestroyAfter(explosionDuration);

        Explode(pos, dir, length - 1);
    }


    /// <summary>
    /// Sends a soft block clearing request
    /// </summary>
    /// <param name="pos">The position to be cleared</param>
    private void ClearDestructible(Vector2 pos)
    {
        EventManager.ClearDestructible?.Invoke(pos);
    }


    /// <summary>
    /// Applies damage to the bomb, chaining its explosion
    /// </summary>
    public void ApplyDamage()
    {
        if (chained)
            return;
        chained = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (explosionCR != null)
            StopCoroutine(explosionCR);
        StartCoroutine(WaitForExplosion(new WaitForSeconds(chainExplosionDelay)));
    }


    /// <summary>
    /// Plays bomb explosion sound and later destroys bomb
    /// </summary>
    /// <returns></returns>
    private IEnumerator DestroyBombCR()
    {
        source.clip = ac_Explosion;
        source.Play();

        rdr.enabled = false;
        col.enabled = false;
        yield return new WaitForSeconds(ac_Explosion.length);

        Destroy(gameObject);
    }
    #endregion
}
