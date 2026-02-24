using System.Collections;
using UnityEngine;

public class FishEnemy : MonoBehaviour
{
    public enum FishState
    {
        HiddenIdle,
        Emerging,
        EmergeIdle,
        Hiding
    }

    public FishState currentState = FishState.HiddenIdle;

    public GameObject fishGO;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Animator animator;

    public float moveSpeed = 4f;
    public int attackDamage = 10;
    public int bleedDamagePerTick = 2;
    public int bleedTicks = 5;
    public float bleedTickInterval = 1f;
    public float detectionRadius = 100f;
    public float attackRange = 2.5f;
    public LayerMask playerLayer;
    public PlayerMovementScript playerMovement;

    [SerializeField] private float hiddenDepth = 2f;
    private float originalY;
    private bool isBleeding = false;

    private void Awake()
    {
        originalY = transform.position.y;

        if (playerTarget == null)
        {
            FindPlayerTarget();
        }

        if (fishGO != null)
        {
            fishGO.SetActive(false);
        }

        transform.position = new Vector3(transform.position.x, originalY - hiddenDepth, transform.position.z);
    }

    private void Update()
    {
        if (playerTarget == null)
        {
            FindPlayerTarget();
            return;
        }

        if (currentState == FishState.HiddenIdle)
        {
            MoveTowardsPlayer();

            float distanceToPlayer = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z), new Vector3(playerTarget.position.x, 0f, playerTarget.position.z));

            if (distanceToPlayer <= attackRange)
            {
                StartEmerge();
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 targetPosition = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        Vector3 directionToFace = targetPosition - transform.position;

        if (directionToFace != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToFace);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    public void Appear()
    {
        if (fishGO != null)
        {
            fishGO.SetActive(true);
        }

        if (playerTarget == null)
        {
            FindPlayerTarget();
        }

        if (playerTarget != null)
        {
            Vector3 targetPosition = new(playerTarget.position.x, 0, playerTarget.position.z);
            transform.position = targetPosition;
        }

        StartEmerge();
    }
    private void StartEmerge()
    {
        currentState = FishState.Emerging;
        transform.position = new Vector3(transform.position.x, originalY, transform.position.z);

        if (fishGO != null)
        {
            fishGO.SetActive(true);
        }

        if (animator != null)
        {
            animator.Play("Emerge");
        }
    }

    public void OnEmergeComplete()
    {
        currentState = FishState.Hiding;

        if (animator != null)
        {
            animator.Play("Hide");
        }
    }

    public void OnHideComplete()
    {
        currentState = FishState.HiddenIdle;
        transform.position = new Vector3(transform.position.x, originalY - hiddenDepth, transform.position.z);

        if (animator != null)
        {
            animator.Play("HiddenIdle");
        }
    }

    public void Disappear()
    {
        if (fishGO != null)
        {
            fishGO.SetActive(false);
        }

        currentState = FishState.HiddenIdle;
        transform.position = new Vector3(transform.position.x, originalY - hiddenDepth, transform.position.z);
        isBleeding = false;
        StopAllCoroutines();
    }

    public void DealDamageEvent()
    {
        if (!isBleeding && playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z), new Vector3(playerTarget.position.x, 0f, playerTarget.position.z));

            if (distanceToPlayer <= attackRange)
            {
                StartCoroutine(ApplyBleedEffect());
            }
        }
    }

    private IEnumerator ApplyBleedEffect()
    {
        isBleeding = true;

        if (playerTarget != null && playerMovement != null)
        {
            playerMovement.ChangeHealth(-attackDamage);
        }

        for (int i = 0; i < bleedTicks; i++)
        {
            yield return new WaitForSeconds(bleedTickInterval);

            if (playerTarget != null && playerMovement != null)
            {
                playerMovement.ChangeHealth(-bleedDamagePerTick);
            }
        }

        isBleeding = false;
    }

    private void FindPlayerTarget()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
            playerMovement = playerObject.GetComponent<PlayerMovementScript>();
        }
    }
}