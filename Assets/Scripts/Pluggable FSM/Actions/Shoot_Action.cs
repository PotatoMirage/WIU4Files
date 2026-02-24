using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Shoot")]
public class Shoot_Action : Action
{
    private readonly int shootAnimHash = Animator.StringToHash("Shoot");

    [Header("Shoot Settings")]
    public GameObject projectilePrefab;
    public float shootCastDuration = 0.5f;
    public int shotsPerBurst = 3;        // how many shots per burst
    public float timeBetweenShots = 0.3f; // time between each shot in burst
    public float cooldownAfterBurst = 1f; // cooldown before going back to aim

    private int shotsFired = 0;
    private float nextShotTime = 0f;
    private bool burstComplete = false;

    public override void OnEnter(StateController controller)
    {
        shotsFired = 0;
        burstComplete = false;
        nextShotTime = controller.stateTimeElapsed + shootCastDuration;

        if (controller.animator != null)
            controller.animator.CrossFadeInFixedTime(shootAnimHash, 0.1f);
    }

    public override void Act(StateController controller)
    {
        if (controller.chaseTarget == null) return;
        if (burstComplete) return;

        // Stop shooting the flipping player if it dies
        PlayerMovementScript playerMovement = controller.chaseTarget.GetComponent<PlayerMovementScript>();
        if (playerMovement != null && playerMovement.IsDead)
            return;

        // Fire shots in burst
        if (shotsFired < shotsPerBurst && controller.stateTimeElapsed >= nextShotTime)
        {
            Fire(controller);
            shotsFired++;
            nextShotTime = controller.stateTimeElapsed + timeBetweenShots;
        }

        // Burst complete, wait for cooldown then return to aim
        if (shotsFired >= shotsPerBurst &&
            controller.stateTimeElapsed >= nextShotTime + cooldownAfterBurst)
        {
            burstComplete = true;
        }
    }

    private void Fire(StateController controller)
    {
        if (projectilePrefab == null || controller.projectileSpawnPoint == null) return;

        Vector3 firePos = controller.lookPoint != null
            ? controller.lookPoint.position
            : controller.projectileSpawnPoint.position;

        // Aim at player's center (add offset if needed for chest height)
        Vector3 targetPos = controller.chaseTarget.position + Vector3.up * 0.2f;
        Vector3 dir = (targetPos - firePos).normalized;

        GameObject proj = Instantiate(projectilePrefab, firePos, Quaternion.LookRotation(dir));
        SpiderProjectile projectile = proj.GetComponent<SpiderProjectile>();
        if (projectile != null)
            projectile.Init(dir, controller.spiderCollider);
    }

    public override void OnExit(StateController controller)
    {
        shotsFired = 0;
        burstComplete = false;
    }

    public bool IsBurstComplete() => burstComplete;
}