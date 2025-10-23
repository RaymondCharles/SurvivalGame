using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//[RequireComponent(typeof(Collider))]
public class Inventory : MonoBehaviour
{
    public InventoryUi ui;
    public Transform player;

    public AudioSource audioSource;
    public GameObject droppedItemPrefab;
    public AudioClip pickupAudio;
    public AudioClip droppedAudio;

    [SerializeField]
    SerializedDictionary<string, Item> inventory = new();
    public Dictionary<string, int> itemCounts = new();

    public void OnControllerColliderHit(ControllerColliderHit other)
    {
        if (other.gameObject.CompareTag("DroppedItem"))
        {
            var droppedItem = other.gameObject.GetComponent<DroppedItem>();
            if (droppedItem.pickedUp)
            {
                return;
            }
            droppedItem.pickedUp = true;
            AddItem(droppedItem.item);
            Destroy(other.gameObject);
            audioSource.PlayOneShot(pickupAudio);
        }
    }

    void AddItem(Item item)
    {
        string existingId = null;
        foreach (var x in inventory)
        {
            if (x.Value.name == item.name)
            {
                existingId = x.Key;
                break;
            }
        }
        if (existingId != null)
        {
            itemCounts[existingId]++;
            ui.UpdateUIItemCount(existingId, itemCounts[existingId]);

        }
        else
        {
            var inventoryId = Guid.NewGuid().ToString();
            inventory.Add(inventoryId, item);
            itemCounts[inventoryId] = 1;
            ui.AddUIItem(inventoryId, item);

        }

        var ProgressUi = FindFirstObjectByType<itemProgressUI>();
        if (ProgressUi != null)
        {
            ProgressUi.AddItem(item.name, 1);
        }
    }

    public void DropItem(string inventoryId)
    {
        if (!inventory.TryGetValue(inventoryId, out var item))
        {
            return;
        }
        Vector3 dropPos = player.position + player.forward * 1f + Vector3.up * 0.5f;
        Instantiate(item.prefab, dropPos, Quaternion.identity);

        if (itemCounts[inventoryId] > 1)
        {
            itemCounts[inventoryId]--;
            ui.UpdateUIItemCount(inventoryId, itemCounts[inventoryId]);
        }
        else
        {
            inventory.Remove(inventoryId);
            itemCounts.Remove(inventoryId);
            ui.RemoveUIItem(inventoryId);

        }

        var ProgressUI = FindFirstObjectByType<itemProgressUI>();
        if (ProgressUI != null)
        {
            ProgressUI.RemoveItem(item.name, 1);
        }

        if (audioSource && droppedAudio)
        {
            audioSource.PlayOneShot(droppedAudio);
        }


    }


    public Dictionary<string, int> GetItemCountsByName()
    {
        Dictionary<string, int> counts = new();
        foreach (var x in inventory)
        {
            string name = x.Value.name.ToLower();
            int count = itemCounts[x.Key];
            if (!counts.ContainsKey(name))
            {
                counts[name] = 0;

            }
            counts[name] += count;

        }
        return counts;

    }

    public void RemoveItemFromInventory(string inventoryId)
    {
        if (!inventory.ContainsKey(inventoryId))
        {
            return;
        }
        if (itemCounts[inventoryId] > 1)
        {
            itemCounts[inventoryId]--;
            ui.UpdateUIItemCount(inventoryId, itemCounts[inventoryId]);
        }
        else
        {
            inventory.Remove(inventoryId);
            itemCounts.Remove(inventoryId);
            ui.RemoveUIItem(inventoryId);
        }
    }

    public bool TryGetItem(string inventoryId, out Item item)
    {
        return inventory.TryGetValue(inventoryId, out item);
    }

    public Dictionary<string, Item> GetInventoryDictionary() => inventory;
    public Dictionary<string, int> GetItemCounts() => itemCounts;


}