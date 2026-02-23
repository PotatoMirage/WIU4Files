using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject dropPrefab;
    public bool isConsumable;
    public ItemEffect useEffect;
}