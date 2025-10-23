using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class CraftManager : MonoBehaviour
{
    public Inventory inventory;
    public itemProgressUI progressUI;
    public Transform craftSpawnPos;
    public GameObject itemPrefab;

    public List<string> requiredItems = new List<string>();

    public void OnCraftButtonPressed()
    {
        if (!HasRequiredItems())
        {
            Debug.Log("Not enough Materials");
            return;
        }
        RemoveRequiredItems();
        Instantiate(itemPrefab, craftSpawnPos.position, Quaternion.identity);

        if (progressUI != null)
        {
            progressUI.SyncWInventory(inventory.GetItemCountsByName());
        }
    }
    private void RemoveRequiredItems()
    {
        Dictionary<string, int> requiredCounts = new Dictionary<string, int>();
        foreach (var required in requiredItems)
        {
            string key = required.Trim().ToLower();
            if (!requiredCounts.ContainsKey(key))
            {
                requiredCounts[key] = 0;
            }
            requiredCounts[key]++;
        }
        var allItems = inventory.GetInventoryDictionary();
        var itemCounts = inventory.GetItemCounts();

        foreach (var required in requiredCounts)
        {
            string requiredName = required.Key;
            int amtToRemove = required.Value;

            foreach (var x in new Dictionary<string, Item>(allItems))
            {
                string itemName = x.Value.name.Trim().ToLower();
                if (itemName == requiredName && amtToRemove > 0)
                {
                    int availableCount = itemCounts[x.Key];
                    int removeNow = Mathf.Min(amtToRemove, availableCount);

                    for (int i = 0; i < removeNow; i++)
                    {
                        inventory.RemoveItemFromInventory(x.Key);
                    }
                    amtToRemove -= removeNow;
                    if (amtToRemove <= 0)
                    {
                        break;
                    }
                }

            }


        }


    }

    private bool HasRequiredItems()
    {
        Dictionary<string, int> requiredCounts = new Dictionary<string, int>();
        foreach (var required in requiredItems)
        {
            string key = required.Trim().ToLower();
            if (!requiredCounts.ContainsKey(key))
            {
                requiredCounts[key] = 0;
            }
            requiredCounts[key]++;
        }
        var allItems = inventory.GetInventoryDictionary();
        var itemCounts = inventory.GetItemCounts();

        Dictionary<string, int> inventoryCounts = new Dictionary<string, int>();
        foreach (var x in allItems)
        {
            string key = x.Value.name.Trim().ToLower();
            int count = itemCounts[x.Key];

            if (!inventoryCounts.ContainsKey(key))
            {
                inventoryCounts[key] = 0;
            }
            inventoryCounts[key] += count;
        }

        foreach (var a in requiredCounts)
        {
            int have;
            if (inventoryCounts.ContainsKey(a.Key))
            {
                have = inventoryCounts[a.Key];
            }
            else
            {
                have = 0;
            }
            if (have < a.Value)
            {
                return false;
            }
        }
        return true;


    }






}