using UnityEngine;
using UnityEngine.AI; // Required to access the NavMeshAgent

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    // You can change this value in the Inspector to control the death delay
    public float deathDelay = 3f;

    private PassiveAI passiveAI;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        passiveAI = GetComponent<PassiveAI>();
    }

    public void TakeDamage(float amount)
    {
        // Don't do anything if the enemy is already dead
        if (isDead) return;

        currentHealth -= amount;

        // If this is a passive AI, provoke it
        if (passiveAI != null)
        {
            passiveAI.BecomeHostile();
        }

        // Check if health has dropped to zero
        if (currentHealth <= 0)
        {
            isDead = true; // Set the flag so Die() is only called once
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has been defeated.");

        // Disable the AI's brain and collider so it stops moving and interacting
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // --- THIS IS THE FIX ---
        // Destroy the object after the specified delay
        Destroy(gameObject, deathDelay);
    }
}