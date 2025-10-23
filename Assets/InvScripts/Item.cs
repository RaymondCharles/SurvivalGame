using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "RumbledCode/Item", order = 1)]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public string id;
    public string desc;
    public Sprite icon;
    public GameObject prefab; // Prefab for the dropped item

    [Header("Usage Settings")]
    public bool isConsumable;       // Can the player use it from inventory?
    public int hpRestoreAmount;     // How much HP it restores
    public int hungerRestoreAmount; // How much Hunger it restores

    public bool canBeDropped = true;

    // --- NEW METHOD ---
    // This virtual method defines what happens when the item is used.
    // It takes the PlayerVitals component of the user as input.
    // Returns true if the item was successfully used.
    public virtual bool Use(PlayerVitals playerVitals)
    {
        // Default behavior: Check if it restores health or hunger
        bool used = false;
        if (isConsumable && playerVitals != null)
        {
            if (hpRestoreAmount > 0)
            {
                playerVitals.Heal(hpRestoreAmount);
                Debug.Log($"Used {id}, restored {hpRestoreAmount} HP.");
                used = true;
            }
            if (hungerRestoreAmount > 0)
            {
                // Assuming PlayerVitals.Eat handles both hunger and potential HP
                playerVitals.Eat(hungerRestoreAmount, 0); // Eat takes calories, optionally HP
                Debug.Log($"Used {id}, restored {hungerRestoreAmount} Hunger.");
                used = true;
            }
        }
        return used;
    }
    // --- END NEW METHOD ---
}