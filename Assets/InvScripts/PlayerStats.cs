using TMPro;  // Import TextMeshPro namespace
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int hp;       // Current HP value
    public int hunger;   // Current Hunger value

    public TMP_Text hpText;      // Assign in Inspector
    public TMP_Text hungerText;  // Assign in Inspector

    public void IncreaseStats(int hpAmount, int hungerAmount)
    {
        hp += hpAmount;
        hunger += hungerAmount;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (hpText != null)
            hpText.text = $"HP: {hp}";

        if (hungerText != null)
            hungerText.text = $"Hunger: {hunger}";
    }

    public void TakeDamage(int amount)
    {
        Debug.Log("Took Damage!");
        hp -= amount;
        Debug.Log("New hp " + hp);
    }

    public void ReduceHunger(int amount)
    {
        hunger -= amount;
    }

    public void Heal(int amount)
    {
        hp += amount;
    }

    public void IncreaseHunger(int amount)
    {
        hunger += amount;
    }
}
