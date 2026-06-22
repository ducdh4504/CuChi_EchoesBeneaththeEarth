using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class JumpAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private AnMovement playerMovement;

    [Header("Jump Audio")]
    [SerializeField] private AudioClip jumpSfx;
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 0.75f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundCheckDistance = 1.25f;
    [SerializeField] private float groundedGraceTime = 0.12f;

    [Header("Input")]
    [SerializeField] private float jumpSoundCooldown = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private float lastGroundedTime;
    private float lastJumpSoundTime;

    private void Awake()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<AnMovement>();
        }
    }

    private void Update()
    {
        UpdateGroundedTime();

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (!keyboard.spaceKey.wasPressedThisFrame)
        {
            return;
        }

        TryPlayJumpSound();
    }

    private void UpdateGroundedTime()
    {
        if (IsGrounded())
        {
            lastGroundedTime = Time.time;
        }
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.25f;

        return Physics.Raycast(
            origin,
            Vector3.down,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore);
    }

    private void TryPlayJumpSound()
    {
        if (playerMovement != null && !playerMovement.enabled)
        {
            if (debugLogs)
            {
                Debug.Log("Jump sound skipped because AnMovement is disabled.");
            }

            return;
        }

        if (Time.time - lastJumpSoundTime < jumpSoundCooldown)
        {
            return;
        }

        bool canPlayBecauseGrounded = Time.time - lastGroundedTime <= groundedGraceTime;

        if (!canPlayBecauseGrounded)
        {
            if (debugLogs)
            {
                Debug.Log("Jump sound skipped because player is not grounded.");
            }

            return;
        }

        PlayJumpSfx();
    }

    private void PlayJumpSfx()
    {
        if (jumpSfx == null)
        {
            Debug.LogWarning("Jump SFX is not assigned.");
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager was not found. Cannot play jump SFX.");
            return;
        }

        lastJumpSoundTime = Time.time;

        AudioManager.Instance.PlaySFX(jumpSfx, jumpVolume);

        if (debugLogs)
        {
            Debug.Log("Jump SFX played.");
        }
    }
}