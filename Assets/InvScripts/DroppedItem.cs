using UnityEngine;
using System.Collections; // Needed for IEnumerator

public class DroppedItem : MonoBehaviour
{
    public Item item; // Assign the ScriptableObject (Food or Potion) in the prefab's Inspector
    [HideInInspector] public bool pickedUp = false;
    public float enablePickupDelay = 0.5f; // Short delay before pickup is allowed

    private Collider itemCollider; // Store reference to the collider

    void Awake()
    {
        // Get the collider component when the object wakes up
        itemCollider = GetComponent<Collider>();
        if (itemCollider == null)
        {
            Debug.LogError("DroppedItem is missing a Collider component!", this.gameObject);
        }
    }

    // Called by the Inventory script when an item is dropped
    public void Initialize(Item itemToDrop)
    {
        item = itemToDrop;
        // Optional: Update visual appearance based on item if needed

        // Start delay before pickup is possible
        if (itemCollider != null)
        {
            itemCollider.enabled = false; // Disable collider initially
            StartCoroutine(EnablePickup(enablePickupDelay));
        }
    }

    IEnumerator EnablePickup(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (itemCollider != null)
        {
            itemCollider.enabled = true; // Enable collider after delay
            Debug.Log($"Collider enabled for dropped item: {item?.id}", this.gameObject); // Debug log
        }
    }
}