using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    [Header("Hitbox Setup")]
    public Collider attackHitbox;

    [Header("VFX")]
    public EnemyVFX enemyVFX;

    private void Start()
    {
        if (attackHitbox != null)
        {
            attackHitbox.enabled = false;
        }

        if (enemyVFX == null)
            enemyVFX = GetComponentInParent<EnemyVFX>();
    }

    // Animation to call this exact method
    public void EnableHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = true;

        if (enemyVFX != null)
            enemyVFX.PlayAttackImpact(transform.position);
    }

    // Animation to call this when the attack ends
    public void DisableHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = false;
    }
}