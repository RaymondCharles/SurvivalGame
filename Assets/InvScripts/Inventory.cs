using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; // Keep if SerializedDictionary needs it

// If PlayerVitals is in a different namespace, add its 'using' statement here

public class Inventory : MonoBehaviour
{
    [Header("References")]
    public InventoryUi ui;
    public AudioSource audioSource;
    public GameObject droppedItemPrefab; // Prefab used when dropping ANY item
    public PlayerVitals playerVitals; // *** ASSIGN THIS IN THE INSPECTOR ***

    [Header("Audio")]
    public AudioClip pickupAudio;
    public AudioClip droppedAudio;
    // Optional: public AudioClip useAudio;
    // Optional: public AudioClip cannotUseAudio;

    [Header("Inventory Data")]
    [SerializeField]
    SerializedDictionary<string, Item> inventory = new(); // Holds the actual item data

    // --- Item Management ---

    /// <summary>
    /// Adds a picked-up item to the inventory.
    /// </summary>
    void AddItem(Item item)
    {
        var inventoryId = Guid.NewGuid().ToString(); // Generate a unique ID for this inventory slot
        inventory.Add(inventoryId, item);
        if (ui != null) ui.AddUIItem(inventoryId, item); // Update the UI
        if (audioSource != null && pickupAudio != null) audioSource.PlayOneShot(pickupAudio);
        // Debug.Log($"Added {item.id} to inventory."); // Uncomment if needed
    }

    /// <summary>
    /// Spawns a dropped item prefab in the world and removes the item from inventory.
    /// </summary>
    public void DropItem(string inventoryId)
    {
        if (inventory.TryGetValue(inventoryId, out Item itemToDrop))
        {
            if (itemToDrop.canBeDropped && droppedItemPrefab != null)
            {
                Vector3 dropPosition = transform.position + transform.forward * 1.0f;
                var droppedItemInstance = Instantiate(droppedItemPrefab, dropPosition, Quaternion.identity);
                var droppedItemScript = droppedItemInstance.GetComponent<DroppedItem>();

                if (droppedItemScript != null)
                {
                    droppedItemScript.Initialize(itemToDrop);
                    RemoveItemFromInventoryInternal(inventoryId);
                    if (audioSource != null && droppedAudio != null) audioSource.PlayOneShot(droppedAudio);
                    // Debug.Log($"Dropped {itemToDrop.id}."); // Uncomment if needed
                }
                else
                {
                    Debug.LogError("DroppedItemPrefab is missing the DroppedItem script!");
                    Destroy(droppedItemInstance);
                }
            }
            else
            {
                Debug.Log($"{itemToDrop.id} cannot be dropped or droppedItemPrefab is missing.");
            }
        }
    }

    /// <summary>
    /// Attempts to use an item based on its inventory ID.
    /// </summary>
    public void UseItem(string inventoryId)
    {
        if (playerVitals == null)
        {
            Debug.LogError("PlayerVitals reference not set on Inventory script!");
            return;
        }

        if (inventory.TryGetValue(inventoryId, out Item itemToUse))
        {
            if (itemToUse.isConsumable)
            {
                bool successfullyUsed = itemToUse.Use(playerVitals); // Call the Item's Use method

                if (successfullyUsed)
                {
                    RemoveItemFromInventoryInternal(inventoryId); // Remove if used
                    // Play use sound if needed
                }
                else
                {
                    Debug.Log($"{itemToUse.id} could not be used right now.");
                    // Play failure sound if needed
                }
            }
            else
            {
                Debug.Log($"{itemToUse.id} is not consumable.");
                // Play failure sound if needed
            }
        }
        else
        {
            Debug.LogError($"Item with ID {inventoryId} not found in inventory!");
        }
    }

    /// <summary>
    /// Internal helper to remove item data and update UI.
    /// </summary>
    private void RemoveItemFromInventoryInternal(string inventoryId)
    {
        if (inventory.ContainsKey(inventoryId))
        {
            inventory.Remove(inventoryId);
            if (ui != null) ui.RemoveUIItem(inventoryId); // Update UI
            // Debug.Log("Item removed from inventory."); // Uncomment if needed
        }
    }

    /// <summary>
    /// Public method if something external needs to directly remove an item.
    /// </summary>
    public void RemoveItemFromInventory(string inventoryId)
    {
        RemoveItemFromInventoryInternal(inventoryId);
    }

    /// <summary>
    /// Safely tries to get an Item object from the inventory using its unique ID.
    /// </summary>
    public bool TryGetItem(string inventoryId, out Item item)
    {
        return inventory.TryGetValue(inventoryId, out item);
    }

    // --- Pickup Logic ---
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DroppedItem")) // Make sure your dropped item prefabs have this tag!
        {
            var droppedItem = other.GetComponent<DroppedItem>();
            if (droppedItem != null && !droppedItem.pickedUp && droppedItem.item != null)
            {
                droppedItem.pickedUp = true;
                AddItem(droppedItem.item);
                Destroy(other.gameObject);
            }
        }
    }
}