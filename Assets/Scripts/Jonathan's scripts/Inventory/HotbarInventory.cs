using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HotbarInventory : MonoBehaviour
{
    public ItemData[] slots = new ItemData[9];
    public int selectedIndex = 0;
    public HotbarUI uiManager;
    public float dropForce = 5f;
    public float dropUpForce = 2f;
    public Transform dropPoint;

    private InputSystem_Actions inputActions;
    private List<WorldItem> nearbyItems = new();

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.performed -= OnInteract;
        inputActions.Player.Disable();
    }

    private void Start()
    {
        uiManager.UpdateSelection(selectedIndex);
        for (int i = 0; i < 9; i++)
        {
            uiManager.UpdateSlot(i, slots[i]);
        }
    }
    private void Update()
    {
        HandleHotbarInput();
        HandleDropInput();
    }

    private void HandleHotbarInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame) SelectSlot(0);
        if (keyboard.digit2Key.wasPressedThisFrame) SelectSlot(1);
        if (keyboard.digit3Key.wasPressedThisFrame) SelectSlot(2);
        if (keyboard.digit4Key.wasPressedThisFrame) SelectSlot(3);
        if (keyboard.digit5Key.wasPressedThisFrame) SelectSlot(4);
        if (keyboard.digit6Key.wasPressedThisFrame) SelectSlot(5);
        if (keyboard.digit7Key.wasPressedThisFrame) SelectSlot(6);
        if (keyboard.digit8Key.wasPressedThisFrame) SelectSlot(7);
        if (keyboard.digit9Key.wasPressedThisFrame) SelectSlot(8);
    }

    private void HandleDropInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.qKey.wasPressedThisFrame)
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

        for (int i = 0; i < 9; i++)
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
}