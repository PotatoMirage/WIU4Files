using UnityEngine;

public abstract class ItemEffect : ScriptableObject
{
    public abstract void Execute(GameObject user);
}