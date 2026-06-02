using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

/// <summary>
/// Điều khiển di chuyển nhân vật bằng Rigidbody:
/// đi bộ / lén (sneak) / bò (crawl), nhảy có coyote-time + jump buffer,
/// tự co/giãn capsule theo tư thế và tự trượt dọc tường khi va chạm.
/// Cần kèm 2 class AnInputHandler / AnInputSnapshot (không nằm trong file này).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class AnMovement : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    #region Kiểu dữ liệu

    private enum Stance
    {
        Standing,   
        Sneaking,   
        Crawling    
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Thông số (Inspector)

    [Header("Tốc độ di chuyển")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float sneakSpeed = 2.2f;
    [SerializeField] private float crawlSpeed = 1.25f;
    [SerializeField, Range(0f, 1f)] private float airControl = 0.55f; // hệ số điều khiển khi ở trên không
    [SerializeField] private float acceleration = 24f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Nhảy")]
    [SerializeField] private float jumpVelocity = 4.2f;
    [SerializeField] private Transform groundCheck;          // điểm kiểm tra mặt đất (có thể để trống)
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float coyoteTime = 0.08f;       // thời gian vẫn nhảy được sau khi rời đất
    [SerializeField] private float jumpBufferTime = 0.12f;   // thời gian nhớ lệnh nhảy trước khi chạm đất
    [SerializeField] private bool canJumpWhileCrawling;

    [Header("Tương tác")]
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private float interactionDistance = 1.6f;
    [SerializeField] private float interactionRadius = 0.35f;
    [SerializeField] private LayerMask interactionMask = ~0;

    [Header("Va chạm khi di chuyển")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionSkin = 0.04f;
    // Mặt có normal.y > ngưỡng này coi như sàn/dốc đi được (không chặn ngang).
    [SerializeField, Range(0f, 1f)] private float groundNormalThreshold = 0.55f;
    [SerializeField] private bool useFrictionlessMaterial = true;

    [Header("Kích thước capsule theo tư thế")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float sneakingHeight = 1.35f;
    [SerializeField] private float crawlingHeight = 0.8f;
    [SerializeField] private float standingRadius = 0.5f;
    [SerializeField] private float sneakingRadius = 0.45f;
    [SerializeField] private float crawlingRadius = 0.35f;

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Hash tham số Animator

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    private static readonly int IsSneakingHash = Animator.StringToHash("isSneaking");
    private static readonly int IsCrawlingHash = Animator.StringToHash("isCrawling");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int IsJumpingHash = Animator.StringToHash("isJumping");
    private static readonly int MoveSpeedHash = Animator.StringToHash("moveSpeed");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("verticalSpeed");

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Biến nội bộ

    private readonly Collider[] overlapBuffer = new Collider[12];
    private readonly RaycastHit[] castBuffer = new RaycastHit[12];
    private readonly HashSet<int> animatorParams = new HashSet<int>();
    private readonly AnInputHandler input = new AnInputHandler();

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Animator animator;
    private PhysicsMaterial frictionlessMaterial;

    // Trạng thái input / di chuyển
    private Vector2 moveInput;
    private Vector3 inputDirection;       
    private Stance requestedStance = Stance.Standing;
    private Stance currentStance = Stance.Standing;

    private bool sprintHeld;
    private bool sneakToggled;
    private bool crawlToggled;
    private bool isGrounded;
    private bool isMoving;

    private float lastGroundedTime = -999f;
    private float lastJumpPressedTime = -999f;

    // Dữ liệu gốc của capsule để giãn/co quanh chân nhân vật
    private float capsuleBottomY;
    private Vector2 capsuleCenterXZ;

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Thuộc tính public

    public bool IsGrounded => isGrounded;
    public bool IsMoving => isMoving;
    public bool IsRunning => IsSprintActive();
    public bool IsSneaking => currentStance == Stance.Sneaking;
    public bool IsCrawling => currentStance == Stance.Crawling;

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Vòng đời Unity

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        animator = GetComponentInChildren<Animator>();

        rb.freezeRotation = true;
        rb.useGravity = true;

        CacheCapsuleDefaults();
        CacheAnimatorParameters();
        CreateFrictionlessMaterial();
        ApplyStance(Stance.Standing);
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        UpdateGroundedState();
        ResolveRequestedStance();
        ApplyMovement();
        ApplyRotation();
        TryJump();
        UpdateAnimator();
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Đọc input

    private void ReadInput()
    {
        AnInputSnapshot snapshot = input.Read();
        moveInput = snapshot.Move;
        sprintHeld = snapshot.SprintHeld;

        // Toggle lén: bật/tắt, đồng thời tắt bò
        if (snapshot.SneakPressed)
        {
            sneakToggled = !sneakToggled;
            crawlToggled = false;
        }

        // Toggle bò: bật/tắt, đồng thời tắt lén
        if (snapshot.CrawlPressed)
        {
            crawlToggled = !crawlToggled;
            sneakToggled = false;
        }

        // Nhớ thời điểm bấm nhảy (jump buffer)
        if (snapshot.JumpPressed)
        {
            lastJumpPressedTime = Time.time;
        }

        if (snapshot.InteractPressed)
        {
            TryInteract();
        }

        requestedStance = ResolveRequestedStanceFromInput();
    }

    private Stance ResolveRequestedStanceFromInput()
    {
        if (crawlToggled) return Stance.Crawling;
        if (sneakToggled) return Stance.Sneaking;
        return Stance.Standing;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Kiểm tra mặt đất

    private void UpdateGroundedState()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                groundLayer,
                QueryTriggerInteraction.Ignore);
        }
        else
        {
            // Không có groundCheck thì cast một quả cầu nhỏ xuống dưới chân.
            Vector3 origin = transform.position + Vector3.up * 0.05f;
            isGrounded = Physics.SphereCast(
                origin,
                Mathf.Max(0.05f, capsule.radius * 0.85f),
                Vector3.down,
                out _,
                0.18f,
                groundLayer,
                QueryTriggerInteraction.Ignore);
        }

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Xử lý tư thế (stance)

    /// <summary>
    /// Chọn tư thế thực tế: nếu trần quá thấp không đứng/lén được thì tự hạ xuống tư thế vừa.
    /// </summary>
    private void ResolveRequestedStance()
    {
        Stance target = requestedStance;

        if (target == Stance.Standing && !CanFitStance(Stance.Standing))
        {
            target = CanFitStance(Stance.Sneaking) ? Stance.Sneaking : Stance.Crawling;
        }
        else if (target == Stance.Sneaking && !CanFitStance(Stance.Sneaking))
        {
            target = Stance.Crawling;
        }

        if (target != currentStance)
        {
            ApplyStance(target);
        }
    }

    private void ApplyStance(Stance stance)
    {
        GetStanceSize(stance, out float height, out float radius);

        height = Mathf.Max(height, 0.1f);
        radius = Mathf.Clamp(radius, 0.05f, height * 0.5f - 0.01f);

        capsule.height = height;
        capsule.radius = radius;
        // Giữ chân cố định, giãn capsule lên trên.
        capsule.center = new Vector3(
            capsuleCenterXZ.x,
            capsuleBottomY + height * 0.5f,
            capsuleCenterXZ.y);

        currentStance = stance;
    }

    /// <summary>Kiểm tra có đủ chỗ trống để chuyển sang tư thế này không (không bị trần/vật cản chặn).</summary>
    private bool CanFitStance(Stance stance)
    {
        GetStanceSize(stance, out float height, out float radius);
        GetWorldCapsule(height, radius, out Vector3 top, out Vector3 bottom, out float worldRadius);

        int count = Physics.OverlapCapsuleNonAlloc(
            top, bottom, worldRadius,
            overlapBuffer,
            collisionMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider hit = overlapBuffer[i];
            if (hit == null || hit == capsule || hit.attachedRigidbody == rb) continue;
            if (IsInLayerMask(hit.gameObject.layer, groundLayer)) continue; // bỏ qua mặt đất

            return false; // có vật cản chặn -> không vừa
        }

        return true;
    }

    private void GetStanceSize(Stance stance, out float height, out float radius)
    {
        switch (stance)
        {
            case Stance.Crawling:
                height = crawlingHeight; radius = crawlingRadius; break;
            case Stance.Sneaking:
                height = sneakingHeight; radius = sneakingRadius; break;
            default:
                height = standingHeight; radius = standingRadius; break;
        }
    }

    private float GetCurrentSpeed()
    {
        if (IsSprintActive()) return runSpeed;

        return currentStance switch
        {
            Stance.Crawling => crawlSpeed,
            Stance.Sneaking => sneakSpeed,
            _ => walkSpeed
        };
    }

    private bool IsSprintActive()
    {
        return sprintHeld
               && currentStance == Stance.Standing
               && moveInput.sqrMagnitude > 0.001f;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Di chuyển & xoay

    private void ApplyMovement()
    {
        // Hướng input quy ra world (lưu để dùng cho xoay nhân vật).
        inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        if (inputDirection.sqrMagnitude > 1f) inputDirection.Normalize();

        float speed = GetCurrentSpeed() * (isGrounded ? 1f : airControl);
        Vector3 targetVelocity = ResolveHorizontalVelocity(inputDirection, speed);

        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);

        // Tăng tốc khi muốn đi nhanh hơn, giảm tốc khi muốn dừng/chậm lại.
        float rate = targetVelocity.sqrMagnitude > horizontal.sqrMagnitude ? acceleration : deceleration;
        Vector3 next = Vector3.MoveTowards(horizontal, targetVelocity, rate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(next.x, velocity.y, next.z);
        isMoving = next.sqrMagnitude > 0.01f;
    }

    private void ApplyRotation()
    {
        // Xoay theo HƯỚNG INPUT (không xoay theo hướng trượt dọc tường).
        if (inputDirection.sqrMagnitude < 0.001f) return;

        Quaternion target = Quaternion.LookRotation(inputDirection, Vector3.up);
        Quaternion next = Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(next);
    }

    /// <summary>
    /// Trả về vận tốc ngang mong muốn, đã xử lý trượt dọc tường nếu phía trước bị chặn.
    /// </summary>
    private Vector3 ResolveHorizontalVelocity(Vector3 wishDirection, float speed)
    {
        if (wishDirection.sqrMagnitude < 0.001f || speed <= 0f)
            return Vector3.zero;

        Vector3 direction = wishDirection.normalized;
        float distance = speed * Time.fixedDeltaTime + collisionSkin;

        if (TryGetWallHit(direction, distance, out RaycastHit hit))
        {
            // Trượt dọc theo mặt tường.
            Vector3 slide = Vector3.ProjectOnPlane(direction, hit.normal);
            slide.y = 0f;

            // Nếu hướng trượt cũng bị chặn hoặc không còn hướng -> đứng yên.
            if (slide.sqrMagnitude < 0.001f || TryGetWallHit(slide.normalized, distance, out _))
                return Vector3.zero;

            direction = slide.normalized;
        }

        return direction * speed;
    }

    /// <summary>Cast capsule theo hướng di chuyển để tìm bức tường gần nhất chặn đường.</summary>
    private bool TryGetWallHit(Vector3 direction, float distance, out RaycastHit nearest)
    {
        nearest = default;

        GetWorldCapsule(out Vector3 top, out Vector3 bottom, out float radius);
        int count = Physics.CapsuleCastNonAlloc(
            top, bottom, radius, direction,
            castBuffer, distance,
            collisionMask,
            QueryTriggerInteraction.Ignore);

        float nearestDistance = float.PositiveInfinity;
        bool found = false;

        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = castBuffer[i];
            Collider col = hit.collider;

            if (col == null || col == capsule || col.attachedRigidbody == rb) continue;
            if (hit.normal.y > groundNormalThreshold) continue; // sàn/dốc, không tính là tường

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearest = hit;
                found = true;
            }
        }

        return found;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Nhảy

    private void TryJump()
    {
        bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool withinCoyote = Time.time - lastGroundedTime <= coyoteTime;

        if (!jumpBuffered || !withinCoyote) return;

        // Đang bò và không cho nhảy khi bò -> thử đứng dậy trước, nếu bị chặn thì thôi.
        if (currentStance == Stance.Crawling && !canJumpWhileCrawling)
        {
            if (!CanFitStance(Stance.Standing)) return;

            crawlToggled = false;
            requestedStance = Stance.Standing;
            ApplyStance(Stance.Standing);
        }

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpVelocity;
        rb.linearVelocity = velocity;

        // Reset để tránh nhảy lặp.
        isGrounded = false;
        lastJumpPressedTime = -999f;
        lastGroundedTime = -999f;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Tương tác

    private void TryInteract()
    {
        Collider target = FindInteractionTarget();
        if (target == null) return;

        target.SendMessageUpwards("Interact", null, SendMessageOptions.DontRequireReceiver);
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
            if (!CanInteractWith(hit)) continue;
            if (!IsInInteractionDirection(hit, origin, direction)) continue;

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

            if (!CanInteractWith(col)) continue;
            if (hit.distance >= nearestDistance) continue;

            nearest = col;
            nearestDistance = hit.distance;
        }

        return nearest;
    }

    private Vector3 GetInteractionOrigin()
    {
        if (interactionOrigin != null) return interactionOrigin.position;
        if (capsule != null) return transform.TransformPoint(capsule.center);

        return transform.position + Vector3.up;
    }

    private bool CanInteractWith(Collider col)
    {
        if (col == null || col == capsule) return false;
        if (col.attachedRigidbody == rb) return false;

        return true;
    }

    private bool IsInInteractionDirection(Collider col, Vector3 origin, Vector3 direction)
    {
        Vector3 toTarget = col.bounds.center - origin;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.001f) return true;

        return Vector3.Dot(direction, toTarget.normalized) > 0f;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Capsule helpers

    private void CacheCapsuleDefaults()
    {
        // Đảm bảo kích thước đứng không nhỏ hơn capsule gốc trong scene.
        standingHeight = Mathf.Max(standingHeight, capsule.height);
        standingRadius = Mathf.Max(standingRadius, capsule.radius);
        sneakingRadius = Mathf.Min(sneakingRadius, sneakingHeight * 0.5f - 0.01f);
        crawlingRadius = Mathf.Min(crawlingRadius, crawlingHeight * 0.5f - 0.01f);

        capsuleBottomY = capsule.center.y - capsule.height * 0.5f;
        capsuleCenterXZ = new Vector2(capsule.center.x, capsule.center.z);
    }

    private void GetWorldCapsule(out Vector3 top, out Vector3 bottom, out float worldRadius)
    {
        GetWorldCapsule(capsule.height, capsule.radius, out top, out bottom, out worldRadius);
    }

    private void GetWorldCapsule(float height, float radius, out Vector3 top, out Vector3 bottom, out float worldRadius)
    {
        radius = Mathf.Clamp(radius, 0.05f, height * 0.5f - 0.01f);

        Vector3 localCenter = new Vector3(
            capsuleCenterXZ.x,
            capsuleBottomY + height * 0.5f,
            capsuleCenterXZ.y);

        Vector3 worldCenter = transform.TransformPoint(localCenter);

        Vector3 scale = transform.lossyScale;
        float worldHeight = height * Mathf.Abs(scale.y);
        worldRadius = radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));

        float halfSegment = Mathf.Max(0f, worldHeight * 0.5f - worldRadius);
        top = worldCenter + Vector3.up * halfSegment;
        bottom = worldCenter - Vector3.up * halfSegment;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Vật liệu & Animator

    private void CreateFrictionlessMaterial()
    {
        if (!useFrictionlessMaterial) return;

        frictionlessMaterial = new PhysicsMaterial($"{name}_NoFriction")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };

        capsule.material = frictionlessMaterial;
    }

    private void CacheAnimatorParameters()
    {
        animatorParams.Clear();
        if (animator == null) return;

        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            animatorParams.Add(p.nameHash);
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        bool running = isGrounded && IsSprintActive();
        bool walking = isGrounded && currentStance == Stance.Standing && isMoving;
        bool sneaking = isGrounded && currentStance == Stance.Sneaking && isMoving;
        bool crawling = currentStance == Stance.Crawling;
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

        SetBool(IsWalkingHash, walking);
        SetBool(IsRunningHash, running);
        SetBool(IsSneakingHash, sneaking);
        SetBool(IsCrawlingHash, crawling);
        SetBool(IsGroundedHash, isGrounded);
        SetBool(IsJumpingHash, !isGrounded && rb.linearVelocity.y > 0.05f);
        SetFloat(MoveSpeedHash, horizontalSpeed);
        SetFloat(VerticalSpeedHash, rb.linearVelocity.y);
    }

    private void SetBool(int hash, bool value)
    {
        if (animatorParams.Contains(hash)) animator.SetBool(hash, value);
    }

    private void SetFloat(int hash, float value)
    {
        if (animatorParams.Contains(hash)) animator.SetFloat(hash, value);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Tiện ích & Gizmos

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = interactionOrigin != null ? interactionOrigin.position : transform.position + Vector3.up;
        Vector3 target = origin + transform.forward * Mathf.Max(0f, interactionDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, target);
        Gizmos.DrawWireSphere(target, Mathf.Max(0.01f, interactionRadius));

        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    #endregion
}
