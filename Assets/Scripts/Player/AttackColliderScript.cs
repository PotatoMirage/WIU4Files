using UnityEngine;
using System.Collections.Generic;

public class AttackColliderScript : MonoBehaviour
{
    [Header("Attack Collider Settings")]
    public float meleeDamage = 40.0f;

    private readonly List<GameObject> hitTargets = new();

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Enemies") || hitTargets.Contains(other.gameObject))
            return;

        hitTargets.Add(other.gameObject);
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(meleeDamage);
        }
        Debug.Log("Player has dealt " + meleeDamage + " damage to " + other.gameObject.name);

    }

    void OnEnable() => hitTargets.Clear();
    public void ClearHits() => hitTargets.Clear();
}