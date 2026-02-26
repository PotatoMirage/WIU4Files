using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Decisions/IsGrounded")]
public class IsGrounded_Decision : Decision
{
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;

    public override bool Decide(StateController controller)
    {
        // Start raycast from bottom of collider instead of centre
        Collider col = controller.GetComponent<Collider>();
        float colBottom = col != null ? col.bounds.min.y : controller.transform.position.y;

        Vector3 rayOrigin = new Vector3(
            controller.transform.position.x,
            colBottom + 0.1f, // slightly above bottom to avoid starting inside ground
            controller.transform.position.z
        );

        bool grounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayer);
        return grounded;
    }
}