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

    [Header("AI Stats")]
    public float health = 100f;
    public float sightRange = 30f;
    public float attackRange = 15f;

    [Header("Attack Settings")]
    public float timeBetweenAttacks = 3f;
    public float projectileSpeed = 60f;

    // --- Private variables ---
    private bool isHostile = false;
    private bool playerInSightRange, playerInAttackRange;
    private bool alreadyAttacked;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!isHostile || player == null)
        {
            // ADDED SAFETY CHECK
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(transform.position);
            }
            return;
        }

        playerInSightRange = Vector3.Distance(transform.position, player.position) <= sightRange;
        playerInAttackRange = Vector3.Distance(transform.position, player.position) <= attackRange;

        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        else if (playerInSightRange && playerInAttackRange) AttackPlayer();
    }

    public void TakeDamage(float damage)
    {
        isHostile = true;
        health -= damage;
        if (health <= 0) Destroy(gameObject);
    }

    private void ChasePlayer()
    {
        // ADDED SAFETY CHECK
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    private void AttackPlayer()
    {
        // ADDED SAFETY CHECK
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(transform.position);
        }
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