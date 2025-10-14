using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InventoryUi : MonoBehaviour
{

    public GameObject uiItemPrefab;
    public Inventory inventory;
    public Transform uiInventoryParent;
    [SerializeField]
    SerializedDictionary<string, GameObject> inventoryUI = new();

    public void AddUIItem(string inventoryId, Item item)
    {
        var itemUI = Instantiate(uiItemPrefab).GetComponent<ItemUi>();


        itemUI.transform.SetParent(uiInventoryParent);
        inventoryUI.Add(inventoryId, itemUI.gameObject);
        itemUI.Initialize(inventoryId, item, inventory.DropItem);


    }

    public void RemoveUIItem(string inventoryId)
    {
        var itemUI = inventoryUI.GetValueOrDefault(inventoryId);
        inventoryUI.Remove(inventoryId);
        Destroy(itemUI);

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
