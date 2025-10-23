using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public bool autoStart;
    public float enabledPickupDelay = 3.0f;
    public Item item;
    public bool pickedUp = false;

    // Start is called before the first frame update
    void Start()
    {
        if (autoStart && item != null)
        {
            Initialize(item);

        }
    }

    // Update is called once per frame
    //public void Initialize(Item item)
    //{
    //    this.item = item;
    //    var droppedItem = Instantiate(item.prefab, transform);
    //    Debug.Log("Spawned prefab: " + droppedItem.name);

    //    droppedItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    //    StartCoroutine(EnablePickup(enabledPickupDelay));
    //}

    public void Initialize(Item item)
    {
        this.item = item;

        if (item.prefab == null)
        {
            Debug.LogError($"Item {item.name} has no prefab assigned!");
            return;
        }

        var droppedItem = Instantiate(item.prefab, transform);

        //Debug.Log($"Spawned prefab: {droppedItem.name} at {droppedItem.transform.position}");

        droppedItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        StartCoroutine(EnablePickup(enabledPickupDelay));
    }

    IEnumerator EnablePickup(float delay)
    {
        yield return new WaitForSeconds(delay);
        GetComponent<Collider>().enabled = true;
    }



}