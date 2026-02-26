using UnityEngine;

public class SpiderProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float moveSpeed = 10f;
    public float lifetime = 10f;
    public float damageAmount = 10f;

    [Header("Effects")]
    public GameObject impactEffect;

    [Header("Layer Settings")]
    public LayerMask environmentLayer;
    public LayerMask enemyLayer;

    private Rigidbody rb;
    private Vector3 direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    public void Init(Vector3 targetDirection, Collider spiderCollider)
    {
        direction = targetDirection.normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        rb.linearVelocity = direction * moveSpeed;



        // *** Ignore collision with spider so it doesnt deflect on spawn ***
        if (spiderCollider != null)
        {
            Collider[] projectileColliders = GetComponents<Collider>();
            foreach (Collider col in projectileColliders)
                Physics.IgnoreCollision(col, spiderCollider);
        }

        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Spider projectile hit player for " + damageAmount + " damage!");
            collision.gameObject.GetComponent<PlayerMovementScript>().ChangeHealth(-(int)damageAmount);
            SpawnImpact(collision);
            Destroy(gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("PlayerProjectile"))
        {
            Destroy(collision.gameObject);
            SpawnImpact(collision);
            Destroy(gameObject);
            return;
        }

        int combinedMask = environmentLayer.value | enemyLayer.value;
        if ((combinedMask & (1 << collision.gameObject.layer)) != 0)
        {
            SpawnImpact(collision);
            Destroy(gameObject);
        }
    }

    private void SpawnImpact(Collision collision)
    {
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(
                impactEffect,
                collision.contacts[0].point,
                Quaternion.LookRotation(collision.contacts[0].normal)
            );
            Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration);
        }
    }
}