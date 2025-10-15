using UnityEngine;


[CreateAssetMenu(menuName = "Gameplay/Item", fileName = "Item_Data")]
public class ItemData : ScriptableObject
{
    #region Variables & Properties

    #region Local
    bool enabledBTSBLast;
    #endregion

    #region SerializeField
    [Header("Item Settings")]
    [SerializeField] Sprite spt_ItemIcon;

    [Space(20), SerializeField] int speedIncrease;

    [Space(20), SerializeField] int bombsAmountIncrease;
    [SerializeField] int blastRadiusIncrease;
    [SerializeField] bool enableBlastThroughSoftBlocks;
    [SerializeField] bool disableBlastThroughSoftBlocks;

    [Space(20), SerializeField] int maxLivesIncrease;
    [SerializeField] int livesIncrease;

    [Space(10), SerializeField] bool enableInvincibility;
    [SerializeField] float invincibilityDuration;

    [Space(20), SerializeField] AudioClip ac_Collected;
    #endregion

    #region Properties
    public Sprite Spt_ItemIcon => spt_ItemIcon;
    public int SpeedIncrease => speedIncrease;
    public int BombsAmountIncrease => bombsAmountIncrease;
    public int BlastRadiusIncrease => blastRadiusIncrease;
    public bool EnableBlastThroughSoftBlocks => enableBlastThroughSoftBlocks;
    public bool DisableBlastThroughSoftBlocks => disableBlastThroughSoftBlocks;
    public int MaxLivesIncrease => maxLivesIncrease;
    public int LivesIncrease => livesIncrease;
    public bool EnableInvincibility => enableInvincibility;
    public float InvincibilityDuration => invincibilityDuration;
    public AudioClip Ac_Collected => ac_Collected;
    #endregion


    #region Mono
    private void OnValidate()
    {
        if (enableBlastThroughSoftBlocks && !disableBlastThroughSoftBlocks)
            enabledBTSBLast = true;
        else if (!enableBlastThroughSoftBlocks && disableBlastThroughSoftBlocks)
            enabledBTSBLast = false;
        else if (enableBlastThroughSoftBlocks && disableBlastThroughSoftBlocks)
        {
            if (enabledBTSBLast)
            {
                enableBlastThroughSoftBlocks = false;
                disableBlastThroughSoftBlocks = true;
                enabledBTSBLast = false;
            }
            else
            {
                enableBlastThroughSoftBlocks = true;
                disableBlastThroughSoftBlocks = false;
                enabledBTSBLast = true;
            }
        }
    }
    #endregion

    #endregion
}
