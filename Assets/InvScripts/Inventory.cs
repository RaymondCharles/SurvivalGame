using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//[RequireComponent(typeof(Colldier))]
public class Inventory : MonoBehaviour
{
    public InventoryUi ui;
    public AudioSource audioSource;
    public GameObject droppedItemPrefab;
    public AudioClip pickupAudio;
    public AudioClip droppedAudio;
    [SerializeField]
    SerializedDictionary<string, Item> inventory = new();

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DroppedItem"))
        {
            var droppedItem = other.GetComponent<DroppedItem>();
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
        var inventoryId = Guid.NewGuid().ToString();
        inventory.Add(inventoryId, item);
        ui.AddUIItem(inventoryId, item);
    }
    public void DropItem(string inventoryId)
    {


        var droppedItem = Instantiate(droppedItemPrefab, transform.position, Quaternion.identity).GetComponent<DroppedItem>();
        Debug.Log("Spawned prefab: " + droppedItem.name);

        var item = inventory.GetValueOrDefault(inventoryId);
        droppedItem.Initialize(item);
        inventory.Remove(inventoryId);
        ui.RemoveUIItem(inventoryId);
        audioSource.PlayOneShot(droppedAudio);
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