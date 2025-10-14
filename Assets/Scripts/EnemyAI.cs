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
    public float attackRange = 15f;     // IMPORTANT: This MUST be greater than the agent's Stopping Distance

    [Header("Attack Settings")]
    public float projectileSpeed = 60f;
    public float timeBetweenAttacks = 1.5f;

    // --- Private variables ---
    private bool playerInSightRange, playerInAttackRange;
    private bool alreadyAttacked;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (player == null) return;

        playerInSightRange = Vector3.Distance(transform.position, player.position) <= sightRange;
        playerInAttackRange = Vector3.Distance(transform.position, player.position) <= attackRange;

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        else if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        else if (playerInSightRange && playerInAttackRange) AttackPlayer();
    }

    // This function is the safety net that prevents floating
    private void LateUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, whatIsGround))
        {
            float targetY = hit.point.y + agent.baseOffset;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        }
    }

    private void Patroling()
    {
        agent.SetDestination(transform.position);
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            GameObject currentProjectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = currentProjectile.GetComponent<Rigidbody>();
            Vector3 direction = (player.position - firePoint.position).normalized;
            rb.AddForce(direction * projectileSpeed, ForceMode.Impulse);

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