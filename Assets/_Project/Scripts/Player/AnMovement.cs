using UnityEngine;
using UnityEngine.InputSystem;

public class AnMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private Animator animator;

    private Vector2 moveInput;
    private Vector3 moveDirection;

    private bool isGrounded;
    private bool isWalking;
    private bool jumpRequested;

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        rb.freezeRotation = true;
    }

    private void Update()
    {
        ReadMovementInput();
        ReadJumpInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        CheckGround();
        Move();
        Rotate();
        HandleJump();
    }

    private void ReadMovementInput()
    {
        moveInput = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            moveInput.y += 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            moveInput.y -= 1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            moveInput.x += 1f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            moveInput.x -= 1f;

        moveInput = moveInput.normalized;
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
    }

    private void ReadJumpInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }
    }

    private void UpdateAnimation()
    {
        isWalking = moveInput.sqrMagnitude > 0.01f;

        if (animator != null)
        {
            animator.SetBool(IsWalkingHash, isWalking);
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void Move()
    {
        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;
    }

    private void Rotate()
    {
        if (!isWalking) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        Quaternion smoothRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(smoothRotation);
    }

    private void HandleJump()
    {
        if (!jumpRequested) return;

        jumpRequested = false;

        if (!isGrounded) return;

        Jump();
    }

    private void Jump()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;

        rb.linearVelocity = velocity;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}