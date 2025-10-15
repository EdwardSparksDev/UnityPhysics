using UnityEngine;


public struct ST_BombSettings
{
    public int explosionRadius;
    public float explosionDuration;
    public float maxBombSlideDistance;
    public WaitForSeconds fuseDelay;
    public bool bypassSoftBlocks;
    public LayerMask defaultExplosionBlockingMask;
    public LayerMask passThroughExplosionMask;
}