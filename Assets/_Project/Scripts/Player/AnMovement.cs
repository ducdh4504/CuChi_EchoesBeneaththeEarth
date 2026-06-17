using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable 0649

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class AnMovement : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Keo Transform cua camera vao day. Neu de trong se tu lay Camera.main.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Toc do di chuyen")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 4.5f;
    [SerializeField] private float sneakSpeed = 1.2f;
    [SerializeField] private float crawlSpeed = 0.5f;
    [SerializeField, Range(0f, 1f)] private float airControl = 0.55f;
    [SerializeField] private float acceleration = 24f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float rotationSpeed = 3f;

    [Header("Nhay")]
    [SerializeField] private float jumpVelocity = 4.2f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float coyoteTime = 0.08f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private bool canJumpWhileCrawling;

    [Header("Tuong tac")]
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private float interactionDistance = 1.6f;
    [SerializeField] private float interactionRadius = 0.35f;
    [SerializeField] private LayerMask interactionMask = ~0;

    [Header("Va cham khi di chuyen")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionSkin = 0.04f;
    [SerializeField, Range(0f, 1f)] private float groundNormalThreshold = 0.55f;
    [SerializeField] private bool useFrictionlessMaterial = true;

    [Header("Kich thuoc capsule theo tu the")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float sneakingHeight = 1.35f;
    [SerializeField] private float crawlingHeight = 0.8f;
    [SerializeField] private float standingRadius = 0.5f;
    [SerializeField] private float sneakingRadius = 0.45f;
    [SerializeField] private float crawlingRadius = 0.35f;

    private readonly AnInputHandler input = new AnInputHandler();

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private AnGroundChecker groundChecker;
    private AnStanceController stanceController;
    private AnMotor motor;
    private AnInteractor interactor;
    private AnAnimatorDriver animatorDriver;
    private PhysicsMaterial frictionlessMaterial;

    // xử lý đèn pin
    private AnFlashlight flashlight;

    private Vector2 moveInput;
    private AnStance requestedStance = AnStance.Standing;
    private bool sprintHeld;
    private bool sneakToggled;
    private bool crawlToggled;
    private bool isGrounded;
    private float lastGroundedTime = -999f;
    private float lastJumpPressedTime = -999f;
    

    public bool IsGrounded => isGrounded;
    public bool IsMoving => motor != null && motor.IsMoving;
    public bool IsRunning => IsSprintActive();
    public bool IsSneaking => CurrentStance == AnStance.Sneaking;
    public bool IsCrawling => CurrentStance == AnStance.Crawling;

    private AnStance CurrentStance => stanceController != null ? stanceController.CurrentStance : AnStance.Standing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.useGravity = true;

        GetCameraTransform();
        CreateFrictionlessMaterial();
        InitializeComponents();
    }

    private void Update()
    {
        ReadInput();
    }

    private void OnDisable()
    {
        StopMovementAndAnimation();
    }

    private void FixedUpdate()
    {
        UpdateGroundedState();
        stanceController.Resolve(requestedStance);
        motor.Move(moveInput, GetCurrentSpeed(), isGrounded);
        motor.Rotate();
        TryJump();
        animatorDriver.UpdateState(isGrounded, motor.IsMoving, IsSprintActive(), CurrentStance, rb);
    }

    private void InitializeComponents()
    {
        groundChecker = GetOrAddComponent<AnGroundChecker>();
        stanceController = GetOrAddComponent<AnStanceController>();
        motor = GetOrAddComponent<AnMotor>();
        interactor = GetOrAddComponent<AnInteractor>();
        animatorDriver = GetOrAddComponent<AnAnimatorDriver>();

        // xử lý đèn pin
        flashlight = GetComponent<AnFlashlight>();

        stanceController.Initialize(
            rb,
            capsule,
            collisionMask,
            groundLayer,
            standingHeight,
            sneakingHeight,
            crawlingHeight,
            standingRadius,
            sneakingRadius,
            crawlingRadius);

        groundChecker.Initialize(groundCheck, groundCheckRadius, groundLayer, capsule);

        motor.Initialize(
            rb,
            capsule,
            stanceController,
            cameraTransform,
            collisionMask,
            airControl,
            acceleration,
            deceleration,
            rotationSpeed,
            collisionSkin,
            groundNormalThreshold);

        interactor.Initialize(
            rb,
            capsule,
            interactionOrigin,
            interactionDistance,
            interactionRadius,
            interactionMask);

        animatorDriver.Initialize(GetComponentInChildren<Animator>());
    }

    private void ReadInput()
    {
        AnInputSnapshot snapshot = input.Read();
        moveInput = snapshot.Move;
        sprintHeld = snapshot.SprintHeld;

        if (snapshot.SneakPressed)
        {
            sneakToggled = !sneakToggled;
            crawlToggled = false;
        }

        if (snapshot.CrawlPressed)
        {
            crawlToggled = !crawlToggled;
            sneakToggled = false;
        }

        if (snapshot.JumpPressed)
        {
            lastJumpPressedTime = Time.time;
        }

        if (snapshot.InteractPressed)
        {
            interactor.TryInteract();
        }

        // xử lý đèn pin
        if (snapshot.FlashlightPressed)
        {
            if (flashlight == null)
            {
                flashlight = GetComponent<AnFlashlight>();
            }

            flashlight?.HandleFlashlightButtonPressed();
        }

        requestedStance = ResolveRequestedStanceFromInput();
    }

    private AnStance ResolveRequestedStanceFromInput()
    {
        if (crawlToggled)
        {
            return AnStance.Crawling;
        }

        return sneakToggled ? AnStance.Sneaking : AnStance.Standing;
    }

    private void UpdateGroundedState()
    {
        isGrounded = groundChecker.CheckGrounded();

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }

    private void TryJump()
    {
        bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool withinCoyote = Time.time - lastGroundedTime <= coyoteTime;

        if (!jumpBuffered || !withinCoyote)
        {
            return;
        }

        if (CurrentStance == AnStance.Crawling && !canJumpWhileCrawling)
        {
            if (!stanceController.CanFitStance(AnStance.Standing))
            {
                return;
            }

            crawlToggled = false;
            requestedStance = AnStance.Standing;
            stanceController.ApplyStance(AnStance.Standing);
        }

        motor.Jump(jumpVelocity);
        isGrounded = false;
        lastJumpPressedTime = -999f;
        lastGroundedTime = -999f;
    }

    private void StopMovementAndAnimation()
    {
        moveInput = Vector2.zero;
        sprintHeld = false;
        lastJumpPressedTime = -999f;

        if (motor != null)
        {
            motor.StopImmediately();
        }
        else if (rb != null)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            rb.angularVelocity = Vector3.zero;
        }

        animatorDriver?.ResetMovementState(isGrounded, CurrentStance);
    }

    private float GetCurrentSpeed()
    {
        if (IsSprintActive())
        {
            return runSpeed;
        }

        switch (CurrentStance)
        {
            case AnStance.Crawling:
                return crawlSpeed;
            case AnStance.Sneaking:
                return sneakSpeed;
            default:
                return walkSpeed;
        }
    }

    private bool IsSprintActive()
    {
        return sprintHeld && CurrentStance == AnStance.Standing && moveInput.sqrMagnitude > 0.001f;
    }

    private void CreateFrictionlessMaterial()
    {
        if (!useFrictionlessMaterial)
        {
            return;
        }

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

    private void GetCameraTransform()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform == null)
        {
            cameraTransform = transform;
        }
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        return TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();
    }

    private void OnDrawGizmosSelected()
    {
        interactor?.DrawGizmos();
        groundChecker?.DrawGizmos();
    }
}
