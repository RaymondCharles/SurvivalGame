using System;
using UnityEngine;
using UnityEngine.Rendering; // Keep if SerializedDictionary needs it

public class InventoryUi : MonoBehaviour
{
    [Header("References")]
    public GameObject uiItemPrefab;
    public Inventory inventory; // Reference to the main Inventory script
    public Transform uiInventoryParent; // The grid/layout group for UI items
    public InvDropPanel invDropPanel;   // Your panel with the "Drop" button
    public InvPlanelUse invUsePanel;    // Your panel with the "Use" button

    [SerializeField]
    SerializedDictionary<string, GameObject> inventoryUI = new(); // Tracks the UI GameObjects

    private string currentSelectedItemId;

    public void AddUIItem(string inventoryId, Item item)
    {
        if (uiItemPrefab == null || uiInventoryParent == null)
        {
            Debug.LogError("UI Prefab or Parent not set in InventoryUi!");
            return;
        }

        var itemUIInstance = Instantiate(uiItemPrefab, uiInventoryParent);
        var itemUIScript = itemUIInstance.GetComponent<ItemUi>();

        if (itemUIScript != null)
        {
            inventoryUI.Add(inventoryId, itemUIInstance);
            itemUIScript.Initialize(inventoryId, item, OnItemSelected);
        }
        else
        {
            Debug.LogError("uiItemPrefab is missing the ItemUi script!");
            Destroy(itemUIInstance);
        }
    }

    private void OnItemSelected(string inventoryId)
    {
        if (inventory == null || !inventory.TryGetItem(inventoryId, out Item selectedItem))
        {
            Debug.LogError($"Could not find item with ID {inventoryId} or Inventory reference is missing.");
            HideActionPanels(); // Hide panels if item is invalid
            return;
        }

        currentSelectedItemId = inventoryId;

        if (invDropPanel != null)
        {
            if (selectedItem.canBeDropped) invDropPanel.Show(OnDropConfirmed);
            else invDropPanel.gameObject.SetActive(false);
        }

        if (invUsePanel != null)
        {
            if (selectedItem.isConsumable) invUsePanel.Show(OnUseConfirmed);
            else invUsePanel.gameObject.SetActive(false);
        }
    }

    private void OnDropConfirmed()
    {
        if (inventory != null) inventory.DropItem(currentSelectedItemId);
        HideActionPanels();
    }

    private void OnUseConfirmed()
    {
        if (inventory != null) inventory.UseItem(currentSelectedItemId);
        HideActionPanels();
    }

    public void RemoveUIItem(string inventoryId)
    {
        if (inventoryUI.TryGetValue(inventoryId, out var itemUIGameObject))
        {
            inventoryUI.Remove(inventoryId);
            Destroy(itemUIGameObject);
        }
        if (inventoryId == currentSelectedItemId)
        {
            HideActionPanels();
        }
    }

    private void HideActionPanels()
    {
        if (invDropPanel != null) invDropPanel.gameObject.SetActive(false);
        if (invUsePanel != null) invUsePanel.gameObject.SetActive(false);
        currentSelectedItemId = null;
    }
}