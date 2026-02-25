using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackScript : MonoBehaviour
{
    [Header("Player Attack Settings")]
    public PlayerInput playerInput;
    public Animator animator;
    public PlayerMovementScript playerMovement;
    public GameObject slingshotObject;
    public GameObject projectilePrefab;
    public GameObject fireEffectPrefab;
    public GameObject weaponBack;
    public GameObject weaponHand;
    public Transform leftHandPosition;
    public Transform rightHandPosition;
    public Transform projectileSpawnPoint;
    public AudioClip playerSwingSFX;
    public LineRenderer leftLineRenderer;
    public LineRenderer rightLineRenderer;
    public BoxCollider attackCollider;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public float fireCooldown = 0.5f;
    public float fireMaxCharge = 1.0f;
    public float meleeCooldown = 0.5f;

    [Header("Crosshair Settings")]
    public RectTransform crosshairOutline;
    public GameObject aimCrosshair;

    [Header("Attack SFX Settings")]
    public AudioSource audioSource;
    public AudioClip meleeSFX;
    public AudioClip slingshotPullSFX;
    public AudioClip slingshotFireSFX;

    private InputAction leftClickAction, rightClickAction, meleeAction;
    private bool isAllowedADS, isFiringSlingshot, isMeleeAttacking;
    private int upperLayerIndex;
    private float cooldownTimer, chargeTimer, meleeCooldownTimer;
    private string currentUpperAnimation;

    // Awake is called when loading an instance of a script component
    void Awake()
    {
        leftClickAction = playerInput.actions["LeftClick"];
        rightClickAction = playerInput.actions["RightClick"];
        meleeAction = playerInput.actions["Melee"];

        upperLayerIndex = animator.GetLayerIndex("Upper Layer");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slingshotObject.transform.SetParent(leftHandPosition);
        slingshotObject.transform.SetLocalPositionAndRotation(positionOffset, Quaternion.Euler(rotationOffset));
        aimCrosshair.SetActive(false);
        attackCollider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        bool isActionBlocked = playerMovement.IsRolling || playerMovement.IsReadyJump || playerMovement.IsJumping || playerMovement.IsLanding || playerMovement.IsHitStunned || playerMovement.IsDead;

        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        if (meleeCooldownTimer > 0)
            meleeCooldownTimer -= Time.deltaTime;

        if (isActionBlocked)
            isAllowedADS = false;

        // Allow ADS when RMB is pressed only if the player is not blocked
        // If the player is crouched, immediately force uncrouch before aiming
        if (rightClickAction.WasPressedThisFrame() && !isActionBlocked && cooldownTimer <= 0 && Cursor.lockState == CursorLockMode.Locked)
            isAllowedADS = playerMovement.UnCrouchPlayer();

        // Check to see if the player is aiming, if so, begin charging the attack and show the indicator
        bool wasAiming = IsAiming;
        IsAiming = rightClickAction.IsPressed() && !isFiringSlingshot && !isActionBlocked && !isMeleeAttacking && cooldownTimer <= 0 && isAllowedADS && !playerMovement.IsCrouching && Cursor.lockState == CursorLockMode.Locked;
        chargeTimer = IsAiming ? Mathf.Min(chargeTimer + Time.deltaTime, fireMaxCharge) : 0;
        aimCrosshair.SetActive(IsAiming);

        if (IsAiming && !wasAiming)
            audioSource.PlayOneShot(slingshotPullSFX);

        // Shrinks the crosshair outline as the charge increases
        if (IsAiming)
            crosshairOutline.sizeDelta = Vector2.one * Mathf.Lerp(400.0f, 0.0f, chargeTimer / fireMaxCharge);

        slingshotObject.SetActive(IsAiming || isFiringSlingshot);
        weaponBack.SetActive(!isMeleeAttacking);
        weaponHand.SetActive(isMeleeAttacking);

        if (IsAiming)
        {
            leftLineRenderer.SetPosition(1, leftLineRenderer.transform.InverseTransformPoint(rightHandPosition.position));
            rightLineRenderer.SetPosition(1, rightLineRenderer.transform.InverseTransformPoint(rightHandPosition.position));
        }

        // Fires the projectile when LMB is pressed while aiming
        if (leftClickAction.WasPressedThisFrame() && IsAiming)
            StartCoroutine(FireProjectile());

        // Triggers melee attack on "E" press if unblocked and not already attacking
        if (meleeAction.WasPressedThisFrame() && !isActionBlocked && !isMeleeAttacking && meleeCooldownTimer <= 0 && Cursor.lockState == CursorLockMode.Locked)
        {
            if (!playerMovement.UnCrouchPlayer())
                return;

            isAllowedADS = false;
            StartCoroutine(MeleeAttack());
        }

        animator.SetLayerWeight(upperLayerIndex, Mathf.MoveTowards(animator.GetLayerWeight(upperLayerIndex), (IsAiming || isFiringSlingshot) ? 1.0f : 0.0f, Time.deltaTime / 0.25f));

        if (IsAiming)
            PlayUpperAnimation("Player_SlingshotAim");
        else if (isFiringSlingshot)
            PlayUpperAnimation("Player_SlingshotFire");
        else
            currentUpperAnimation = "";
    }

    private void PlayUpperAnimation(string animationName)
    {
        if (animationName == currentUpperAnimation)
            return;

        animator.CrossFadeInFixedTime(animationName, 0.25f, upperLayerIndex);
        currentUpperAnimation = animationName;
    }

    System.Collections.IEnumerator FireProjectile()
    {
        isFiringSlingshot = true;
        audioSource.PlayOneShot(slingshotFireSFX);

        // Instantiate the slingshot fire particle effect at the "projectileSpawnPoint"
        GameObject slingshotEffect = Instantiate(fireEffectPrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation * Quaternion.Euler(-90, 0, 0));
        Destroy(slingshotEffect, slingshotEffect.GetComponent<ParticleSystem>().main.duration);

        Vector3 leftStart = leftLineRenderer.GetPosition(1), rightStart = rightLineRenderer.GetPosition(1);
        float slingshotStringReboundDuration = 0;

        // Instantiate the projectile and fire in the direction the camera is facing
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Camera.main.transform.rotation);
        projectile.GetComponent<SlingshotProjectileScript>().gravityMultiplier = 1.0f - (chargeTimer / fireMaxCharge);

        while (slingshotStringReboundDuration < 0.1f)
        {
            float lerpTimer = slingshotStringReboundDuration / 0.1f;
            leftLineRenderer.SetPosition(1, Vector3.Lerp(leftStart, new Vector3(0.0f, 0.0f, 0.1f), lerpTimer));
            rightLineRenderer.SetPosition(1, Vector3.Lerp(rightStart, new Vector3(0.0f, 0.0f, -0.1f), lerpTimer));
            slingshotStringReboundDuration += Time.deltaTime;
            yield return null;
        }

        leftLineRenderer.SetPosition(1, new Vector3(0.0f, 0.0f, 0.1f));
        rightLineRenderer.SetPosition(1, new Vector3(0.0f, 0.0f, -0.1f));
        yield return new WaitForSeconds(0.4f);

        isAllowedADS = false;
        isFiringSlingshot = false;
        cooldownTimer = fireCooldown;
        currentUpperAnimation = "";
    }

    System.Collections.IEnumerator MeleeAttack()
    {
        isMeleeAttacking = true;
        attackCollider.enabled = true;
        audioSource.PlayOneShot(meleeSFX);
        attackCollider.GetComponent<AttackColliderScript>().ClearHits();

        if (Random.value < 0.5f)
            audioSource.PlayOneShot(playerSwingSFX);

        string meleeAnim = Random.value < 0.5f ? "Player_MeleeSwingOne" : "Player_MeleeSwingTwo";
        animator.CrossFadeInFixedTime(meleeAnim, 0.25f);
        playerMovement.StartMeleeDash();

        yield return new WaitForSeconds(1.0f);

        attackCollider.enabled = false;
        isMeleeAttacking = false;
        meleeCooldownTimer = meleeCooldown;
    }

    public bool IsAiming { get; private set; }
    public bool IsFiring => isFiringSlingshot;
    public bool IsMeleeAttacking => isMeleeAttacking;
}