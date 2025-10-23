using UnityEngine;
using System.Collections; // Needed for IEnumerator

public class DroppedItem : MonoBehaviour
{
    public Item item; // Assign the ScriptableObject (Food or Potion) here in the prefab's Inspector
    [HideInInspector] public bool pickedUp = false;
    public float enabledPickupDelay = 0.5f; // Short delay before pickup is allowed

    // Called by the Inventory script when an item is dropped
    public void Initialize(Item itemToDrop)
    {
        item = itemToDrop;
        // Optional: Update visual appearance based on item
        // GetComponentInChildren<Renderer>().material = item.pickupMaterial; // Example

        // Start delay before pickup is possible
        GetComponent<Collider>().enabled = false; // Disable collider initially
        StartCoroutine(EnablePickup(enabledPickupDelay));
    }

    IEnumerator EnablePickup(float delay)
    {
        yield return new WaitForSeconds(delay);
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true; // Enable collider after delay
    }
}