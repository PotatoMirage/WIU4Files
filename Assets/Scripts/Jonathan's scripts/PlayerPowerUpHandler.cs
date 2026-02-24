using UnityEngine;
using System.Collections;

public class PlayerPowerUpHandler : MonoBehaviour
{
    public void ApplyBuff(string buffType, int amount, float duration, bool isPermanent, Sprite icon)
    {
        if (StatusEffectUIManager.Instance != null)
        {
            StatusEffectUIManager.Instance.AddEffect(buffType, icon, duration, isPermanent);
        }

        if (isPermanent)
        {
            ApplyPermanentBuff(buffType, amount);
        }
        else
        {
            StartCoroutine(TemporaryBuffRoutine(buffType, amount, duration));
        }
    }

    private void ApplyPermanentBuff(string buffType, int amount)
    {
        if (buffType == "Health")
        {
            int currentMax = PlayerSave.Instance.GetMaxHealth();
            PlayerSave.Instance.SaveMaxHealth(currentMax + amount);
        }
        else if (buffType == "Damage")
        {
            int currentDamage = PlayerSave.Instance.GetMaxDamage();
            PlayerSave.Instance.SaveMaxDamage(currentDamage + amount);
        }
    }

    private IEnumerator TemporaryBuffRoutine(string buffType, int amount, float duration)
    {
        ApplyTemporaryStat(buffType, amount);

        yield return new WaitForSeconds(duration);

        RemoveTemporaryStat(buffType, amount);

        if (StatusEffectUIManager.Instance != null)
        {
            StatusEffectUIManager.Instance.RemoveEffect(buffType);
        }
    }

    private void ApplyTemporaryStat(string buffType, int amount)
    {
        if (buffType == "Speed")
        {

        }
        else if (buffType == "Damage")
        {

        }
    }

    private void RemoveTemporaryStat(string buffType, int amount)
    {
        if (buffType == "Speed")
        {

        }
        else if (buffType == "Damage")
        {

        }
    }
}