using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Shoot")]
public class Shoot_Action : Action
{
    private readonly int shootAnimHash = Animator.StringToHash("Shoot");

    [Header("Shoot Settings")]
    public GameObject projectilePrefab;
    public float shootCastDuration = 0.5f; // how long the cast plays before firing

    private bool hasFired = false;

    public override void OnEnter(StateController controller)
    {
        hasFired = false;

        if (controller.animator != null)
            controller.animator.CrossFadeInFixedTime(shootAnimHash, 0.1f);
    }

    public override void Act(StateController controller)
    {
        if (controller.chaseTarget == null) return;

        // Wait for cast duration before firing
        if (!hasFired && controller.stateTimeElapsed >= shootCastDuration)
        {
            Fire(controller);
            hasFired = true;
        }
    }

    private void Fire(StateController controller)
    {
        if (projectilePrefab == null || controller.projectileSpawnPoint == null) return;

        // Spawn projectile at spawn point
        GameObject proj = Instantiate(
            projectilePrefab,
            controller.projectileSpawnPoint.position,
            controller.projectileSpawnPoint.rotation
        );

        // Aim at player
        Vector3 dir = controller.chaseTarget.position - controller.projectileSpawnPoint.position;
        SpiderProjectile projectile = proj.GetComponent<SpiderProjectile>();
        if (projectile != null)
            projectile.Init(dir);
    }

    public override void OnExit(StateController controller)
    {
        hasFired = false;
    }
}