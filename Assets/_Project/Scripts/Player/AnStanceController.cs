using UnityEngine;

public class AnStanceController : MonoBehaviour
{
    private readonly Collider[] overlapBuffer = new Collider[12];

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private LayerMask collisionMask;
    private LayerMask groundLayer;
    private float standingHeight;
    private float sneakingHeight;
    private float crawlingHeight;
    private float standingRadius;
    private float sneakingRadius;
    private float crawlingRadius;
    private float capsuleBottomY;
    private Vector2 capsuleCenterXZ;
    private int uprightCapsuleDirection = 1;

    public AnStance CurrentStance { get; private set; } = AnStance.Standing;

    public void Initialize(
        Rigidbody rb,
        CapsuleCollider capsule,
        LayerMask collisionMask,
        LayerMask groundLayer,
        float standingHeight,
        float sneakingHeight,
        float crawlingHeight,
        float standingRadius,
        float sneakingRadius,
        float crawlingRadius)
    {
        this.rb = rb;
        this.capsule = capsule;
        this.collisionMask = collisionMask;
        this.groundLayer = groundLayer;
        this.standingHeight = Mathf.Max(standingHeight, capsule.height);
        this.sneakingHeight = sneakingHeight;
        this.crawlingHeight = crawlingHeight;
        this.standingRadius = Mathf.Max(standingRadius, capsule.radius);
        this.sneakingRadius = Mathf.Min(sneakingRadius, sneakingHeight * 0.5f - 0.01f);
        this.crawlingRadius = Mathf.Min(crawlingRadius, crawlingHeight * 0.5f - 0.01f);
        uprightCapsuleDirection = capsule.direction;
        capsuleBottomY = capsule.center.y - capsule.height * 0.5f;
        capsuleCenterXZ = new Vector2(capsule.center.x, capsule.center.z);
        ApplyStance(AnStance.Standing);
    }

    public void Resolve(AnStance requestedStance)
    {
        AnStance target = requestedStance;

        if (target == AnStance.Standing && !CanFitStance(AnStance.Standing))
        {
            target = CanFitStance(AnStance.Sneaking) ? AnStance.Sneaking : AnStance.Crawling;
        }
        else if (target == AnStance.Sneaking && !CanFitStance(AnStance.Sneaking))
        {
            target = AnStance.Crawling;
        }

        if (target != CurrentStance)
        {
            ApplyStance(target);
        }
    }

    public void ApplyStance(AnStance stance)
    {
        GetStanceSize(stance, out float height, out float radius);
        int direction = GetCapsuleDirection(stance);

        height = Mathf.Max(height, 0.1f);
        radius = Mathf.Clamp(radius, 0.05f, height * 0.5f - 0.01f);

        capsule.direction = direction;
        capsule.height = height;
        capsule.radius = radius;
        capsule.center = GetLocalCenter(height, radius, direction);

        CurrentStance = stance;
    }

    public bool CanFitStance(AnStance stance)
    {
        GetStanceSize(stance, out float height, out float radius);
        GetWorldCapsule(
            height,
            radius,
            GetCapsuleDirection(stance),
            out Vector3 top,
            out Vector3 bottom,
            out float worldRadius);

        int count = Physics.OverlapCapsuleNonAlloc(
            top,
            bottom,
            worldRadius,
            overlapBuffer,
            collisionMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider hit = overlapBuffer[i];
            if (hit == null || hit == capsule || hit.attachedRigidbody == rb)
            {
                continue;
            }

            if (IsInLayerMask(hit.gameObject.layer, groundLayer))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    public void GetWorldCapsule(out Vector3 top, out Vector3 bottom, out float worldRadius)
    {
        GetWorldCapsule(capsule.height, capsule.radius, capsule.direction, out top, out bottom, out worldRadius);
    }

    private void GetStanceSize(AnStance stance, out float height, out float radius)
    {
        switch (stance)
        {
            case AnStance.Crawling:
                height = crawlingHeight;
                radius = crawlingRadius;
                break;
            case AnStance.Sneaking:
                height = sneakingHeight;
                radius = sneakingRadius;
                break;
            default:
                height = standingHeight;
                radius = standingRadius;
                break;
        }
    }

    private void GetWorldCapsule(
        float height,
        float radius,
        int direction,
        out Vector3 top,
        out Vector3 bottom,
        out float worldRadius)
    {
        radius = Mathf.Clamp(radius, 0.05f, height * 0.5f - 0.01f);

        Vector3 localCenter = GetLocalCenter(height, radius, direction);
        Vector3 worldCenter = transform.TransformPoint(localCenter);
        Vector3 worldAxis = GetWorldAxis(direction);

        Vector3 scale = transform.lossyScale;
        float worldHeight = height * GetAxisScale(scale, direction);
        worldRadius = radius * GetRadiusScale(scale, direction);

        float halfSegment = Mathf.Max(0f, worldHeight * 0.5f - worldRadius);
        top = worldCenter + worldAxis * halfSegment;
        bottom = worldCenter - worldAxis * halfSegment;
    }

    private int GetCapsuleDirection(AnStance stance)
    {
        return stance == AnStance.Crawling ? 2 : uprightCapsuleDirection;
    }

    private Vector3 GetLocalCenter(float height, float radius, int direction)
    {
        float centerY = direction == 1 ? capsuleBottomY + height * 0.5f : capsuleBottomY + radius;
        return new Vector3(capsuleCenterXZ.x, centerY, capsuleCenterXZ.y);
    }

    private Vector3 GetWorldAxis(int direction)
    {
        switch (direction)
        {
            case 0:
                return transform.right;
            case 2:
                return transform.forward;
            default:
                return transform.up;
        }
    }

    private static float GetAxisScale(Vector3 scale, int direction)
    {
        switch (direction)
        {
            case 0:
                return Mathf.Abs(scale.x);
            case 2:
                return Mathf.Abs(scale.z);
            default:
                return Mathf.Abs(scale.y);
        }
    }

    private static float GetRadiusScale(Vector3 scale, int direction)
    {
        switch (direction)
        {
            case 0:
                return Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            case 2:
                return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            default:
                return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        }
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
