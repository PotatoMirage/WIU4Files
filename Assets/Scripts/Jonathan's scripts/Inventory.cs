using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Inventory", menuName = "Scriptable Objects/Inventory")]
public class Inventory : ScriptableObject
{
    public int maxItems = 9;
    public List<ItemInstance> items = new List<ItemInstance>();
    public ItemInstance GetItem(int index) { return items[index]; }

    public int selectedIndex = 0;

    public UnityEvent onInventoryChanged;

    public void OnEnable()
    {
        selectedIndex = 0;
    }

    public ItemInstance GetSelectedItem()
    {
        if (items.Count > 0)
        {
            return items[selectedIndex];
        }
        return null;
    }
    public void RemoveItem(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            items.RemoveAt(index);
            if (selectedIndex >= items.Count && items.Count > 0)
            {
                selectedIndex = items.Count - 1;
            }
            onInventoryChanged?.Invoke();
        }
    }

    public bool AddItem(ItemInstance item)
    {
        if (items.Count >= maxItems)
        {
            return false;
        }
        items.Add(item);
        onInventoryChanged?.Invoke();
        return true;
    }

    public void DisplayItems()
    {
        foreach (ItemInstance item in items)
        {
            Debug.Log($"Item Name: {item.itemData.itemName}");
        }

    }



    public void SelectNextItem()
    {
        if (items.Count == 0) return;
        selectedIndex = (selectedIndex + 1) % items.Count;
        onInventoryChanged?.Invoke();
    }

    public void SelectPreviousItem()
    {
        if (items.Count == 0) return;
        selectedIndex--;
        if (selectedIndex < 0)
        {
            selectedIndex = items.Count - 1;
        }
        onInventoryChanged?.Invoke();
    }
}
