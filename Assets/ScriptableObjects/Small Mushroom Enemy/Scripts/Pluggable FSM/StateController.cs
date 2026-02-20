using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
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
    public State jumpState;

    public Transform chaseTarget;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public Animator animator;
    [HideInInspector] public float stateTimeElapsed;

    private Rigidbody rb;

    private bool aiActive = true;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponentInChildren<Rigidbody>();
    }

    private void Start()
    {
        if (chaseTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) chaseTarget = player.transform;
        }

        if (currentState != null)
        {
            currentState.OnEnterState(this);
        }
    }

    private void Update()
    {
        if (!aiActive) return;

        if (currentState != null)
            currentState.UpdateState(this);

        stateTimeElapsed += Time.deltaTime;
    }

    public void TransitionToState(State nextState)
    {
        // Only transition if it's a valid, different state
        if (nextState != remainState && nextState != null)
        {
            // 1. Exit current state
            if (currentState != null)
                currentState.OnExitState(this);

            // 2. Swap state
            currentState = nextState;

            // 3. Reset timer
            stateTimeElapsed = 0f;

            // 4. Enter new state
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
        if (!aiActive) return; // Prevent dead enemies from flinching

        if (hitState != null)
        {
            TransitionToState(hitState);
        }
    }

    public void OnDeath()
    {
        if (!aiActive) return; // Prevent multiple death calls

        if (deathState != null)
        {
            TransitionToState(deathState);

            // Shut down the AI brain completely so it stays dead
            aiActive = false;

            //// Disable collider or agent here so players don't bump into the corpse
            //if (navMeshAgent != null)
            //{
            //    navMeshAgent.enabled = false;
            //}
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Steps"))
        {
            Debug.Log("jump");
            StartCoroutine(nameof(JumpStateNav));
        }
    }

    IEnumerator JumpStateNav()
    {
        Debug.Log("jump");
        navMeshAgent.enabled = false;
        yield return new WaitForSeconds(5f);
        rb.isKinematic = true;
        rb.useGravity = true;
        Vector3 moveForce = new(0f,2f,0f);
        rb.AddForce(moveForce, ForceMode.Impulse);
    }
}