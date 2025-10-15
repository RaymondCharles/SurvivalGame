using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // --- Assign these in the Inspector ---
    [Header("Required Components")]
    public NavMeshAgent agent;
    public Transform player;
    public GameObject projectilePrefab; // The projectile PREFAB from your Project folder
    public Transform firePoint;         // The empty child object positioned in front of the enemy
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;

    [Header("AI Stats")]
    public float sightRange = 30f;
    public float rangedAttackRange = 15f; // IMPORTANT: Must be > agent's stoppingDistance
    public float meleeAttackRange = 3f;   // A new, very close range for punching

    [Header("Attack Settings")]
    public float timeBetweenAttacks = 1.5f;
    public float projectileSpeed = 60f;

    // --- Private variables ---
    private Animator animator; // For playing the punch animation
    private bool playerInSightRange, playerInRangedRange, playerInMeleeRange;
    private bool alreadyAttacked;

    private void Awake()
    {
        // Get the required components automatically
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Stop all logic if the player doesn't exist
        if (player == null) return;

        // Check distances to the player to determine the current state
        playerInSightRange = Vector3.Distance(transform.position, player.position) <= sightRange;
        playerInRangedRange = Vector3.Distance(transform.position, player.position) <= rangedAttackRange;
        playerInMeleeRange = Vector3.Distance(transform.position, player.position) <= meleeAttackRange;

        // --- State Machine with Melee Priority ---
        if (!playerInSightRange)
        {
            Patroling();
        }
        else if (playerInMeleeRange)
        {
            MeleeAttack();
        }
        else if (playerInRangedRange)
        {
            RangedAttack();
        }
        else
        {
            ChasePlayer(); // If in sight but out of all attack ranges
        }
    }

    // This function runs every frame AFTER Update() and is the permanent fix for the floating bug
    private void LateUpdate()
    {
        RaycastHit hit;
        // Fire a raycast straight down from the enemy's current position
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, whatIsGround))
        {
            // We hit the ground. Find the correct Y position by adding the agent's built-in offset.
            float targetY = hit.point.y + agent.baseOffset;

            // Manually force the enemy's position to that Y level, keeping its X and Z position.
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        }
    }

    private void Patroling()
    {
        // For now, the enemy will stand still when it can't see the player.
        agent.SetDestination(transform.position);
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void MeleeAttack()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            // Trigger the "Punch" animation
            animator.SetTrigger("Punch");

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void RangedAttack()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            // Create a projectile at the FirePoint's position and rotation
            GameObject currentProjectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = currentProjectile.GetComponent<Rigidbody>();

            // Calculate the exact direction from the fire point to the player
            Vector3 direction = (player.position - firePoint.position).normalized;

            // Add a powerful force in that direction
            rb.AddForce(direction * projectileSpeed, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);

            // Destroy the projectile after 5 seconds to clean up the scene
            Destroy(currentProjectile, 5f);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}