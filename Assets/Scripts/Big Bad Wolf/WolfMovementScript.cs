using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WolfMovementScript : MonoBehaviour
{
    [Header("Wolf Health Settings")]
    public Animator animator;
    public int health = 400;
    public int maxHealth = 400;

    [Header("Wolf Movement Settings")]
    public GameManager gameManager;
    public WolfAttackScript wolfAttack;
    public PlayerMovementScript playerMovement;
    public GameObject phaseTransitionVFXPrefab;
    public GameObject phaseTwoAuraVFXPrefab;
    public float moveSpeed = 20.0f;
    public float rotationSpeed = 5.0f;
    public float attackRange = 2.5f;

    private NavMeshAgent navAgent;
    private Rigidbody rigidBody;
    private GameObject phase2VFXInstance;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool isDead, isBossFightStarted, wasAttacking, isReturningToSpawn, isPhaseTransition, isStumbling, isStunned;
    private int currentPhase = 1;
    private float rangedAttackCheckTimer;
    private string currentAnimation;

    // Awake is called when loading an instance of a script component
    void Awake()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navAgent.speed = moveSpeed;
        navAgent.stoppingDistance = attackRange;
        navAgent.isStopped = true;
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        wolfAttack.SetSpawnPosition(spawnPosition);
        PlayAnimation("BigBadWolf_Idle");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isBossFightStarted || isDead || isPhaseTransition || isStumbling || isStunned)
            return;

        // If the player died, force stop attacks and walk back to spawn
        if (playerMovement.IsDead)
        {
            if (!isReturningToSpawn)
            {
                isReturningToSpawn = true;
                wasAttacking = false;
                currentAnimation = "";
                wolfAttack.ForceStopAttack();
            }

            if (Vector3.Distance(transform.position, spawnPosition) <= 0.2f)
            {
                if (navAgent.enabled)
                {
                    navAgent.isStopped = true;
                    navAgent.velocity = Vector3.zero;
                }
                transform.rotation = spawnRotation;
                PlayAnimation("BigBadWolf_Idle");
            }
            else
            {
                if (navAgent.enabled)
                {
                    navAgent.isStopped = false;
                    navAgent.SetDestination(spawnPosition);
                }
                PlayAnimation("BigBadWolf_Chase");
            }

            return;
        }

        if (isReturningToSpawn)
            isReturningToSpawn = false;

        // Detect when the attack ends so it can reset and forces a transition back to default
        if (!wolfAttack.IsAttacking && wasAttacking)
            currentAnimation = "";

        wasAttacking = wolfAttack.IsAttacking;

        float distance = Vector3.Distance(transform.position, playerMovement.transform.position);
        bool inRange = distance <= attackRange;

        if (wolfAttack.IsAttacking)
        {
            if (navAgent.enabled)
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero;
            }
            FacePlayer();
            return;
        }

        if (inRange)
        {
            if (navAgent.enabled)
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero;
            }
            FacePlayer();

            if (wolfAttack.CanAttack)
                wolfAttack.TryMeleeAttack();
            else
                PlayAnimation("BigBadWolf_Idle");
        }
        else
        {
            // Only active on Phase two, roll chance for a special attack while chasing
            if (currentPhase == 2 && wolfAttack.CanAttack && !wolfAttack.IsAttacking)
            {
                rangedAttackCheckTimer -= Time.deltaTime;
                if (rangedAttackCheckTimer <= 0.0f)
                {
                    rangedAttackCheckTimer = 0.5f;
                    if (Random.value < 0.2f)
                    {
                        wolfAttack.TryRangedAttack();
                        return;
                    }
                }
            }

            if (navAgent.enabled)
            {
                navAgent.isStopped = false;
                navAgent.SetDestination(playerMovement.transform.position);
            }
            PlayAnimation("BigBadWolf_Chase");
        }
    }

    public void StartBossFight()
    {
        isBossFightStarted = true;
    }

    void FacePlayer()
    {
        Vector3 faceDirection = playerMovement.transform.position - transform.position;
        faceDirection.y = 0;

        if (faceDirection == Vector3.zero)
            return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(faceDirection), rotationSpeed * Time.deltaTime);
    }

    public void TakeDamage(float amount)
    {
        // Ignore damage if the boss fight has not started or wolf is dead / transitioning
        if (!isBossFightStarted || isDead || isPhaseTransition) return;

        health = Mathf.Clamp(health - (int)amount, 0, maxHealth);

        // Trigger phase two transition when the boss health drops
        // to 100 or below at the first phase
        if (currentPhase == 1 && health <= 100)
        {
            StartCoroutine(PhaseTransition());
            return;
        }

        if (health == 0)
            TriggerDeath();
    }

    public void TriggerStumble()
    {
        if (isDead || isPhaseTransition || isStumbling)
            return;
        StartCoroutine(StumbleCoroutine());
    }

    public void TriggerStunned()
    {
        if (isDead || isStunned || !wolfAttack.IsRetreating)
            return;
        StartCoroutine(StunCoroutine());
    }

    void TriggerDeath()
    {
        isDead = true;
        gameManager.PlaySFX(5);
        if (phase2VFXInstance != null) Destroy(phase2VFXInstance);
        navAgent.isStopped = true;
        navAgent.velocity = Vector3.zero;
        wolfAttack.ForceStopAttack();
        animator.CrossFadeInFixedTime("BigBadWolf_Death", 0.25f);
    }

    void PlayAnimation(string animationName)
    {
        if (animationName == currentAnimation) return;

        animator.CrossFadeInFixedTime(animationName, 0.25f);
        currentAnimation = animationName;
    }

    IEnumerator StumbleCoroutine()
    {
        isStumbling = true;
        gameManager.PlaySFX(Random.Range(0, 3));
        wolfAttack.ForceStopAttack();
        navAgent.isStopped = true;
        navAgent.velocity = Vector3.zero;
        animator.CrossFadeInFixedTime("BigBadWolf_Stumble", 0.1f);

        yield return new WaitForSeconds(0.5f);

        isStumbling = false;
        currentAnimation = "";
    }

    IEnumerator StunCoroutine()
    {
        isStunned = true;
        wolfAttack.TriggerLevitatingStun();

        Vector3 airPos = transform.position;
        Vector3 groundPos = new(airPos.x, spawnPosition.y, airPos.z);

        if (Physics.Raycast(airPos, Vector3.down, out RaycastHit groundHit, 20.0f))
            groundPos.y = groundHit.point.y;

        float elapsed = 0.0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(airPos, groundPos, elapsed / 0.4f);
            yield return null;
        }

        transform.position = groundPos;
        navAgent.enabled = true;
        navAgent.Warp(groundPos);
        rigidBody.isKinematic = true;
        animator.CrossFadeInFixedTime("BigBadWolf_StunnedLoop", 0.1f);
        currentAnimation = "BigBadWolf_StunnedLoop";

        yield return new WaitForSeconds(5.0f);

        isStunned = false;
        currentAnimation = "";
    }

    // Teleports wolf back to the starting point, levitates while regenerate health
    // in preparation for the second phase of the boss battle
    IEnumerator PhaseTransition()
    {
        isPhaseTransition = true;
        currentPhase = 2;
        gameManager.PlaySFX(3);
        currentAnimation = "";
        wolfAttack.ForceStopAttack();
        navAgent.isStopped = true;
        navAgent.velocity = Vector3.zero;
        navAgent.Warp(spawnPosition);
        transform.rotation = spawnRotation;
        navAgent.enabled = false;
        rigidBody.isKinematic = true;
        animator.CrossFadeInFixedTime("BigBadWolf_IdleAir", 0.25f);
        currentAnimation = "BigBadWolf_IdleAir";
        GameObject transitionVFX = phaseTransitionVFXPrefab != null ? Instantiate(phaseTransitionVFXPrefab, transform) : null;

        Vector3 groundPos = spawnPosition;
        Vector3 airPos = spawnPosition + Vector3.up * 3.0f;
        int startHealth = health;
        float totalElapsed = 0.0f;
        float elapsed = 0.0f;

        // Levitating animation
        while (elapsed < 1.5f)
        {
            elapsed += Time.deltaTime;
            totalElapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(groundPos, airPos, elapsed / 1.5f);
            health = Mathf.RoundToInt(Mathf.Lerp(startHealth, 400, totalElapsed / 5.0f));
            yield return null;
        }

        // Hover animation
        elapsed = 0.0f;
        while (elapsed < 2.0f)
        {
            elapsed += Time.deltaTime;
            totalElapsed += Time.deltaTime;
            health = Mathf.RoundToInt(Mathf.Lerp(startHealth, 400, totalElapsed / 5.0f));
            yield return null;
        }

        // Descend animation
        elapsed = 0.0f;
        while (elapsed < 1.5f)
        {
            elapsed += Time.deltaTime;
            totalElapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(airPos, groundPos, elapsed / 1.5f);
            health = Mathf.RoundToInt(Mathf.Lerp(startHealth, 400, totalElapsed / 5.0f));
            yield return null;
        }

        transform.position = groundPos;
        if (transitionVFX != null)
            Destroy(transitionVFX);

        // Re-enable NavMeshAgent and strengthen the big bad wolf
        navAgent.enabled = true;
        navAgent.Warp(groundPos);
        maxHealth = 800;
        health = 800;
        wolfAttack.EnterSecondPhase();

        if (phaseTwoAuraVFXPrefab != null)
            phase2VFXInstance = Instantiate(phaseTwoAuraVFXPrefab, transform);

        isPhaseTransition = false;
        currentAnimation = "";
        PlayAnimation("BigBadWolf_Idle");
    }

    public bool IsDead => isDead;
    public bool IsRetreating => wolfAttack.IsRetreating;
    public int CurrentPhase => currentPhase;
}