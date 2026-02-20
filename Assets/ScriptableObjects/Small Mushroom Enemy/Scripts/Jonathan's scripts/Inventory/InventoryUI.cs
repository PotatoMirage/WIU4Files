using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Inventory playerInventory;

    public Image itemSprite;
    public TextMeshProUGUI itemName;
    public GameObject inventorySlot;

    void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.onInventoryChanged.AddListener(UpdateUI);
        }
    }

    void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.onInventoryChanged.RemoveListener(UpdateUI);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        ItemInstance selectedItem = playerInventory.GetSelectedItem();

        if (selectedItem != null && selectedItem.itemData != null)
        {
            inventorySlot.SetActive(true);
            itemSprite.sprite = selectedItem.itemData.itemImage;
            itemName.text = selectedItem.itemData.itemName;
        }
        else
        {
            inventorySlot.SetActive(false);
        }
    }
}