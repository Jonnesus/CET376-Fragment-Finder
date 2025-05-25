using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTargeting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private LayerMask groundLayer, playerLayer;
    [SerializeField] private Transform bulletSpawnPoint;

    //[Header("Patrolling")]
    //[SerializeField] private float walkPointRange;

    //private Vector3 walkPoint;
    //private bool walkPointSet;

    [Header("Attacking")]
    [SerializeField] private float timeBetweenAttacks;
    [SerializeField] private GameObject projectile;
    private bool alreadyAttacked;

    [Header("States")]
    //[SerializeField] private float sightRange;
    [SerializeField] private float attackRange;
    //[SerializeField] private bool playerInSightRange;
    [SerializeField] private bool playerInAttackRange;
    [SerializeField] private bool isRangedEnemy = false;

    private bool searchingForPlayer = false;
    private Transform playerTarget;
    private NavMeshAgent agent;
    private FieldOfView fov;

    System.Random rnd = new System.Random();

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();

        if (playerTarget == null && !searchingForPlayer)
        {
            searchingForPlayer=true;
            StartCoroutine(SearchForPlayer());
        }
    }

    void Update()
    {
        if (isRangedEnemy)
        {
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

            //if (!fov.canSeePlayer && !playerInAttackRange)
            //Patroling();

            if (fov.canSeePlayer && !playerInAttackRange)
                ChasePlayer();

            if (fov.canSeePlayer && playerInAttackRange)
                AttackPlayer();
        }
        else
        {
            if (fov.canSeePlayer)
                ChasePlayer();
        }
    }

/*    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundLayer))
            walkPointSet = true;
    }*/

    private void ChasePlayer()
    {
        agent.SetDestination(playerTarget.position);
    }

    private void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(playerTarget);

        if (!alreadyAttacked)
        {
            //Attack code here
            Rigidbody rb = Instantiate(projectile, bulletSpawnPoint.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !playerHealth.iFrames)
        {
            playerHealth.iFrames = true;
            playerHealth.TakeDamage(rnd.Next(5, 10));
        }
    }

    IEnumerator SearchForPlayer()
    {
        GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
        if (searchResult == null)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(SearchForPlayer());
        }
        else
        {
            playerTarget = searchResult.transform;
            playerHealth = searchResult.GetComponent<PlayerHealth>();
            searchingForPlayer = false;
            yield return false;
        }
    }
}