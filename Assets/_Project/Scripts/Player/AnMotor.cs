using UnityEngine;

public class AnMotor : MonoBehaviour
{
    private readonly RaycastHit[] castBuffer = new RaycastHit[12];

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private AnStanceController stanceController;
    private Transform cameraTransform;
    private LayerMask collisionMask;
    private float airControl;
    private float acceleration;
    private float deceleration;
    private float rotationSpeed;
    private float collisionSkin;
    private float groundNormalThreshold;

    public Vector3 InputDirection { get; private set; }
    public bool IsMoving { get; private set; }

    public void Initialize(
        Rigidbody rb,
        CapsuleCollider capsule,
        AnStanceController stanceController,
        Transform cameraTransform,
        LayerMask collisionMask,
        float airControl,
        float acceleration,
        float deceleration,
        float rotationSpeed,
        float collisionSkin,
        float groundNormalThreshold)
    {
        this.rb = rb;
        this.capsule = capsule;
        this.stanceController = stanceController;
        this.cameraTransform = cameraTransform;
        this.collisionMask = collisionMask;
        this.airControl = airControl;
        this.acceleration = acceleration;
        this.deceleration = deceleration;
        this.rotationSpeed = rotationSpeed;
        this.collisionSkin = collisionSkin;
        this.groundNormalThreshold = groundNormalThreshold;
    }

    public void Move(Vector2 moveInput, float speed, bool isGrounded)
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        InputDirection = camForward * moveInput.y + camRight * moveInput.x;

        if (InputDirection.sqrMagnitude > 1f)
        {
            InputDirection = InputDirection.normalized;
        }

        float targetSpeed = speed * (isGrounded ? 1f : airControl);
        Vector3 targetVelocity = ResolveHorizontalVelocity(InputDirection, targetSpeed);

        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        float rate = targetVelocity.sqrMagnitude > horizontal.sqrMagnitude ? acceleration : deceleration;
        Vector3 next = Vector3.MoveTowards(horizontal, targetVelocity, rate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(next.x, velocity.y, next.z);
        IsMoving = next.sqrMagnitude > 0.01f;
    }

    public void Rotate()
    {
        if (InputDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion target = Quaternion.LookRotation(InputDirection, Vector3.up);
        Quaternion next = Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(next);
    }

    public void Jump(float jumpVelocity)
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpVelocity;
        rb.linearVelocity = velocity;
    }

    private Vector3 ResolveHorizontalVelocity(Vector3 wishDirection, float speed)
    {
        if (wishDirection.sqrMagnitude < 0.001f || speed <= 0f)
        {
            return Vector3.zero;
        }

        Vector3 direction = wishDirection.normalized;
        float distance = speed * Time.fixedDeltaTime + collisionSkin;

        if (TryGetWallHit(direction, distance, out RaycastHit hit))
        {
            Vector3 slide = Vector3.ProjectOnPlane(direction, hit.normal);
            slide.y = 0f;

            if (slide.sqrMagnitude < 0.001f || TryGetWallHit(slide.normalized, distance, out _))
            {
                return Vector3.zero;
            }

            direction = slide.normalized;
        }

        return direction * speed;
    }

    private bool TryGetWallHit(Vector3 direction, float distance, out RaycastHit nearest)
    {
        nearest = default;

        stanceController.GetWorldCapsule(out Vector3 top, out Vector3 bottom, out float radius);
        int count = Physics.CapsuleCastNonAlloc(
            top,
            bottom,
            radius,
            direction,
            castBuffer,
            distance,
            collisionMask,
            QueryTriggerInteraction.Ignore);

        float nearestDistance = float.PositiveInfinity;
        bool found = false;

        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = castBuffer[i];
            Collider col = hit.collider;

            if (col == null || col == capsule || col.attachedRigidbody == rb)
            {
                continue;
            }

            if (hit.normal.y > groundNormalThreshold)
            {
                continue;
            }

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearest = hit;
                found = true;
            }
        }

        return found;
    }
}
