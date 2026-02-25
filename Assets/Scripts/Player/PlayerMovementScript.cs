using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerMovementScript : MonoBehaviour
{
    [Header("Player Movement Settings")]
    public PlayerInput playerInput;
    public Animator animator;
    public CharacterController controller;
    public PlayerAttackScript playerAttack;
    public Transform cameraTransform;
    public int health = 100;
    public int maxHealth = 100;
    public float walkSpeed = 2.5f;
    public float crouchSpeed = 1.25f;
    public float rollSpeed = 3.5f;
    public float meleeDashSpeed = 8.0f;
    public float rotationSpeed = 10.0f;
    public float jumpHeight = 2.5f;
    public float hardLandFallDistance = 1.5f;
    public float hardLandVelocityThreshold = -6.0f;
    public float hardLandDuration = 1.25f;
    public float gravity = -19.62f;

    [Header("Collision Layer Settings")]
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("Character Controller Settings")]
    public float standingHeight = 1.3f;
    public float standingCenterY = -0.055f;
    public float crouchingHeight = 0.75f;
    public float crouchingCenterY = -0.33f;
    public float jumpingCenterY = 0.15f;

    private InputAction moveAction, crouchAction, jumpAction, rollAction;
    private Vector2 moveInput;
    private bool isGrounded, isCrouching, isReadyJump, isJumping, isRolling, isFalling, isLanding, isDead, isHitStunned, wasMeleeAttacking;
    private float verticalVelocity, jumpSpeedMultiplier = 1.0f, gravityMultiplier = 1.0f, fallStartY, meleeDashTimer, hitStunCooldownTimer;
    private string currentAnimation;


    [Header("Debuff Effects")]
    public GameObject debuffVFXPrefab;

    private float debuffTimer = 0f;
    private float debuffSpeedMultiplier = 1f;
    private float baseWalkSpeed;
    private float baseCrouchSpeed;
    private float baseRollSpeed;
    private Coroutine debuffUICoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = playerInput.actions["Move"];
        crouchAction = playerInput.actions["Crouch"];
        jumpAction = playerInput.actions["Jump"];
        rollAction = playerInput.actions["Roll"];

        baseWalkSpeed = walkSpeed;
        baseCrouchSpeed = crouchSpeed;
        baseRollSpeed = rollSpeed;

        currentAnimation = "Player_Idle";
        animator.Play("Player_Idle");
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundLayer);

        // If the player is dead, rolling or landing, ignores every input
        if (isDead || isRolling || isLanding || isHitStunned)
        {
            if (isDead && !isGrounded) // Still affected by gravity though (as you should)
                verticalVelocity += gravity * Time.deltaTime;
            else
                verticalVelocity = -2.0f;

            controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
            return;
        }

        moveInput = moveAction.ReadValue<Vector2>();

        // Detect when melee finishes and reset so PlayAnimation forces a transition back
        if (!playerAttack.IsMeleeAttacking && wasMeleeAttacking)
            currentAnimation = "";

        wasMeleeAttacking = playerAttack.IsMeleeAttacking;

        if (meleeDashTimer > 0)
            meleeDashTimer -= Time.deltaTime;

        if (hitStunCooldownTimer > 0)
            hitStunCooldownTimer -= Time.deltaTime;

        // Adjust CharacterController's height depending if the player is crouching or not (aiming ignored)
        if (crouchAction.WasPressedThisFrame() && isGrounded && !playerAttack.IsAiming && !playerAttack.IsMeleeAttacking)
        {
            if (isCrouching && CheckForBlockedOverhead())
                return;

            isCrouching = !isCrouching;
            controller.height = isCrouching ? crouchingHeight : standingHeight;
            controller.center = new Vector3(0, isCrouching ? crouchingCenterY : standingCenterY, 0);
        }

        if (isGrounded && !isJumping && !isReadyJump)
        {
            jumpSpeedMultiplier = 1.0f;
            gravityMultiplier = 1.0f;
        }

        // Records the player's Y position when falling starts (for landing animation purposes)
        if (!isGrounded && verticalVelocity < 0 && !isFalling)
        {
            fallStartY = transform.position.y;
            isFalling = true;
        }

        // Trigger the landing animation if the fall distance or velocity exceeds thresholds
        if (isFalling && isGrounded)
        {
            isFalling = false;
            float fallDistance = fallStartY - transform.position.y;

            if (fallDistance >= hardLandFallDistance || verticalVelocity <= hardLandVelocityThreshold)
                StartCoroutine(LandingWithDelay());
        }

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2.0f;
            isJumping = false;
        }
        else
        {
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }

        if (isJumping)
            controller.center = new Vector3(0, verticalVelocity > 0 ? jumpingCenterY : standingCenterY, 0);

        // Prevents jump and roll inputs while aiming or melee attacking
        if (!playerAttack.IsAiming && !playerAttack.IsMeleeAttacking)
        {
            if (jumpAction.WasPressedThisFrame() && isGrounded && !isCrouching && !isReadyJump && !isJumping)
            {
                if (moveInput.magnitude > 0) // Player is moving while jump command is triggered
                {
                    isJumping = true;
                    jumpSpeedMultiplier = 2.0f;
                    gravityMultiplier = 0.5f;
                    PlayAnimation("Player_JumpMovement");
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2.0f * gravity) * 0.6f;
                }
                else // Player is idle and the jump command is triggered
                {
                    StartCoroutine(JumpWithStartDelay());
                }
            }

            // Player Rolling System (Cardinal Directions)
            if (rollAction.WasPressedThisFrame() && isGrounded && moveInput.magnitude > 0 && !isReadyJump && !isJumping)
            {
                string rollAnim = "Player_Roll";
                Vector3 rollDir = transform.forward;

                if (moveInput.x < 0)
                {
                    rollAnim = "Player_RollLeft";
                    rollDir = -transform.right;
                }
                else if (moveInput.x > 0)
                {
                    rollAnim = "Player_RollRight";
                    rollDir = transform.right;
                }
                else if (moveInput.y < 0)
                {
                    rollAnim = "Player_RollBack";
                    rollDir = -transform.forward;
                }

                StartCoroutine(RollWithStartDelay(rollAnim, rollDir));
            }
        }

        // Blocks movement animations while melee attacking or hit stunned
        if (isGrounded && !isJumping && !isReadyJump && !isRolling && !isLanding && !playerAttack.IsMeleeAttacking && !isHitStunned)
        {
            Vector2 animInput = playerAttack.IsAiming ? new Vector2(moveInput.y, moveInput.x) : moveInput;
            PlayAnimation(GetPlayerAnimationState(animInput));
        }

        Vector3 cameraForward = cameraTransform.forward; cameraForward.y = 0; cameraForward.Normalize();
        Vector3 cameraRight = cameraTransform.right; cameraRight.y = 0; cameraRight.Normalize();
        Vector3 moveDirection;

        // Dash forward during melee attack, otherwise use normal movement input
        if (playerAttack.IsMeleeAttacking)
        {
            moveDirection = meleeDashTimer > 0 ? transform.forward * meleeDashSpeed : Vector3.zero;
        }
        else
        {
            float speed = isCrouching ? crouchSpeed : walkSpeed * jumpSpeedMultiplier;
            moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized * speed;
        }

        // Rotate player to face camera forward when moving or aiming
        if ((moveDirection.magnitude > 0.1f || playerAttack.IsAiming) && !playerAttack.IsMeleeAttacking)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        moveDirection.y = verticalVelocity;
        controller.Move(moveDirection * Time.deltaTime);


        if (debuffTimer > 0)
        {
            debuffTimer -= Time.deltaTime;

            // Apply multiplier against BASE speeds, not current speeds
            walkSpeed = baseWalkSpeed * debuffSpeedMultiplier;
            crouchSpeed = baseCrouchSpeed * debuffSpeedMultiplier;
            rollSpeed = baseRollSpeed * debuffSpeedMultiplier;
        }
        else
        {
            // Always restore base speeds when debuff expires
            walkSpeed = baseWalkSpeed;
            crouchSpeed = baseCrouchSpeed;
            rollSpeed = baseRollSpeed;
            debuffSpeedMultiplier = 1f;
        }
    }

    public void SetHealth(int amount)
    {
        int previous = health;
        health = Mathf.Clamp(amount, 0, maxHealth);

        if (health < previous && health > 0 && hitStunCooldownTimer <= 0)
            StartCoroutine(HitStunAnimation());

        if (health == 0)
            TriggerDeath();
    }

    public bool UnCrouchPlayer()
    {
        if (!isCrouching)
            return true;

        if (CheckForBlockedOverhead())
            return false;

        isCrouching = false;
        controller.height = standingHeight;
        controller.center = new Vector3(0, standingCenterY, 0);

        return true;
    }

    void TriggerDeath()
    {
        if (!IsDead)
        {
            isDead = true;
            animator.CrossFadeInFixedTime(Random.value < 0.5f ? "Player_DeathOne" : "Player_DeathTwo", 0.25f);
        }
    }

    void PlayAnimation(string animationName)
    {
        if (animationName == currentAnimation)
            return;

        animator.CrossFadeInFixedTime(animationName, 0.25f);
        currentAnimation = animationName;
    }

    string GetPlayerAnimationState(Vector2 input)
    {
        if (isCrouching)
        {
            if (input.x < 0)
                return "Player_CrouchStrafeLeft";
            if (input.x > 0)
                return "Player_CrouchStrafeRight";
            if (input.y > 0)
                return "Player_CrouchMoveForward";
            if (input.y < 0)
                return "Player_CrouchMoveBackward";
            return "Player_CrouchIdle";
        }

        if (input.magnitude == 0)
            return "Player_Idle";

        if (input.y > 0)
        {
            if (input.x < 0)
                return "Player_MoveForward_DiagLeft";
            if (input.x > 0)
                return "Player_MoveForward_DiagRight";
            return "Player_MoveForward";
        }
        if (input.y < 0)
        {
            if (input.x < 0)
                return "Player_MoveBackward_DiagLeft";
            if (input.x > 0)
                return "Player_MoveBackward_DiagRight";
            return "Player_MoveBackward";
        }

        return input.x < 0 ? "Player_MoveStrafeLeft" : "Player_MoveStrafeRight";
    }

    bool CheckForBlockedOverhead()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.05f;
        float castDistance = standingCenterY + standingHeight / 2.0f;

        return Physics.Raycast(rayOrigin, Vector3.up, castDistance, groundLayer);
    }

    System.Collections.IEnumerator JumpWithStartDelay()
    {
        isReadyJump = true;
        jumpSpeedMultiplier = 0.0f;
        PlayAnimation("Player_JumpIdle");

        yield return new WaitForSeconds(0.4f);

        isReadyJump = false;
        isJumping = true;
        jumpSpeedMultiplier = 2.0f;
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
    }

    System.Collections.IEnumerator RollWithStartDelay(string rollAnim, Vector3 rollDir)
    {
        isRolling = true;
        PlayAnimation(rollAnim);

        yield return new WaitForSeconds(0.1f);

        controller.height = crouchingHeight;
        controller.center = new Vector3(0, crouchingCenterY, 0);
        float rollTimer = 0.0f;

        while (rollTimer < 1.0f)
        {
            Vector3 rollMove = rollDir * rollSpeed;
            rollMove.y = -2.0f;
            controller.Move(rollMove * Time.deltaTime);
            rollTimer += Time.deltaTime;

            yield return null;
        }

        if (!isCrouching && CheckForBlockedOverhead())
            isCrouching = true;

        controller.height = isCrouching ? crouchingHeight : standingHeight;
        controller.center = new Vector3(0, isCrouching ? crouchingCenterY : standingCenterY, 0);
        isRolling = false;
    }

    System.Collections.IEnumerator LandingWithDelay()
    {
        isLanding = true;
        PlayAnimation("Player_JumpLand");

        yield return new WaitForSeconds(hardLandDuration);

        isLanding = false;
    }

    System.Collections.IEnumerator HitStunAnimation()
    {
        isHitStunned = true;
        hitStunCooldownTimer = 1.0f;

        animator.CrossFadeInFixedTime(Random.value < 0.5f ? "Player_AttackedOne" : "Player_AttackedTwo", 0.1f);
        yield return new WaitForSeconds(0.5f);

        isHitStunned = false;
        currentAnimation = "";
    }

    public void ApplyDebuff(float duration, float speedMultiplier, Sprite icon, string effectName)
    {
        debuffTimer = duration;
        debuffSpeedMultiplier = speedMultiplier;

        if (debuffVFXPrefab != null)
        {
            GameObject vfx = Instantiate(debuffVFXPrefab, transform);
            Destroy(vfx, duration);
        }

        if (StatusEffectUIManager.Instance != null && icon != null)
        {
            StatusEffectUIManager.Instance.AddEffect(effectName, icon, duration, false);
            if (debuffUICoroutine != null)
            {
                StopCoroutine(debuffUICoroutine);
            }
            debuffUICoroutine = StartCoroutine(RemoveDebuffUIRoutine(effectName, duration));
        }
    }
    private System.Collections.IEnumerator RemoveDebuffUIRoutine(string effectName, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (StatusEffectUIManager.Instance != null)
        {
            StatusEffectUIManager.Instance.RemoveEffect(effectName);
        }
    }

    public void ChangeHealth(int amount) => SetHealth(health + amount);
    public void StartMeleeDash() => meleeDashTimer = 0.2f;
    public bool IsRolling => isRolling;
    public bool IsJumping => isJumping;
    public bool IsReadyJump => isReadyJump;
    public bool IsLanding => isLanding;
    public bool IsCrouching => isCrouching;
    public bool IsHitStunned => isHitStunned;
    public bool IsDead => isDead;
}