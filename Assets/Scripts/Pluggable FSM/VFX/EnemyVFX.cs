using UnityEngine;

public class EnemyVFX : MonoBehaviour
{
    [Header("Base VFX")]
    public GameObject hitEffect;
    public GameObject deathEffect;
    public GameObject jumpTakeoffEffect;
    public GameObject jumpLandEffect;
    public GameObject attackImpactEffect;

    [Header("Big Mushroom VFX")]
    public ParticleSystem spearChargeEffect;

    [Header("Trail VFX")]
    public ParticleSystem sporeTrailLeft;
    public ParticleSystem sporeTrailRight;

    [Header("Spawn Points")]
    public Transform attackVFXPoint;

    public void PlayHitEffect()
    {
        if (hitEffect != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.7f;
            Instantiate(hitEffect, spawnPos, Quaternion.identity);
        }
    }

    public void PlayDeathEffect()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);
    }

    public void PlayJumpTakeoff()
    {
        if (jumpTakeoffEffect != null)
            Instantiate(jumpTakeoffEffect, transform.position, Quaternion.identity);
    }

    public void PlayJumpLand()
    {
        if (jumpLandEffect != null)
            Instantiate(jumpLandEffect, transform.position, Quaternion.identity);
    }

    public void PlayAttackImpact(Vector3 position)
    {
        if (attackImpactEffect != null)
        {
            Vector3 spawnPos = attackVFXPoint != null ? attackVFXPoint.position : position;
            Instantiate(attackImpactEffect, spawnPos, Quaternion.identity);
        }
    }

    public void StartTrail()
    {
        if (sporeTrailLeft != null && !sporeTrailLeft.isPlaying)
            sporeTrailLeft.Play();
        if (sporeTrailRight != null && !sporeTrailRight.isPlaying)
            sporeTrailRight.Play();
    }

    public void StopTrail()
    {
        if (sporeTrailLeft != null)
            sporeTrailLeft.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (sporeTrailRight != null)
            sporeTrailRight.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void StartSpearCharge()
    {
        if (spearChargeEffect != null && !spearChargeEffect.isPlaying)
            spearChargeEffect.Play();
    }

    public void StopSpearCharge()
    {
        if (spearChargeEffect != null)
            spearChargeEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}