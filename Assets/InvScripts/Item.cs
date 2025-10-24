using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "RumbledCode/Item", order = 1)]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public string id;
    public string desc;
    public Sprite icon;
    public GameObject prefab;

    [Header("Usage Settings")]
    public bool isConsumable;      // Can the player use it?
    public float hpRestoreAmount;    // How much HP it restores
    public float hungerRestoreAmount;// How much Hunger it restores

    // Optional: if you want items that can be both dropped and used differently
    public bool canBeDropped = true;
}
