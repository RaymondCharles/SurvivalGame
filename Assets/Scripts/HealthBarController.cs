using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Slider

public class HealthBarController : MonoBehaviour
{
    public Slider slider;
    public EnemyHealth enemyHealth; // Reference to the enemy's health script

    void Update()
    {
        // Update the slider's value to match the enemy's health percentage
        slider.value = enemyHealth.currentHealth / enemyHealth.maxHealth;
    }
}