using UnityEngine;

[CreateAssetMenu(menuName = "Decisions/IsBurstComplete")]
public class IsBurstComplete_Decision : Decision
{
    public override bool Decide(StateController controller)
    {
        // Find Shoot_Action on current state and check if burst is complete
        Shoot_Action shootAction = null;
        foreach (var action in controller.currentState.actions)
        {
            if (action is Shoot_Action shoot)
            {
                shootAction = shoot;
                break;
            }
        }

        return shootAction != null && shootAction.IsBurstComplete();
    }
}