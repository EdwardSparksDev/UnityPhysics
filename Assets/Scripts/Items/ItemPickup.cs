using System.Collections;
using UnityEngine;


public class ItemPickup : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    BoxCollider2D col;
    SpriteRenderer rdr;
    AudioSource source;

    ItemData itemData;
    #endregion

    #region SerializeField
    [Header("Settings")]
    [SerializeField] LayerMask pickupMask;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        rdr = GetComponent<SpriteRenderer>();
        source = GetComponent<AudioSource>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((pickupMask.value & (1 << collision.gameObject.layer)) > 0)
            OnItemPickup(collision.gameObject);
    }
    #endregion


    #region Methods
    /// <summary>
    /// Initializes the item with its new data
    /// </summary>
    /// <param name="item">The new item data</param>
    public void InitializeItemPickup(ItemData item)
    {
        itemData = item;
        GetComponent<SpriteRenderer>().sprite = item.Spt_ItemIcon;
        source.clip = itemData.Ac_Collected;
    }


    /// <summary>
    /// Collects the item by giving it to the associated player controller
    /// </summary>
    /// <param name="actor">The gameObject triggering the interaction</param>
    private void OnItemPickup(GameObject actor)
    {
        PlayerController playerController;
        if (actor.TryGetComponent<PlayerController>(out playerController))
            playerController.OnItemPickup(itemData);

        col.enabled = false;
        rdr.enabled = false;
        source.Play();

        StartCoroutine(DestroyItemDelayedCR());
    }


    /// <summary>
    /// Destroys the item after a set amount of time
    /// </summary>
    /// <param name="delay">The destruction delay</param>
    /// <returns></returns>
    private IEnumerator DestroyItemDelayedCR(float delay = 1f)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
    #endregion
}
