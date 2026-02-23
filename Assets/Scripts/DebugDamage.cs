using UnityEngine;

public class DebugDamage : MonoBehaviour
{
    public StateController target;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            target.OnTakeDamage();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            target.OnDeath();
        }
    }
}