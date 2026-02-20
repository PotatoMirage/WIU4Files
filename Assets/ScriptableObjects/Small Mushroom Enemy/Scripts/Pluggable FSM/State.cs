using UnityEngine;

[CreateAssetMenu]
public class State : ScriptableObject
{
    public Action[] actions;
    public Transition[] transitions;

    public void OnEnterState(StateController controller)
    {
        // Initialize Actions
        for (int i = 0; i < actions.Length; i++)
            if (actions[i] != null) actions[i].OnEnter(controller);

        // Initialize Decisions
        for (int i = 0; i < transitions.Length; i++)
            if (transitions[i].decision != null) transitions[i].decision.OnEnter(controller);
    }

    public void UpdateState(StateController controller)
    {
        DoActions(controller);
        CheckTransitions(controller);
    }

    public void OnExitState(StateController controller)
    {
        // Clean up Actions
        for (int i = 0; i < actions.Length; i++)
            if (actions[i] != null) actions[i].OnExit(controller);

        // Clean up Decisions
        for (int i = 0; i < transitions.Length; i++)
            if (transitions[i].decision != null) transitions[i].decision.OnExit(controller);
    }

    private void DoActions(StateController controller)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            if (actions[i] != null)
                actions[i].Act(controller);
        }
    }

    private void CheckTransitions(StateController controller)
    {
        for (int i = 0; i < transitions.Length; i++)
        {
            if (transitions[i].decision == null) continue;

            bool decisionSucceeded = transitions[i].decision.Decide(controller);
            State targetState = decisionSucceeded ? transitions[i].trueState : transitions[i].falseState;

            if (targetState != controller.remainState && targetState != null)
            {
                controller.TransitionToState(targetState);
                break;
            }
        }
    }
}