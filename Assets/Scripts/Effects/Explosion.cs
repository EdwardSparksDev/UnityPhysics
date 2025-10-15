using UnityEngine;


public class Explosion : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    AnimatedSpriteRenderer asr;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        asr = GetComponentInChildren<AnimatedSpriteRenderer>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable lastIDamageable))
            lastIDamageable.ApplyDamage();
    }
    #endregion


    #region Methods
    /// <summary>
    /// Sets the current explosion animation to the one given
    /// </summary>
    /// <param name="state">The new explosion animation state</param>
    public void SetActiveAnimState(E_ExplosionAnimStates state)
    {
        asr.SetAnimationState((int)state);
    }



    /// <summary>
    /// Sets the explosion orientation
    /// </summary>
    /// <param name="dir">The orientation direction</param>
    public void SetDirection(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }


    /// <summary>
    /// Delays the explosion destruction
    /// </summary>
    /// <param name="seconds">How much time to wait before destroying the explosion</param>
    public void DestroyAfter(float seconds)
    {
        Destroy(gameObject, seconds);
    }
    #endregion
}
