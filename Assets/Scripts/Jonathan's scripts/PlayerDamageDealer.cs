using UnityEngine;

public class PlayerDamageDealer : MonoBehaviour
{
    [SerializeField] private float weaponDamage = 25f;
    [SerializeField] private LayerMask enemyLayer;
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(weaponDamage);
            }
        }
    }
}