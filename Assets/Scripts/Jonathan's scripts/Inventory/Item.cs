using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemInstance item;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = item.itemData.itemImage;
    }

    public ItemInstance TakeItem()
    {
        Destroy(gameObject);
        return item;
    }
}
