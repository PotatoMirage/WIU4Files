using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerPowerUpHandler : MonoBehaviour
{
    private PlayerMovementScript playerMovement;
    private PlayerAttackScript playerAttack;
    private Dictionary<string, Coroutine> activeTemporaryBuffs = new Dictionary<string, Coroutine>();
    private HashSet<string> permanentBuffs = new HashSet<string>();

    public void Awake()
    {
        playerMovement = GetComponent<PlayerMovementScript>();
        playerAttack = GetComponent<PlayerAttackScript>();
    }

    public void ApplyBuff(string buffType, int amount, float duration, bool isPermanent, Sprite icon)
    {
        if (StatusEffectUIManager.Instance != null)
        {
            StatusEffectUIManager.Instance.AddEffect(buffType, icon, duration, isPermanent);
        }

        if (isPermanent)
        {
            ApplyStatModifier(buffType, amount);
            permanentBuffs.Add(buffType);

            if (activeTemporaryBuffs.ContainsKey(buffType))
            {
                StopCoroutine(activeTemporaryBuffs[buffType]);
                activeTemporaryBuffs.Remove(buffType);
            }
        }
        else
        {
            if (permanentBuffs.Contains(buffType))
            {
                return;
            }

            if (activeTemporaryBuffs.ContainsKey(buffType))
            {
                StopCoroutine(activeTemporaryBuffs[buffType]);
            }
            else
            {
                ApplyStatModifier(buffType, amount);
            }

            activeTemporaryBuffs[buffType] = StartCoroutine(TemporaryBuffRoutine(buffType, amount, duration));
        }
    }

    private void ApplyStatModifier(string buffType, int amount)
    {
        if (buffType == "MaxHealth" || buffType == "Health")
        {
            if (playerMovement != null)
            {
                playerMovement.maxHealth += amount;
                playerMovement.ChangeHealth(amount);
            }

            if (PlayerSave.Instance != null)
            {
                int currentMax = PlayerSave.Instance.GetMaxHealth();
                PlayerSave.Instance.SaveMaxHealth(currentMax + amount);
            }
        }
        else if (buffType == "MaxDamage" || buffType == "Damage")
        {
            if (playerAttack != null)
            {
                playerAttack.rangedDamage += amount;

                if (playerAttack.attackCollider != null)
                {
                    AttackColliderScript attackCollider = playerAttack.attackCollider.GetComponent<AttackColliderScript>();
                    if (attackCollider != null)
                    {
                        attackCollider.meleeDamage += amount;
                    }
                }
            }

            if (PlayerSave.Instance != null)
            {
                int currentDamage = PlayerSave.Instance.GetMaxDamage();
                PlayerSave.Instance.SaveMaxDamage(currentDamage + amount);
            }
        }
        else if (buffType == "Speed")
        {
            if (playerMovement != null)
            {
                playerMovement.walkSpeed += amount;
            }
        }
    }

    private void RemoveStatModifier(string buffType, int amount)
    {
        if (buffType == "MaxHealth" || buffType == "Health")
        {
            if (playerMovement != null)
            {
                playerMovement.maxHealth -= amount;
                if (playerMovement.health > playerMovement.maxHealth)
                {
                    playerMovement.health = playerMovement.maxHealth;
                }
            }

            if (PlayerSave.Instance != null)
            {
                int currentMax = PlayerSave.Instance.GetMaxHealth();
                PlayerSave.Instance.SaveMaxHealth(currentMax - amount);
            }
        }
        else if (buffType == "MaxDamage" || buffType == "Damage")
        {
            if (playerAttack != null)
            {
                playerAttack.rangedDamage -= amount;

                if (playerAttack.attackCollider != null)
                {
                    AttackColliderScript attackCollider = playerAttack.attackCollider.GetComponent<AttackColliderScript>();
                    if (attackCollider != null)
                    {
                        attackCollider.meleeDamage -= amount;
                    }
                }
            }

            if (PlayerSave.Instance != null)
            {
                int currentDamage = PlayerSave.Instance.GetMaxDamage();
                PlayerSave.Instance.SaveMaxDamage(currentDamage - amount);
            }
        }
        else if (buffType == "Speed")
        {
            if (playerMovement != null)
            {
                playerMovement.walkSpeed -= amount;
            }
        }
    }

    private IEnumerator TemporaryBuffRoutine(string buffType, int amount, float duration)
    {
        yield return new WaitForSeconds(duration);

        RemoveStatModifier(buffType, amount);
        activeTemporaryBuffs.Remove(buffType);

        if (StatusEffectUIManager.Instance != null)
        {
            StatusEffectUIManager.Instance.RemoveEffect(buffType);
        }
    }
}
