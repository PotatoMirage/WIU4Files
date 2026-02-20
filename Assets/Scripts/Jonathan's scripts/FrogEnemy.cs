using System.Collections;
using UnityEngine;

public class FrogEnemy : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public float emergeRadius = 10f;
    public float hideRadius = 15f;
    public float hideDelay = 3f;
    public float moveSpeed = 3f;

    public Transform hiddenPosition;
    public Transform emergedPosition;

    public float attackCooldown = 2f;
    public GameObject fliesPrefab;
    public Transform mouthSpawnPoint;

    public Collider mouthCollider;
    public Collider tongueCollider;
    public Animator frogAnimator;

    private Transform playerTarget;
    private bool isEmerged = false;
    private float timeOutOfRange = 0f;
    private float lastAttackTime = 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
        FindTarget();
    }
    private void Start()
    {
        if (hiddenPosition != null)
        {
            transform.position = hiddenPosition.position;
        }
        
        isEmerged = false;
        timeOutOfRange = 0f;
        
        if (mouthCollider != null)
        {
            mouthCollider.enabled = false;
        }

        if (tongueCollider != null)
        {
            tongueCollider.enabled = false;
        }
    }
    private void Update()
    {
        if (playerTarget == null)
        {
            FindTarget();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (!isEmerged)
        {
            if (distanceToPlayer <= emergeRadius)
            {
                isEmerged = true;
                timeOutOfRange = 0f;
                StopAllCoroutines();
                StartCoroutine(MoveToPosition(emergedPosition.position));
            }
        }
        else
        {
            if (distanceToPlayer > hideRadius)
            {
                timeOutOfRange += Time.deltaTime;
                if (timeOutOfRange >= hideDelay)
                {
                    isEmerged = false;
                    StopAllCoroutines();
                    StartCoroutine(MoveToPosition(hiddenPosition.position));
                }
            }
            else
            {
                timeOutOfRange = 0f;

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    PerformRandomAttack();
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    private void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;
    }

    private IEnumerator MoveToPosition(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void PerformRandomAttack()
    {
        int attackChoice = Random.Range(0, 2);
        if (attackChoice == 0)
        {
            TongueAttack();
        }
        else
        {
            SummonFlies();
        }
    }

    private void TongueAttack()
    {
        if (frogAnimator != null)
        {
            frogAnimator.SetTrigger("TongueAttack");
        }
    }

    private void SummonFlies()
    {
        if (frogAnimator != null)
        {
            frogAnimator.SetTrigger("SummonFlies");
        }

        if (fliesPrefab != null && mouthSpawnPoint != null)
        {
            GameObject flies = Instantiate(fliesPrefab, mouthSpawnPoint.position, Quaternion.identity);
            FliesEnemy fliesScript = flies.GetComponent<FliesEnemy>();
            if (fliesScript != null)
            {
                fliesScript.SetTarget(playerTarget);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
    public void OpenMouth()
    {
        if (mouthCollider != null)
        {
            mouthCollider.enabled = true;
        }
    }
    public void CloseMouth()
    {
        if (mouthCollider != null)
        {
            mouthCollider.enabled = false;
        }
    }
    public void EnableTongueCollider()
    {
        if (tongueCollider != null)
        {
            tongueCollider.enabled = true;
        }
    }
    public void DisableTongueCollider()
    {
        if (tongueCollider != null)
        {
            tongueCollider.enabled = false;
        }
    }
}