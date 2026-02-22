using UnityEngine;

public abstract class Action : ScriptableObject
{
    public abstract void Act(StateController controller);
    public virtual void OnEnter(StateController controller) { }
    public virtual void OnExit(StateController controller) { }
}