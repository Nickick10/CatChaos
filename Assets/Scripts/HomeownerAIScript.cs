using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.SceneManagement;

public class HomeownerAIScript : MonoBehaviour
{   
    private Animator anim;
    private bool isWaiting = false;
    private bool isChaseMusicPlaying = false;

    private float noiseTriggerThreshold = 0.85f; // 80%
    private float noiseTriggerPatrolThreshold = 0.3f; // 50%

    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    //Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public AudioClip swoosh; 
    [SerializeField] private AudioSource audioSource;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    private bool triggeredChase = false;
    private Coroutine chaseCoroutine;

    [SerializeField] private SceneAudioManager audioManager;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("PlayerCat").transform;
        anim = GetComponent<Animator>();
    }

    public void Update()
    {
        float currentNoise = NoiseManager.Instance.GetNoiseLevel();
        float maxNoise = Mathf.Max(NoiseManager.Instance.GetMaxNoise(), 0.01f); // avoid divide by zero
        float noisePercentage = currentNoise / maxNoise;

        // Check for proximity
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (playerInAttackRange && playerInSightRange)
        {
            // Stop persistent chase if we're ready to attack
            StopPersistentChase();
            AttackPlayer();
        }
        else if (triggeredChase || playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (!playerInSightRange && !playerInAttackRange && noisePercentage >= noiseTriggerThreshold)
        {
            //Trigger chase if the noise level is above the threshold
            TriggerPersistentChase(12f); //Chase for 12 seconds
            ChasePlayer();
        }
        else if (!playerInSightRange && !playerInAttackRange && noisePercentage >= noiseTriggerPatrolThreshold)
        {
            Patrol();
            EndChaseMusic();
        }
        else if (!playerInSightRange && !playerInAttackRange && !triggeredChase)
        {   
            Patrol();
            EndChaseMusic();
        }
    }

    private void Patrol()
    {   
        if (!walkPointSet && !isWaiting) SearchWalkPoint();

        if (walkPointSet && !isWaiting)
        {
            agent.speed = 3.5f;
            agent.SetDestination(walkPoint);
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", true);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        if (!isWaiting && walkPointSet && distanceToWalkPoint.magnitude < 1f)
        {
            StartCoroutine(IdleBeforeNextWalk());
        }
    }

    private IEnumerator IdleBeforeNextWalk()
    {
        isWaiting = true;
        walkPointSet = false;

        agent.SetDestination(transform.position); //Stop moving
        anim.SetBool("isRunning", false);
        anim.SetBool("isWalking", false); //Play idle animation

        yield return new WaitForSeconds(5f); //Wait for 5 seconds

        isWaiting = false;
    }


    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
            agent.SetDestination(walkPoint);
        }
    }

    private void ChasePlayer()
    {
        TriggerChaseMusic();
        agent.speed = 4.5f; // Chase speed

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(agent.transform.position, player.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            //Only set the destination if a complete path is found
            agent.SetDestination(player.position);
            anim.SetBool("isRunning", true);
            anim.SetBool("isWalking", false);
        }
        else
        {
            Patrol(); //If no path is found patrol instead
        }
    }

    private void AttackPlayer()
    {   
        anim.SetBool("isRunning", false);
        anim.SetBool("isWalking", false);

        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            //Add attack code here depending on what we want later
            audioSource.Stop();
            audioSource.clip = swoosh;
            audioSource.loop = false;
            audioSource.Play();

            // anim.SetBool("isAttacking", true);
            PlayerPrefs.DeleteKey("HasSeenCredits");
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("GameOver");
    }

    private IEnumerator StopChasingAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        triggeredChase = false;
    }

    public void TriggerPersistentChase(float duration)
    {
        triggeredChase = true;
        if (chaseCoroutine != null) StopCoroutine(chaseCoroutine);
        chaseCoroutine = StartCoroutine(StopChasingAfterTime(duration));
    }

    private void StopPersistentChase()
    {
        triggeredChase = false;
        if (chaseCoroutine != null) StopCoroutine(chaseCoroutine);
        chaseCoroutine = null;
    }

    public void TriggerChaseMusic()
    {
        if (audioManager != null && !isChaseMusicPlaying) {
            audioManager.PlayChaseMusic();
            isChaseMusicPlaying = true;
        }
    }

    public void EndChaseMusic()
    {
        if (audioManager != null && isChaseMusicPlaying) {
            audioManager.StopChaseMusicAndResumeBGM();
            isChaseMusicPlaying = false;
        }
    }
}