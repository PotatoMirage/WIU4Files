using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private WorldSpaceHealthBar healthBar;

    public UnityEvent onHit;
    public UnityEvent onDeath;

    public GameObject originalPrefab;
    private float currentHealth;

    private void OnEnable()
    {
        float startingHealth = maxHealth;
        currentHealth = startingHealth;

        bool hasHealthBar = healthBar != null;
        if (hasHealthBar)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }
    public void TakeDamage(float damageAmount)
    {
        float newHealth = currentHealth - damageAmount;
        currentHealth = newHealth;

        bool isDead = currentHealth < 0f;
        if (isDead)
        {
            float zeroHealth = 0f;
            currentHealth = zeroHealth;
        }

        bool hasHealthBar = healthBar != null;
        if (hasHealthBar)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        bool isAlive = currentHealth > 0f;
        if (isAlive)
        {
            onHit?.Invoke();
        }
        else
        {
            onDeath?.Invoke();
        }
    }
    public void HandleDeath()
    {
        GameObject currentObject = gameObject;
        ObjectPoolManager.Instance.ReturnToPool(originalPrefab, currentObject);
    }
}