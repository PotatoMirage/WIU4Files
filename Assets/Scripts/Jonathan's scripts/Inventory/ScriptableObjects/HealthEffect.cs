using UnityEngine;

[CreateAssetMenu(fileName = "NewHealthEffect", menuName = "Inventory/Effects/Health")]
public class HealthEffect : ItemEffect
{
    public int healAmount = 20;

    public override void Execute(GameObject user)
    {
        Debug.Log("Used hp item");
    }
}