using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class InventoryUi : MonoBehaviour
{

    public GameObject uiItemPrefab;
    public Inventory inventory;
    public Transform uiInventoryParent;
    public InvDropPanel invDropPanel;
    public InvPlanelUse invUsePanel;
    public VitalsBarBinder playerVitals;

    public ItemUi selectedItemUi;
    public string currentItemId;
    public Item currentItem;

    [SerializeField]
    SerializedDictionary<string, GameObject> inventoryUI = new();

    private void Start()
    {
        // Make sure vitals are assigned
        if (!playerVitals)
        {
            playerVitals = FindFirstObjectByType<VitalsBarBinder>();
        }
    }

    public void AddUIItem(string inventoryId, Item item)
    {
        var itemUI = Instantiate(uiItemPrefab, uiInventoryParent).GetComponent<ItemUi>();
        inventoryUI.Add(inventoryId, itemUI.gameObject);
        itemUI.Initialize(inventoryId, item, OnItemSelected);
    }

    private void OnItemSelected(string inventoryId)
    {
        if (!inventory.TryGetItem(inventoryId, out var item))
            return;

        if (selectedItemUi != null)
            selectedItemUi.SetSelected(false);

        selectedItemUi = inventoryUI[inventoryId].GetComponent<ItemUi>();
        selectedItemUi.SetSelected(true);

        currentItemId = inventoryId;
        currentItem = item;

        invDropPanel.Show(OnDropConfirmed);

        if (item.isConsumable)
            invUsePanel.Show(item, OnUseConfirmed);
        else
            invUsePanel.gameObject.SetActive(false);
    }

    public void UpdateUIItemCount(string inventoryId, int count)
    {
        if (inventoryUI.TryGetValue(inventoryId, out var itemUiObj))
        {
            var itemUi = itemUiObj.GetComponent<ItemUi>();
            itemUi.SetCount(count);
        }
    }

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

    private void OnUseConfirmed()
    {
        if (playerVitals != null && currentItem != null && currentItem.isConsumable)
        {
            if (currentItem.hpRestoreAmount > 0)
                playerVitals.AddHealth(currentItem.hpRestoreAmount);

            if (currentItem.hungerRestoreAmount > 0)
                playerVitals.AddHunger(currentItem.hungerRestoreAmount);
        }

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
