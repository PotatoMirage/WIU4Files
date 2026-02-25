using UnityEngine;
using UnityEngine.AI;

public class StateController : MonoBehaviour
{
    [Header("State Machine")]
    public State currentState;
    public State remainState;

    [Header("Event Overrides")]
    public State hitState;
    public State deathState;

    [Header("Spider Specific")]
    public Transform lookPoint;
    public Transform projectileSpawnPoint;
    public Collider spiderCollider;
    public bool isOnWall = true;
    public bool startsOnWall = false;

    [Header("Target")]
    public Transform chaseTarget;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Rigidbody rigidBody;
    [HideInInspector] public float stateTimeElapsed;

    private bool aiActive = true;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (chaseTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) chaseTarget = player.transform;
        }

        if (currentState != null)
            currentState.OnEnterState(this);
    }

    private void Update()
    {
        if (!aiActive) return;
        if (chaseTarget != null)
        {
            PlayerMovementScript playerMovement = chaseTarget.GetComponent<PlayerMovementScript>();
            if (playerMovement != null && playerMovement.IsDead)
            {
                chaseTarget = null;
            }
        }
        if (currentState != null)
            currentState.UpdateState(this);

        stateTimeElapsed += Time.deltaTime;
    }

    public void TransitionToState(State nextState)
    {
        if (nextState != remainState && nextState != null)
        {
            if (currentState != null)
                currentState.OnExitState(this);

            currentState = nextState;
            stateTimeElapsed = 0f;

            if (currentState != null)
                currentState.OnEnterState(this);
        }
    }

    public bool CheckIfCountDownElapsed(float duration)
    {
        return (stateTimeElapsed >= duration);
    }

    public void OnTakeDamage()
    {
        if (!aiActive) return;

        SpiderController spider = GetComponent<SpiderController>();

        if (isOnWall && spider != null)
        {
            spider.currentWallHits++;
            Debug.Log($"Wall hits: {spider.currentWallHits}/{spider.hitsToFall}");

            if (spider.currentWallHits >= spider.hitsToFall)
            {
                spider.currentWallHits = 0;
                TransitionToState(spider.wallFallState);
            }
            else
            {
                if (spider.wallHitState != null)
                    TransitionToState(spider.wallHitState);
            }
        }
        else
        {
            if (hitState != null)
                TransitionToState(hitState);
        }
    }

    public void OnDeath()
    {
        if (!aiActive) return;

        if (deathState != null)
        {
            TransitionToState(deathState);
            aiActive = false;

            if (navMeshAgent != null)
                navMeshAgent.enabled = false;
        }
    }
}