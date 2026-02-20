using UnityEngine;

public class FliesEnemy : MonoBehaviour
{
    public float speed = 5f;
    public float chaseDuration = 4f; // Time before the flies disperse/despawn
    public int damageAmount = 1;

    private Transform target;
    private float aliveTimer = 0f;

    // Called by the FrogEnemy when spawned
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        aliveTimer += Time.deltaTime;
        if (aliveTimer >= chaseDuration)
        {
            Destroy(gameObject);
            return;
        }

        if (target != null)
        {
            // Calculate a direction towards the player, applying a small random offset 
            // every frame to simulate the erratic, creepy buzzing of a swarm.
            Vector3 erraticOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Vector3 direction = ((target.position + erraticOffset) - transform.position).normalized;

            transform.position += direction * speed * Time.deltaTime;

            // Look towards the player
            transform.LookAt(target.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Flies attacked the player!");
            // Call the player's TakeDamage function here

            // Destroy flies after dealing damage
            Destroy(gameObject);
        }
    }
}