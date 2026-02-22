using UnityEngine;

public class DebugDamage : MonoBehaviour
{
    public StateController target;

    private void Update()
    {
        // Press D to deal damage to spider
        if (Input.GetKeyDown(KeyCode.D))
        {
            target.OnTakeDamage();
        }
    }
}