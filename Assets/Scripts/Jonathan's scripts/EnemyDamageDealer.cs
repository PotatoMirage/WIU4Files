using UnityEngine;

public class EnemyDamageDealer : MonoBehaviour
{
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private LayerMask playerLayer;
    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            PlayerMovementScript playerMovement = collision.gameObject.GetComponent<PlayerMovementScript>();

            if (playerMovement != null)
            {
                playerMovement.ChangeHealth(-attackDamage);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            PlayerMovementScript playerMovement = other.gameObject.GetComponent<PlayerMovementScript>();

            if (playerMovement != null)
            {
                playerMovement.ChangeHealth(-attackDamage);
            }
        }
    }
}