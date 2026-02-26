using UnityEngine;
using System.Collections.Generic;

public class WolfAttackColliderScript : MonoBehaviour
{
    [Header("Wolf Attack Collider Settings")]
    public float attackDamage = 20.0f;

    private readonly List<GameObject> hitTargets = new();

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hitTargets.Contains(other.gameObject))
            return;

        hitTargets.Add(other.gameObject);
        PlayerMovementScript playerMovement = other.gameObject.GetComponent<PlayerMovementScript>();

        if (playerMovement == null || playerMovement.IsRolling || playerMovement.IsDead)
            return;

        playerMovement.ChangeHealth(-(int)attackDamage);
        Debug.Log("Big Bad Wolf has dealt " + attackDamage + " damage to player");
    }

    void OnEnable() => hitTargets.Clear();
}