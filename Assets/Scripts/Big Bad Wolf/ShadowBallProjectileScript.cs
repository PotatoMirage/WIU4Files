using UnityEngine;

public class ShadowBallProjectileScript : MonoBehaviour
{
    [Header("Shadow Ball Settings")]
    public float projectileSpeed = 18.0f;
    public float projectileDamage = 34.0f;
    public GameObject hitEffectPrefab;

    private Rigidbody rigidBody;

    // Awake is called when loading an instance of a script component
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody.linearVelocity = transform.forward * projectileSpeed;
        Destroy(gameObject, 6.0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<WolfMovementScript>() != null)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovementScript playerMovement = collision.gameObject.GetComponent<PlayerMovementScript>();

            if (playerMovement == null || playerMovement.IsRolling || playerMovement.IsDead)
            {
                SpawnHitEffect();
                Destroy(gameObject);
                return;
            }

            playerMovement.ChangeHealth(-(int)projectileDamage);
            Debug.Log("Shadow Ball has dealt " + projectileDamage + " damage to player");
        }

        SpawnHitEffect();
        Destroy(gameObject);
    }

    void SpawnHitEffect()
    {
        if (hitEffectPrefab == null)
            return;

        GameObject effect = Instantiate(hitEffectPrefab, transform.position, transform.rotation);
        Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration);
    }
}