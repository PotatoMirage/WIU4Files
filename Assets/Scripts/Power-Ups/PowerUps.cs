using UnityEngine;

[CreateAssetMenu(fileName = "PowerUps", menuName = "Inventory/Effects/PowerUp")]
public class PowerUps : ItemEffect
{
    public float duration;
    public int effectAmount;
    public string buffType;
    public bool isPermanent;
    public Sprite buffIcon;

    public override void Execute(GameObject user)
    {
        PlayerPowerUpHandler handler = user.GetComponent<PlayerPowerUpHandler>();

        if (handler != null)
        {
            handler.ApplyBuff(buffType, effectAmount, duration, isPermanent, buffIcon);
        }
    }
}