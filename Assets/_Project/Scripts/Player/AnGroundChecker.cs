using UnityEngine;

public class AnGroundChecker : MonoBehaviour
{
    private Transform groundCheck;
    private float groundCheckRadius;
    private LayerMask groundLayer;
    private CapsuleCollider capsule;

    public void Initialize(Transform groundCheck, float groundCheckRadius, LayerMask groundLayer, CapsuleCollider capsule)
    {
        this.groundCheck = groundCheck;
        this.groundCheckRadius = groundCheckRadius;
        this.groundLayer = groundLayer;
        this.capsule = capsule;
    }

    public bool CheckGrounded()
    {
        if (groundCheck != null)
        {
            return Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                groundLayer,
                QueryTriggerInteraction.Ignore);
        }

        float radius = capsule != null ? Mathf.Max(0.05f, capsule.radius * 0.85f) : 0.2f;
        Vector3 origin = transform.position + Vector3.up * 0.05f;

        return Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out _,
            0.18f,
            groundLayer,
            QueryTriggerInteraction.Ignore);
    }

    public void DrawGizmos()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
