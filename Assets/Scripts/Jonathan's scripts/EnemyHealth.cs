using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class LootDrop
{
    public GameObject itemPrefab;
    public float dropWeight;
}

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private WorldSpaceHealthBar healthBar;

    [Header("Loot Settings")]
    [SerializeField] private float overallDropChance = 0.7f;
    [SerializeField] private LootDrop[] possibleDrops;

    public UnityEvent onHit;
    public UnityEvent onDeath;

    public GameObject originalPrefab;
    private float currentHealth;

    public void OnEnable()
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
        SpawnLoot();
        GameObject currentObject = gameObject;
        ObjectPoolManager.Instance.ReturnToPool(originalPrefab, currentObject);
    }

    private void SpawnLoot()
    {
        if (possibleDrops == null || possibleDrops.Length == 0)
        {
            return;
        }

        if (UnityEngine.Random.value <= overallDropChance)
        {
            float totalWeight = 0f;
            for (int i = 0; i < possibleDrops.Length; i++)
            {
                totalWeight += possibleDrops[i].dropWeight;
            }

            float randomVal = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            for (int i = 0; i < possibleDrops.Length; i++)
            {
                currentWeight += possibleDrops[i].dropWeight;
                if (randomVal <= currentWeight)
                {
                    if (possibleDrops[i].itemPrefab != null)
                    {
                        Instantiate(possibleDrops[i].itemPrefab, transform.position + Vector3.up, Quaternion.identity);
                    }
                    break;
                }
            }
        }
    }
}