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

    private bool isBleeding = false;
    private void Awake()
    {
        if (playerTarget == null)
        {
            FindPlayerTarget();
        }

        if (fishGO != null)
        {
            fishGO.SetActive(false);
        }
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
            RotateTowardsPlayer();
            MoveTowardsPlayer();

            if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange)
            {
                StartEmerge();
            }
        }
    }
    private void FindPlayerTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (hits.Length > 0)
        {
            playerTarget = hits[0].transform;
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    private void MoveTowardsPlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, moveSpeed * Time.deltaTime);
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
            Vector3 targetPosition = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
            transform.position = targetPosition;
        }

        StartEmerge();
    }

    public void Disappear()
    {
        if (fishGO != null)
        {
            fishGO.SetActive(false);
        }

        currentState = FishState.HiddenIdle;
        isBleeding = false;
        StopAllCoroutines();
    }
    private void StartEmerge()
    {
        currentState = FishState.Emerging;
        animator.Play("Emerge");
    }

    public void DealDamageEvent()
    {
        if (!isBleeding && playerTarget != null)
        {
            if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange)
            {
                StartCoroutine(ApplyBleedEffect());
            }
        }
    }

    public void OnEmergeComplete()
    {
        currentState = FishState.Hiding;
        animator.Play("Hide");
    }

    public void OnHideComplete()
    {
        currentState = FishState.HiddenIdle;
        animator.Play("HiddenIdle");
    }
    private IEnumerator ApplyBleedEffect()
    {
        isBleeding = true;

        if (playerTarget != null)
        {
            playerTarget.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }

        for (int i = 0; i < bleedTicks; i++)
        {
            yield return new WaitForSeconds(bleedTickInterval);

            if (playerTarget != null)
            {
                playerTarget.SendMessage("TakeDamage", bleedDamagePerTick, SendMessageOptions.DontRequireReceiver);
            }
        }

        isBleeding = false;
    }
}