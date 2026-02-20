using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    [Header("Hitbox Setup")]
    public Collider attackHitbox;

    private void Start()
    {
        if (attackHitbox != null)
        {
            attackHitbox.enabled = false;
        }
    }

    // Animation to call this exact method
    public void EnableHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = true;
    }

    // Animation to call this when the swing ends
    public void DisableHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = false;
    }
}