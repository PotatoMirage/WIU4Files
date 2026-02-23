// SlingshotProjectileScript.cs
// Made by: Heiy Tan

using UnityEngine;

public class SlingshotProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float projectileDamage = 15.0f;
    public float projectileSpeed = 25.0f;
    public float gravityMultiplier = 1.0f;
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
        Destroy(gameObject, 5.0f);
    }

    // FixedUpdate is called once every physics frame
    void FixedUpdate()
    {
        rigidBody.linearVelocity += gravityMultiplier * Time.fixedDeltaTime * Physics.gravity;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            Debug.Log("Player has dealt " + projectileDamage + " damage to " + collision.gameObject.name);
        }

        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration);
        }

        Destroy(gameObject);
    }
}