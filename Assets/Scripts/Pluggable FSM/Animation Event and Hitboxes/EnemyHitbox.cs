using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public float damageAmount = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Enemy smashed the player for " + damageAmount + " damage!");
            GetComponent<Collider>().enabled = false;
        }
    }
}