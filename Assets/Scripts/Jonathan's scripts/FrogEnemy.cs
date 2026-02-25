using System.Collections;
using UnityEngine;

public class FrogEnemy : MonoBehaviour
{
    public EnemyVFX enemyVFX;


    public float emergeRadius = 10f;
    public float hideRadius = 15f;
    public float hideDelay = 3f;
    public float attackCooldown = 2f;
    public float stunDuration = 1.0f;

    public float tongueAttackRange = 5f;
    public float tongueHoldDuration = 1.0f;
    public float rotationSpeed = 5f;
    public LayerMask playerLayer;

    public GameObject fliesPrefab;
    public Transform mouthSpawnPoint;
    public Transform tongueTipBone;

    public Collider tongueCollider;
    public Collider mainCollider;
    public Animator frogAnimator;

    private Transform playerTarget;
    private bool isEmerged = false;
    private bool isFullyEmerged = false;
    private bool isDead = false;
    private bool isStunned = false;
    private bool isAttacking = false;
    private float timeOutOfRange = 0f;
    private float lastAttackTime = 0f;
    private BoxCollider dynamicTongueCollider;
    private PlayerMovementScript playerMovement;
    private void Start()
    {
        isEmerged = false;
        isFullyEmerged = false;
        isAttacking = false;
        timeOutOfRange = 0f;

        if (tongueCollider != null)
        {
            dynamicTongueCollider = tongueCollider as BoxCollider;
            tongueCollider.enabled = false;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }

        if (frogAnimator != null)
        {
            frogAnimator.Play("Emerge", 0, 0f);
            frogAnimator.speed = 0f;
            frogAnimator.SetBool("IsEmerged", false);
        }
    }
    private void Update()
    {
        if (isDead || isStunned) return;

        if (playerTarget == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, emergeRadius, playerLayer);
            if (hits.Length > 0)
            {
                playerTarget = hits[0].transform;
                playerMovement = playerTarget.GetComponent<PlayerMovementScript>();
            }
            else
            {
                return;
            }
        }

        if (playerMovement != null && playerMovement.IsDead)
        {
            if (isEmerged && !isAttacking)
            {
                isEmerged = false;
                isFullyEmerged = false;
                playerTarget = null;

                if (frogAnimator != null)
                {
                    frogAnimator.speed = 1f;
                    frogAnimator.SetBool("IsEmerged", false);
                }

                if (mainCollider != null)
                {
                    mainCollider.enabled = false;
                }

                DisableTongueCollider();
                StopAllCoroutines();
                StartCoroutine(HideRoutine());
            }
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (!isEmerged)
        {
            if (distanceToPlayer <= emergeRadius)
            {
                isEmerged = true;
                timeOutOfRange = 0f;
                if (frogAnimator != null)
                {
                    frogAnimator.speed = 1f;
                    frogAnimator.SetBool("IsEmerged", true);
                }
                StopAllCoroutines();
                StartCoroutine(EmergeRoutine());
            }
        }
        else
        {
            if (distanceToPlayer > hideRadius)
            {
                if (!isAttacking)
                {
                    timeOutOfRange += Time.deltaTime;
                    if (timeOutOfRange >= hideDelay)
                    {
                        isEmerged = false;
                        isFullyEmerged = false;
                        playerTarget = null;
                        if (frogAnimator != null)
                        {
                            frogAnimator.speed = 1f;
                            frogAnimator.SetBool("IsEmerged", false);
                        }
                        if (mainCollider != null)
                        {
                            mainCollider.enabled = false;
                        }
                        DisableTongueCollider();
                        StopAllCoroutines();
                        StartCoroutine(HideRoutine());
                    }
                }
            }
            else if (isFullyEmerged)
            {
                timeOutOfRange = 0f;

                Vector3 lookDirection = playerTarget.position - transform.position;
                lookDirection.y = 0f;
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), rotationSpeed * Time.deltaTime);
                }

                if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
                {
                    PerformDistanceAttack();
                    lastAttackTime = Time.time;
                }
            }
        }
    }
    private IEnumerator EmergeRoutine()
    {
        yield return null;

        while (frogAnimator != null && frogAnimator.GetCurrentAnimatorStateInfo(0).IsName("Emerge") && frogAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        isFullyEmerged = true;

        if (mainCollider != null)
        {
            mainCollider.enabled = true;
        }
    }

    private IEnumerator HideRoutine()
    {
        yield return null;

        while (frogAnimator != null && frogAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hide") && frogAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);

        if (frogAnimator != null)
        {
            frogAnimator.Play("Emerge", 0, 0f);
            frogAnimator.speed = 0f;
        }
    }

    private IEnumerator StunRoutine()
    {
        isStunned = true;
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
    }
    private void PerformDistanceAttack()
    {
        isAttacking = true;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= tongueAttackRange)
        {
            if (frogAnimator != null) frogAnimator.SetTrigger("TongueAttack");
            StartCoroutine(DynamicTongueColliderRoutine());
        }
        else
        {
            if (frogAnimator != null) frogAnimator.SetTrigger("SummonFlies");
        }
    }

    private IEnumerator DynamicTongueColliderRoutine()
    {
        EnableTongueCollider();

        bool impactPlayed = false;

        while (isAttacking)
        {
            if (tongueTipBone != null && mouthSpawnPoint != null && dynamicTongueCollider != null)
            {
                Vector3 direction = tongueTipBone.position - mouthSpawnPoint.position;
                float distance = direction.magnitude;

                dynamicTongueCollider.transform.position = mouthSpawnPoint.position + (direction * 0.5f);

                if (direction != Vector3.zero)
                    dynamicTongueCollider.transform.rotation = Quaternion.LookRotation(direction);

                Vector3 currentSize = dynamicTongueCollider.size;
                currentSize.z = distance *3f;
                dynamicTongueCollider.size = currentSize;

                if (!impactPlayed)
                {
                    // Only raycast against player layer
                    if (Physics.Raycast(mouthSpawnPoint.position, direction.normalized, out RaycastHit hit, distance, playerLayer))
                    {
                        impactPlayed = true;
                        if (enemyVFX != null)
                            enemyVFX.PlayAttackImpact(hit.point); // VFX now only plays when hitting the player
                    }
                }
            }
            yield return null;
        }

        DisableTongueCollider();
    }

    public void OnTongueFullyExtended()
    {
        StopCoroutine(nameof(HoldTongueRoutine));



        StartCoroutine(nameof(HoldTongueRoutine));
    }

    private IEnumerator HoldTongueRoutine()
    {
        if (frogAnimator != null)
        {
            frogAnimator.speed = 0f;
        }

        yield return new WaitForSeconds(tongueHoldDuration);

        if (frogAnimator != null)
        {
            frogAnimator.speed = 1f;
        }
    }

    public void OnMouthFullyOpen()
    {
        if (fliesPrefab != null && mouthSpawnPoint != null)
        {
            GameObject flies = Instantiate(fliesPrefab, mouthSpawnPoint.position, Quaternion.identity);
            FliesEnemy fliesScript = flies.GetComponent<FliesEnemy>();
            if (fliesScript != null && playerTarget != null)
            {
                fliesScript.SetTarget(playerTarget);
            }
        }
    }

    public void ResetAttackState()
    {
        isAttacking = false;
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
    public void OnEnemyHit()
    {
        if (isDead || !isFullyEmerged)
        {
            return;
        }

        isAttacking = false;

        if (frogAnimator != null)
        {
            frogAnimator.speed = 1f;
            frogAnimator.SetTrigger("GotHit");
        }

        DisableTongueCollider();
        StopAllCoroutines();
        StartCoroutine(StunRoutine());

        if (enemyVFX != null)
            enemyVFX.PlayHitEffect();
    }
    public void OnEnemyDeath()
    {
        if (isDead) return;

        isDead = true;
        StopAllCoroutines();

        if (frogAnimator != null)
        {
            frogAnimator.speed = 1f;
            frogAnimator.SetTrigger("Death");
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }

        if (tongueCollider != null)
        {
            tongueCollider.enabled = false;
        }

        if (enemyVFX != null) enemyVFX.StopAura();
        if (enemyVFX != null) enemyVFX.PlayDeathEffect();


        EnemyDissolve dissolve = GetComponent<EnemyDissolve>();
        if (dissolve != null)
        {
            if (dissolve.enemyVFX == null) dissolve.enemyVFX = enemyVFX;
            dissolve.StartDissolve();
        }
        else
        {
            Destroy(gameObject, 3f);
        }
    }
}