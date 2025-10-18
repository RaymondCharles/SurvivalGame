using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Required Components")]
    public NavMeshAgent agent;
    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public LayerMask whatIsGround;

    [Header("Health - Managed by EnemyHealth.cs")]
    // Health logic is now in the separate EnemyHealth.cs script

    [Header("AI Stats")]
    public float sightRange = 30f;
    public float rangedAttackRange = 15f;
    public float meleeAttackRange = 3f;

    [Header("Attack Settings")]
    public float timeBetweenAttacks = 1.5f;
    public float projectileSpeed = 60f;

    private Animator animator;
    private bool playerInSightRange, playerInRangedRange, playerInMeleeRange;
    private bool alreadyAttacked;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (player == null || agent == null || !agent.isOnNavMesh) return;

        playerInSightRange = Vector3.Distance(transform.position, player.position) <= sightRange;
        playerInRangedRange = Vector3.Distance(transform.position, player.position) <= rangedAttackRange;
        playerInMeleeRange = Vector3.Distance(transform.position, player.position) <= meleeAttackRange;

        if (!playerInSightRange) Patroling();
        else if (playerInMeleeRange) MeleeAttack();
        else if (playerInRangedRange) RangedAttack();
        else ChasePlayer();
    }


    // --- THIS IS THE SAFETY NET CODE THAT WAS MISSING ---
    // This function runs every frame and forces the enemy to stay on the ground.
    private void LateUpdate()
    {
        if (agent == null) return;
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, whatIsGround))
        {
            float targetY = hit.point.y + agent.baseOffset;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        }
    }

    private void Patroling()
    {
        if (agent.isOnNavMesh) 
        {
            transform.LookAt(player.position);
            agent.SetDestination(transform.position);
        }
    }

    private void ChasePlayer()
    {
        if (agent.isOnNavMesh) agent.SetDestination(player.position);
    }

    private void MeleeAttack()
    {
        if (agent.isOnNavMesh) agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            if (animator != null) animator.SetTrigger("Punch");
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void RangedAttack()
    {
        if (agent.isOnNavMesh) agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            GameObject p = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = p.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (player.position - firePoint.position).normalized;
                rb.AddForce(dir * projectileSpeed, ForceMode.Impulse);
            }
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
            Destroy(p, 5f);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}