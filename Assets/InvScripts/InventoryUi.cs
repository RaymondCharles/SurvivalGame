using System;
using UnityEngine;
using UnityEngine.Rendering;

public class InventoryUi : MonoBehaviour
{
    [Header("References")]
    public GameObject uiItemPrefab;
    public Inventory inventory;
    public Transform uiInventoryParent;
    public InvDropPanel invDropPanel;
    public InvPlanelUse invUsePanel;
    public PlayerStatss playerStats;

    [SerializeField]
    SerializedDictionary<string, GameObject> inventoryUI = new();

    // Store the current selected item's id and data
    private string currentItemId;
    private Item currentItem;

    public void AddUIItem(string inventoryId, Item item)
    {
        var itemUI = Instantiate(uiItemPrefab).GetComponent<ItemUi>();
        itemUI.transform.SetParent(uiInventoryParent, false);
        inventoryUI.Add(inventoryId, itemUI.gameObject);

        // Pass the method OnItemSelected as the callback instead of a lambda
        itemUI.Initialize(inventoryId, item, OnItemSelected);
    }

    // Called when an item UI is clicked/selected
    private void OnItemSelected(string inventoryId)
    {
        if (!inventory.TryGetItem(inventoryId, out var item))
            return;

        currentItemId = inventoryId;
        currentItem = item;

        invDropPanel.Show(OnDropConfirmed);

        if (item.isConsumable)
        {
            invUsePanel.Show(OnUseConfirmed);
        }
        else
        {
            invUsePanel.gameObject.SetActive(false);
        }
    }



    // Called when user confirms dropping the item
    private void OnDropConfirmed()
    {
        inventory.DropItem(currentItemId);
    }

    // Called when user confirms using the item
    private void OnUseConfirmed()
    {
        playerStats.IncreaseStats(currentItem.hpRestoreAmount, currentItem.hungerRestoreAmount);
        inventory.RemoveItemFromInventory(currentItemId);
    }

    public void RemoveUIItem(string inventoryId)
    {
        if (inventoryUI.TryGetValue(inventoryId, out var itemUI))
        {
            inventoryUI.Remove(inventoryId);
            Destroy(itemUI);
        }
    }
}
