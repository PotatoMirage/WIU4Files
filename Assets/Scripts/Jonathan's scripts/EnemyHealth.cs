using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private WorldSpaceHealthBar healthBar;

    public UnityEvent onHit;
    public UnityEvent onDeath;

    private float currentHealth;
    private void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth < 0f)
        {
            currentHealth = 0f;
        }

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        if (currentHealth > 0f)
        {
            onHit?.Invoke();
        }
        else
        {
            onDeath?.Invoke();
        }
    }
}