using UnityEngine;

public class SlingshotProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float projectileDamage = 15.0f;
    public float projectileSpeed = 25.0f;
    public float gravityMultiplier = 1.0f;
    public GameObject hitEffectPrefab;
    public EnemyHealth enemyHealthScript;

    private Rigidbody rigidBody;

    public void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        enemyHealthScript = UnityEngine.Object.FindFirstObjectByType<EnemyHealth>();
    }

    public void Start()
    {
        rigidBody.linearVelocity = transform.forward * projectileSpeed;
        Destroy(gameObject, 5.0f);
    }

    public void FixedUpdate()
    {
        rigidBody.linearVelocity += gravityMultiplier * Time.fixedDeltaTime * Physics.gravity;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnemyProjectiles"))
        {
            Destroy(collision.gameObject);

            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
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
                Debug.Log($"Player Projectile has dealt {projectileDamage} damage to {collision.gameObject.name}");
            }

            WolfMovementScript wolfMovement = collision.gameObject.GetComponent<WolfMovementScript>();
            if (wolfMovement != null)
            {
                wolfMovement.TakeDamage(projectileDamage);

                if (wolfMovement.IsRetreating)
                {
                    wolfMovement.TriggerStunned();
                }

                Debug.Log($"Player Projectile has dealt {projectileDamage} damage to {collision.gameObject.name}");
            }
        }

        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration);
        }

        Destroy(gameObject);
    }
}