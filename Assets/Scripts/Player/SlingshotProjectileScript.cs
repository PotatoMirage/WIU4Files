using UnityEngine;

public class SlingshotProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    public int projectileDamage = 15;
    public float projectileSpeed = 25.0f;
    public float gravityMultiplier = 1.0f;
    public GameObject hitEffectPrefab;
    public EnemyHealth enemyHealthScript;

    private Rigidbody rigidBody;

    // Awake is called when loading an instance of a script component
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        enemyHealthScript = FindFirstObjectByType<EnemyHealth>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody.linearVelocity = transform.forward * projectileSpeed;
        Destroy(gameObject, 5.0f);
    }

    // FixedUpdate is called once every physics frame
    void FixedUpdate()
    {
        rigidBody.linearVelocity += gravityMultiplier * Time.fixedDeltaTime * Physics.gravity;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Destroy enemy projectile on impact
        if (collision.gameObject.CompareTag("EnemyProjectiles"))
        {
            Destroy(collision.gameObject); // destroy enemy projectiles

            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(
                    hitEffectPrefab,
                    collision.contacts[0].point,
                    Quaternion.LookRotation(collision.contacts[0].normal)
                );
                Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration);
            }

            Destroy(gameObject);
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(projectileDamage);
                Debug.Log($"Projectile hit {collision.gameObject.name} for {projectileDamage} damage.");
            }
        }


        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                hitEffectPrefab,
                collision.contacts[0].point,
                Quaternion.LookRotation(collision.contacts[0].normal)
            );
            Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration);
        }

        Destroy(gameObject);
    }
}