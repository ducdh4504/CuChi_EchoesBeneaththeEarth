using UnityEngine;

public class AnInteractor : MonoBehaviour
{
    private readonly Collider[] overlapBuffer = new Collider[12];
    private readonly RaycastHit[] castBuffer = new RaycastHit[12];

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Transform interactionOrigin;
    private float interactionDistance;
    private float interactionRadius;
    private LayerMask interactionMask;
    private IInteractable currentInteractable;

    public IInteractable CurrentInteractable => currentInteractable;

    public void Initialize(
        Rigidbody rb,
        CapsuleCollider capsule,
        Transform interactionOrigin,
        float interactionDistance,
        float interactionRadius,
        LayerMask interactionMask)
    {
        this.rb = rb;
        this.capsule = capsule;
        this.interactionOrigin = interactionOrigin;
        this.interactionDistance = interactionDistance;
        this.interactionRadius = interactionRadius;
        this.interactionMask = interactionMask;
    }

    public void TryInteract()
    {
        RefreshCurrentInteractable();
        currentInteractable?.Interact();
        RefreshCurrentInteractable();
    }

    public IInteractable GetCurrentInteractable()
    {
        RefreshCurrentInteractable();
        return currentInteractable;
    }

    private void Update()
    {
        RefreshCurrentInteractable();
    }

    private void RefreshCurrentInteractable()
    {
        currentInteractable = FindInteractableTarget();
    }

    private IInteractable FindInteractableTarget()
    {
        Collider target = FindInteractionTarget();
        return target == null ? null : target.GetComponentInParent<IInteractable>();
    }

    private Collider FindInteractionTarget()
    {
        Vector3 origin = GetInteractionOrigin();
        Vector3 direction = transform.forward;
        float radius = Mathf.Max(0.01f, interactionRadius);
        float distance = Mathf.Max(0f, interactionDistance);

        Collider nearest = null;
        float nearestDistance = float.PositiveInfinity;

        int overlapCount = Physics.OverlapSphereNonAlloc(
            origin,
            radius,
            overlapBuffer,
            interactionMask,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < overlapCount; i++)
        {
            Collider hit = overlapBuffer[i];
            if (!CanInteractWith(hit) || !IsInInteractionDirection(hit, origin, direction))
            {
                continue;
            }

            nearest = hit;
            nearestDistance = 0f;
        }

        int castCount = Physics.SphereCastNonAlloc(
            origin,
            radius,
            direction,
            castBuffer,
            distance,
            interactionMask,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < castCount; i++)
        {
            RaycastHit hit = castBuffer[i];
            Collider col = hit.collider;

            if (!CanInteractWith(col) || hit.distance >= nearestDistance)
            {
                continue;
            }

            nearest = col;
            nearestDistance = hit.distance;
        }

        return nearest;
    }

    private Vector3 GetInteractionOrigin()
    {
        if (interactionOrigin != null)
        {
            return interactionOrigin.position;
        }

        if (capsule != null)
        {
            return transform.TransformPoint(capsule.center);
        }

        return transform.position + Vector3.up;
    }

    private bool CanInteractWith(Collider col)
    {
        if (col == null || col == capsule || col.attachedRigidbody == rb)
        {
            return false;
        }

        IInteractable interactable = col.GetComponentInParent<IInteractable>();
        if (interactable == null)
        {
            return false;
        }

        IInteractionAvailability availability = col.GetComponentInParent<IInteractionAvailability>();
        return availability == null || availability.CanInteract();
    }

    private static bool IsInInteractionDirection(Collider col, Vector3 origin, Vector3 direction)
    {
        Vector3 toTarget = col.bounds.center - origin;
        toTarget.y = 0f;

        return toTarget.sqrMagnitude < 0.001f || Vector3.Dot(direction, toTarget.normalized) > 0f;
    }

    public void DrawGizmos()
    {
        Vector3 origin = interactionOrigin != null ? interactionOrigin.position : transform.position + Vector3.up;
        Vector3 target = origin + transform.forward * Mathf.Max(0f, interactionDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, target);
        Gizmos.DrawWireSphere(target, Mathf.Max(0.01f, interactionRadius));
    }
}
