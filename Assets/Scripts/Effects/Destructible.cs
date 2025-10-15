using System.Collections;
using UnityEngine;


public class Destructible : MonoBehaviour
{
    #region Variables & Properties

    #region SerializeField
    [Header("Items Drop Rates")]
    [SerializeField] float destroyDelay;
    [Range(0, 1), SerializeField] float itemsSpawnChance;

    [Space(40), SerializeField] ItemPickup pf_ItemPickup;
    [SerializeField] ItemData[] spawnableItems;
    #endregion

    #endregion


    #region Mono
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(destroyDelay);

        if (spawnableItems != null && Random.value < itemsSpawnChance)
        {
            int randomIndex = Random.Range(0, spawnableItems.Length);
            Instantiate(pf_ItemPickup, transform.position, Quaternion.identity).InitializeItemPickup(spawnableItems[randomIndex]);
        }

        Destroy(gameObject);
    }
    #endregion


    #region Methods
    /// <summary>
    /// Overrides the default item spawn chance
    /// </summary>
    /// <param name="newSpawnChance">The new item spawn chance</param>
    public void ApplyNewSpawnChance(float newSpawnChance)
    {
        itemsSpawnChance = newSpawnChance;
    }
    #endregion
}
