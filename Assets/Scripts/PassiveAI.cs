using UnityEngine;
using UnityEngine.AI;

public class PassiveAI : MonoBehaviour
{
    [Header("Required Components")]
    public NavMeshAgent agent;
    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public LayerMask whatIsGround;

    [Header("AI Behaviour")]
    public float attackRange = 20f;

    [Header("Attack Settings")]
    public float timeBetweenAttacks = 1.5f;
    public float projectileSpeed = 60f;
    public int projectileDamage = 5;

    private bool isHostile = false;
    private bool alreadyAttacked = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // If the AI is not hostile...
        if (!isHostile)
        {
            // ...completely disable its NavMeshAgent brain.
            if (agent.enabled)
            {
                agent.enabled = false;
            }
            return; // And do nothing else.
        }

        // --- If Hostile ---
        if (player == null) return;

        bool playerInAttackRange = Vector3.Distance(transform.position, player.position) <= attackRange;

        if (playerInAttackRange)
        {
            AttackPlayer();
        }
        else
        {
            // If it's hostile but the player is out of range, just look at the player.
            transform.LookAt(player);
        }
    }

    // This is called by the EnemyHealth script when this AI takes damage
    public void BecomeHostile()
    {
        if (isHostile) return; // Only run this once

        isHostile = true;
        // Turn its brain back on so it can attack.
        if (agent != null)
        {
            agent.enabled = true;
        }
        Debug.Log(gameObject.name + " has become hostile!");
    }

    private void AttackPlayer()
    {
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            GameObject currentProjectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            currentProjectile.GetComponent<enemyProjectileScript>().Damage = projectileDamage;
            Rigidbody rb = currentProjectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (player.position - firePoint.position).normalized;
                rb.AddForce(direction * projectileSpeed, ForceMode.Impulse);
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
            Destroy(currentProjectile, 5f);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}