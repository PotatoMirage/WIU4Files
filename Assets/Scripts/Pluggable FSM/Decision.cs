using UnityEngine;

public abstract class Decision : ScriptableObject
{
    public abstract bool Decide(StateController controller);
    public virtual void OnEnter(StateController controller) { }
    public virtual void OnExit(StateController controller) { }
}