using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WolfAttackScript : MonoBehaviour
{
    [Header("Wolf Attack Settings")]
    public Animator animator;
    public GameManager gameManager;
    public PlayerMovementScript playerMovement;
    public NavMeshAgent navAgent;
    public Rigidbody rigidBody;
    public GameObject leftHandCollider;
    public GameObject rightHandCollider;
    public GameObject jumpSmashVFXPrefab;
    public GameObject clawAttackVFXPrefab;
    public GameObject shadowBallPrefab;
    public GameObject shadowBallCastPrefab;
    public Transform shadowBallFirePoint;
    public Transform clawSpawnPoint;
    public Vector3 spawnPosition;
    public float attackCooldown = 2.5f;
    public float jumpSmashRange = 4.0f;
    public float jumpSmashDamage = 60.0f;
    public float retreatRadius = 6.0f;

    private bool isAttacking, canAttack = true, isRetreating;
    private int currentPhase = 1;

    // Awake is called when loading an instance of a script component
    void Awake()
    {
        animator = GetComponent<Animator>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftHandCollider.SetActive(false);
        rightHandCollider.SetActive(false);
    }

    void FacePlayer()
    {
        Vector3 faceDirection = playerMovement.transform.position - transform.position;
        faceDirection.y = 0;

        if (faceDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(faceDirection);
    }

    public void TryMeleeAttack()
    {
        if (isAttacking || !canAttack)
            return;

        if (currentPhase == 1)
        {
            // Wolf Boss Phase 1 Attack pattern
            if (Random.value < 0.5f)
                StartCoroutine(SwingAttack());
            else
                StartCoroutine(SmashAttack());
        }
        else
        {
            // Wolf Boss Phase 2 Attack pattern
            float attackPatternRNG = Random.value;
            if (attackPatternRNG < 0.25f)
                StartCoroutine(SwingAttack());
            else if (attackPatternRNG < 0.5f)
                StartCoroutine(SmashAttack());
            else if (attackPatternRNG < 0.75f)
                StartCoroutine(JumpSmashAttack());
            else if (attackPatternRNG < 0.875f)
                StartCoroutine(RetreatAttack());
            else
                StartCoroutine(RangedProjectileAttack());
        }
    }

    public void TryRangedAttack()
    {
        if (isAttacking || !canAttack)
            return;

        float attackPatternRNG = Random.value;
        if (attackPatternRNG < 0.7f)
            StartCoroutine(JumpSmashAttack());
        else if (attackPatternRNG < 0.9f)
            StartCoroutine(RangedProjectileAttack());
        else
            StartCoroutine(RetreatAttack());
    }

    public void TriggerLevitatingStun()
    {
        if (!isRetreating)
            return;

        StopAllCoroutines();
        isRetreating = false;
        isAttacking = false;
        canAttack = true;
    }

    public void ForceStopAttack()
    {
        StopAllCoroutines();
        leftHandCollider.SetActive(false);
        rightHandCollider.SetActive(false);
        navAgent.enabled = true;
        isAttacking = false;
        canAttack = true;
    }

    public void EnterSecondPhase()
    {
        currentPhase = 2;
        attackCooldown = 1.5f;
    }

    void SpawnAttackVFX(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null)
            return;

        GameObject vfx = Instantiate(prefab, pos, rot);
        Destroy(vfx, vfx.GetComponent<ParticleSystem>().main.duration);
    }

    IEnumerator SwingAttack()
    {
        isAttacking = true;
        canAttack = false;

        bool isLeft = Random.value < 0.5f;
        animator.CrossFadeInFixedTime(isLeft ? "BigBadWolf_AttackLeft" : "BigBadWolf_Attack", 0.25f);

        if (Random.value < 0.5f)
            gameManager.PlaySFX(Random.Range(0, 3));

        yield return new WaitForSeconds(0.3f);

        if (isLeft)
        {
            SpawnAttackVFX(clawAttackVFXPrefab, clawSpawnPoint.position, transform.rotation * Quaternion.Euler(0.0f, 0.0f, 0.0f));
            leftHandCollider.SetActive(true);
        }
        else
        {
            SpawnAttackVFX(clawAttackVFXPrefab, clawSpawnPoint.position, transform.rotation * Quaternion.Euler(0.0f, 0.0f, 180.0f));
            rightHandCollider.SetActive(true);
        }
        gameManager.PlaySFX(Random.value < 0.5f ? 10 : 11);

        yield return new WaitForSeconds(0.7f);

        leftHandCollider.SetActive(false);
        rightHandCollider.SetActive(false);
        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    IEnumerator SmashAttack()
    {
        isAttacking = true;
        canAttack = false;
        animator.CrossFadeInFixedTime("BigBadWolf_Smash", 0.25f);
        if (Random.value < 0.5f) gameManager.PlaySFX(Random.Range(0, 3));

        yield return new WaitForSeconds(0.5f);

        SpawnAttackVFX(clawAttackVFXPrefab, clawSpawnPoint.position, transform.rotation * Quaternion.Euler(0.0f, 0.0f, -90.0f));
        leftHandCollider.SetActive(true);
        rightHandCollider.SetActive(true);
        gameManager.PlaySFX(Random.value < 0.5f ? 10 : 11);

        yield return new WaitForSeconds(1.5f);

        leftHandCollider.SetActive(false);
        rightHandCollider.SetActive(false);
        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    IEnumerator JumpSmashAttack()
    {
        isAttacking = true;
        canAttack = false;
        animator.CrossFadeInFixedTime("BigBadWolf_JumpSmash", 0.1f);

        Vector3 startPos = transform.position;

        yield return new WaitForSeconds(0.6f);

        // Lock the target position just before it jumps so the player has time to react
        Vector3 targetPos = new(playerMovement.transform.position.x, startPos.y, playerMovement.transform.position.z);
        Vector3 peakPos = Vector3.Lerp(startPos, targetPos, 0.5f) + Vector3.up * 4.0f;

        // Disable the NavMeshAgent so it can freely move during the jump arc
        navAgent.enabled = false;
        rigidBody.isKinematic = true;
        gameManager.PlaySFX(8);

        // Arc Motion
        float elapsed = 0.0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / 0.5f;
            Vector3 startToPeak = Vector3.Lerp(startPos, peakPos, progress);
            Vector3 peakToTarget = Vector3.Lerp(peakPos, targetPos, progress);
            transform.position = Vector3.Lerp(startToPeak, peakToTarget, progress);

            yield return null;
        }

        // Spawn Slam VFX
        transform.position = targetPos;
        gameManager.PlaySFX(9);
        SpawnAttackVFX(clawAttackVFXPrefab, targetPos, transform.rotation * Quaternion.Euler(0.0f, 0.0f, -90.0f));

        if (jumpSmashVFXPrefab != null)
        {
            GameObject vfx = Instantiate(jumpSmashVFXPrefab, targetPos, Quaternion.identity);
            Destroy(vfx, vfx.GetComponent<ParticleSystem>().main.duration);
        }

        if (Vector3.Distance(transform.position, playerMovement.transform.position) <= jumpSmashRange
            && !playerMovement.IsRolling && !playerMovement.IsDead)
        {
            playerMovement.ChangeHealth(-(int)jumpSmashDamage);
            Debug.Log("JumpSmash has dealt " + jumpSmashDamage + " damage to player");
        }

        navAgent.enabled = true;
        navAgent.Warp(targetPos);

        yield return new WaitForSeconds(1.4f);

        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    IEnumerator RetreatAttack()
    {
        isAttacking = true;
        isRetreating = true;
        canAttack = false;
        animator.CrossFadeInFixedTime("BigBadWolf_IdleAir", 0.25f);

        navAgent.enabled = false;
        rigidBody.isKinematic = true;

        Vector3 startPos = transform.position;
        Vector3 hoverPos = startPos + Vector3.up * 3.0f;

        // Pick a landing point on the spawn radius away from the player
        Vector3 awayDir = spawnPosition - playerMovement.transform.position;
        awayDir.y = 0;
        awayDir = awayDir.magnitude > 0.1f ? awayDir.normalized : -transform.forward;
        Vector3 landPos = spawnPosition + awayDir * retreatRadius;

        // Clamp to the NavMesh so the wolf doesn't land on an invalid position
        if (NavMesh.SamplePosition(landPos, out NavMeshHit hit, retreatRadius, NavMesh.AllAreas))
            landPos = hit.position;

        Vector3 hoverLandPos = landPos + Vector3.up * 3.0f;

        // Levitate up aniamtion
        float elapsed = 0.0f;
        while (elapsed < 1.5f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, hoverPos, elapsed / 1.5f);
            FacePlayer();
            yield return null;
        }

        // Hover animation
        elapsed = 0.0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            FacePlayer();
            yield return null;
        }

        // Glide towards the 'hoverLandPos' animation
        elapsed = 0.0f;
        while (elapsed < 1.0f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(hoverPos, hoverLandPos, elapsed / 1.0f);
            FacePlayer();
            yield return null;
        }

        // Hover animation
        elapsed = 0.0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            FacePlayer();
            yield return null;
        }

        // Descend animation
        elapsed = 0.0f;
        while (elapsed < 1.0f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(hoverLandPos, landPos, elapsed / 1.0f);
            FacePlayer();
            yield return null;
        }

        transform.position = landPos;
        navAgent.enabled = true;
        navAgent.Warp(landPos);
        isRetreating = false;
        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    IEnumerator RangedProjectileAttack()
    {
        isAttacking = true;
        canAttack = false;
        animator.CrossFadeInFixedTime("BigBadWolf_ChargeAnim", 0.25f);

        yield return new WaitForSeconds(1.0f);

        gameManager.PlaySFX(4);
        animator.CrossFadeInFixedTime("BigBadWolf_Shoot", 0.1f);

        for (int i = 0; i < 3; i++)
        {
            if (shadowBallFirePoint != null)
            {
                Vector3 dir = (playerMovement.transform.position - shadowBallFirePoint.position).normalized;

                if (shadowBallPrefab != null)
                    Instantiate(shadowBallPrefab, shadowBallFirePoint.position, Quaternion.LookRotation(dir));

                if (shadowBallCastPrefab != null)
                {
                    GameObject castVFX = Instantiate(shadowBallCastPrefab, shadowBallFirePoint.position, shadowBallFirePoint.rotation);
                    gameManager.PlaySFX(7);
                    Destroy(castVFX, castVFX.GetComponent<ParticleSystem>().main.duration);
                }
            }

            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(0.5f);

        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void OnDrawGizmos()
    {
        if (leftHandCollider != null)
        {
            Gizmos.color = leftHandCollider.activeSelf ? Color.red : Color.green;
            Gizmos.DrawWireSphere(leftHandCollider.transform.position, 0.4f);
        }

        if (rightHandCollider != null)
        {
            Gizmos.color = rightHandCollider.activeSelf ? Color.red : Color.green;
            Gizmos.DrawWireSphere(rightHandCollider.transform.position, 0.4f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, jumpSmashRange);
    }

    public void SetSpawnPosition(Vector3 pos) => spawnPosition = pos;
    public bool IsAttacking => isAttacking;
    public bool CanAttack => canAttack;
    public bool IsRetreating => isRetreating;
    public float AttackCooldown { set => attackCooldown = value; }
}