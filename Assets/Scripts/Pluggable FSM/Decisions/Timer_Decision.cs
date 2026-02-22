using UnityEngine;

[CreateAssetMenu(menuName = "Decisions/Timer")]
public class Timer_Decision : Decision
{
    [Tooltip("How many seconds to wait before returning true.")]
    public float waitTime = 1f;

    public override bool Decide(StateController controller)
    {
        // Returns true only after the controller has been in the current state for 'waitTime' seconds
        return controller.CheckIfCountDownElapsed(waitTime);
    }
}