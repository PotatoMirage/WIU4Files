using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HotbarInventory : MonoBehaviour
{
    public ItemData[] slots = new ItemData[5];
    public int selectedIndex = 0;
    public HotbarUI uiManager;
    public float dropForce = 5f;
    public float dropUpForce = 2f;
    public Transform dropPoint;

    private PlayerInput playerInput;
    private InputAction interactAction;
    private List<WorldItem> nearbyItems = new List<WorldItem>();

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];
    }

    private void OnEnable()
    {
        interactAction.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.performed -= OnInteract;
    }

    private void Start()
    {
        LoadInventory();

        if (uiManager != null)
        {
            uiManager.UpdateSelection(selectedIndex);

            for (int i = 0; i < 5; i++)
            {
                uiManager.UpdateSlot(i, slots[i]);
            }
        }
    }
    private void Update()
    {
        HandleHotbarInput();
        HandleDropInput();
        HandleScrollInput();
    }

    private void HandleHotbarInput()
    {
        if (playerInput == null) return;

        if (playerInput.actions["Slot1"].WasPressedThisFrame()) SelectSlot(0);
        if (playerInput.actions["Slot2"].WasPressedThisFrame()) SelectSlot(1);
        if (playerInput.actions["Slot3"].WasPressedThisFrame()) SelectSlot(2);
        if (playerInput.actions["Slot4"].WasPressedThisFrame()) SelectSlot(3);
        if (playerInput.actions["Slot5"].WasPressedThisFrame()) SelectSlot(4);
    }

    private void HandleDropInput()
    {
        if (playerInput == null) return;

        if (playerInput.actions["Drop"].WasPressedThisFrame())
        {
            DropCurrentItem();
        }
    }
    private void SelectSlot(int index)
    {
        selectedIndex = index;
        uiManager.UpdateSelection(selectedIndex);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        CleanNearbyItems();

        if (nearbyItems.Count > 0)
        {
            WorldItem itemToPickup = nearbyItems[0];
            PickupItem(itemToPickup);
        }
        else
        {
            UseCurrentItem();
        }
    }

    private void UseCurrentItem()
    {
        ItemData currentItem = slots[selectedIndex];

        if (currentItem != null && currentItem.useEffect != null)
        {
            currentItem.useEffect.Execute(this.gameObject);

            if (currentItem.isConsumable)
            {
                slots[selectedIndex] = null;
                uiManager.UpdateSlot(selectedIndex, null);
            }
        }
    }

    private void CleanNearbyItems()
    {
        for (int i = nearbyItems.Count - 1; i >= 0; i--)
        {
            if (nearbyItems[i] == null)
            {
                nearbyItems.RemoveAt(i);
            }
        }
    }

    private void PickupItem(WorldItem worldItem)
    {
        bool itemAdded = false;

        for (int i = 0; i < 5; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = worldItem.itemData;
                uiManager.UpdateSlot(i, slots[i]);
                itemAdded = true;
                break;
            }
        }

        if (!itemAdded)
        {
            ItemData currentItem = slots[selectedIndex];
            if (currentItem != null)
            {
                DropItemPrefab(currentItem);
            }

            slots[selectedIndex] = worldItem.itemData;
            uiManager.UpdateSlot(selectedIndex, slots[selectedIndex]);
            itemAdded = true;
        }

        if (itemAdded)
        {
            nearbyItems.Remove(worldItem);
            Destroy(worldItem.gameObject);
        }
    }
    private void DropCurrentItem()
    {
        if (slots[selectedIndex] != null)
        {
            DropItemPrefab(slots[selectedIndex]);
            slots[selectedIndex] = null;
            uiManager.UpdateSlot(selectedIndex, null);
        }
    }

    private void DropItemPrefab(ItemData itemData)
    {
        GameObject droppedObject = Instantiate(itemData.dropPrefab, dropPoint.position, dropPoint.rotation);
        
        if (droppedObject.TryGetComponent<Rigidbody>(out Rigidbody rigidBody))
        {
            Vector3 force = dropPoint.forward * dropForce + dropPoint.up * dropUpForce;
            rigidBody.AddForce(force, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Collectables"))
        {
            WorldItem worldItem = other.GetComponent<WorldItem>();
            if (worldItem != null && !nearbyItems.Contains(worldItem))
            {
                nearbyItems.Add(worldItem);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Collectables"))
        {
            WorldItem worldItem = other.GetComponent<WorldItem>();
            if (worldItem != null && nearbyItems.Contains(worldItem))
            {
                nearbyItems.Remove(worldItem);
            }
        }
    }
    public void SaveInventory()
    {
        if (PlayerSave.Instance == null)
        {
            return;
        }

        string[] itemNames = new string[5];

        for (int i = 0; i < 5; i++)
        {
            if (slots[i] != null)
            {
                itemNames[i] = slots[i].itemName;
            }
            else
            {
                itemNames[i] = "";
            }
        }

        string inventoryData = string.Join(",", itemNames);
        PlayerSave.Instance.SaveInventory(inventoryData);
    }
    public void LoadInventory()
    {
        if (PlayerSave.Instance == null)
        {
            return;
        }

        string savedData = PlayerSave.Instance.GetInventory();

        if (string.IsNullOrEmpty(savedData))
        {
            return;
        }

        string[] itemNames = savedData.Split(',');

        for (int i = 0; i < 5; i++)
        {
            if (i < itemNames.Length && !string.IsNullOrEmpty(itemNames[i]))
            {
                ItemData loadedItem = Resources.Load<ItemData>("Items/" + itemNames[i]);
                slots[i] = loadedItem;
            }
            else
            {
                slots[i] = null;
            }
        }
    }
    private void HandleScrollInput()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
        {
            return;
        }

        float scrollValue = mouse.scroll.ReadValue().y;

        if (scrollValue > 0f)
        {
            int newIndex = selectedIndex - 1;

            if (newIndex < 0)
            {
                newIndex = 8;
            }

            SelectSlot(newIndex);
        }
        else if (scrollValue < 0f)
        {
            int newIndex = selectedIndex + 1;

            if (newIndex > 8)
            {
                newIndex = 0;
            }

            SelectSlot(newIndex);
        }
    }
}