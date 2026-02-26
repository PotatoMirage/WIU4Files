using UnityEngine;

[CreateAssetMenu(menuName = "Decisions/CheckGap")]
public class Check_Gap_Decision : Decision
{
    [Tooltip("Check True to trigger jump. Check False to detect landing.")]
    public bool isStartingJump = true;

    public override bool Decide(StateController controller)
    {
        if (isStartingJump)
        {
            return controller.navMeshAgent.isOnOffMeshLink;
        }
        else
        {
            // Only check for landing AFTER some time has passed
            // prevents instant exit before the arc even starts
            return !controller.navMeshAgent.isOnOffMeshLink
                   && controller.stateTimeElapsed > 0.1f;
        }
    }
}