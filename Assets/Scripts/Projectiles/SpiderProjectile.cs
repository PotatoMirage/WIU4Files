using UnityEngine;

public class SpiderProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float moveSpeed = 10f;
    public float lifetime = 5f;
    public float damageAmount = 10f;

    [Header("Effects")]
    public GameObject impactEffect;

    private Vector3 direction;

    public void Init(Vector3 targetDirection)
    {
        direction = targetDirection.normalized;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Spider projectile hit player for " + damageAmount + " damage!");

            // Spawn impact effect
            if (impactEffect != null)
                Instantiate(impactEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }

        // Destroy on hitting anything else except the spider itself
        if (!other.CompareTag("Enemies"))
        {
            if (impactEffect != null)
                Instantiate(impactEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}