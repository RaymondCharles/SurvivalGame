using System;
using Unity.VisualScripting;
using UnityEditor;
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

    public ItemUi selectedItemUi;
    public string currentItemId;
    public Item currentItem;


    [SerializeField]
    SerializedDictionary<string, GameObject> inventoryUI = new();

    public void AddUIItem(string inventoryId, Item item)
    {
        var itemUI = Instantiate(uiItemPrefab, uiInventoryParent).GetComponent<ItemUi>();
        inventoryUI.Add(inventoryId, itemUI.gameObject);
        itemUI.Initialize(inventoryId, item, OnItemSelected);
    }

    // Called when an item UI is clicked/selected
    private void OnItemSelected(string inventoryId)
    {
        if (!inventory.TryGetItem(inventoryId, out var item))
            return;

        if (selectedItemUi != null)
        {
            selectedItemUi.SetSelected(false);
        }

        selectedItemUi = inventoryUI[inventoryId].GetComponent<ItemUi>();
        selectedItemUi.SetSelected(true);

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

    public void UpdateUIItemCount(string inventoryId, int count)
    {
        if (inventoryUI.TryGetValue(inventoryId, out var itemUiobj))
        {
            var itemUi = itemUiobj.GetComponent<ItemUi>();
            itemUi.SetCount(count);
        }


    }



    // Called when user confirms dropping the item
    private void OnDropConfirmed()
    {
        inventory.DropItem(currentItemId);

        if (selectedItemUi != null)
        {
            selectedItemUi.SetSelected(false);
            selectedItemUi = null;
        }
        currentItem = null;
        currentItemId = null;
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