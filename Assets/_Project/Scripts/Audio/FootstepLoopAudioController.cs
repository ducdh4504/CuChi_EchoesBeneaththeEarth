using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FootstepLoopAudioController : MonoBehaviour
{
    private enum FootstepState
    {
        None,
        Walk,
        Run,
        Crouch,
        Crawl
    }

    [Header("References")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private AudioSource footstepSource;

    [Header("Walk Clips")]
    [SerializeField] private AudioClip walkTunnelClip;
    [SerializeField] private AudioClip walkGrassClip;

    [Header("Run Clips")]
    [SerializeField] private AudioClip runTunnelClip;
    [SerializeField] private AudioClip runGrassClip;

    [Header("Crouch / Sneak Clips")]
    [SerializeField] private AudioClip crouchTunnelClip;
    [SerializeField] private AudioClip crouchGrassClip;

    [Header("Crawl Clips")]
    [SerializeField] private AudioClip crawlTunnelClip;
    [SerializeField] private AudioClip crawlGrassClip;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundCheckDistance = 1.3f;
    [SerializeField] private bool useGrassSoundOnTerrain = true;

    [Header("Movement Thresholds")]
    [SerializeField] private float minMoveSpeed = 0.2f;
    [SerializeField] private float runSpeedThreshold = 3.2f;

    [Header("Fallback Thresholds If Animator Is Missing")]
    [SerializeField] private float crawlSpeedMax = 0.8f;
    [SerializeField] private float crouchSpeedMax = 1.8f;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float walkVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float runVolume = 0.65f;
    [SerializeField, Range(0f, 1f)] private float crouchVolume = 0.4f;
    [SerializeField, Range(0f, 1f)] private float crawlVolume = 0.35f;

    [Header("Pitch")]
    [SerializeField] private float walkPitch = 1f;
    [SerializeField] private float runPitch = 1f;
    [SerializeField] private float crouchPitch = 1f;
    [SerializeField] private float crawlPitch = 1f;

    [Header("Audio Source Settings")]
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private AudioClip currentClip;
    private FootstepState currentState = FootstepState.None;

    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    private static readonly int IsSneakingHash = Animator.StringToHash("isSneaking");
    private static readonly int IsCrawlingHash = Animator.StringToHash("isCrawling");

    private bool hasIsRunningParameter;
    private bool hasIsSneakingParameter;
    private bool hasIsCrawlingParameter;

    private void Awake()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }

        CacheAnimatorParameters();
        CreateFootstepSourceIfNeeded();
        SetupAudioSource();
    }

    private void Update()
    {
        if (playerRigidbody == null || footstepSource == null)
        {
            return;
        }

        if (!TryGetGroundHit(out RaycastHit groundHit))
        {
            StopFootstep();
            return;
        }

        float horizontalSpeed = GetHorizontalSpeed();

        if (horizontalSpeed < minMoveSpeed)
        {
            StopFootstep();
            return;
        }

        FootstepState targetState = GetFootstepState(horizontalSpeed);

        if (targetState == FootstepState.None)
        {
            StopFootstep();
            return;
        }

        AudioClip targetClip = ChooseClip(groundHit, targetState);

        if (targetClip == null)
        {
            StopFootstep();
            return;
        }

        PlayLoop(targetClip, targetState);
    }

    private void CacheAnimatorParameters()
    {
        if (playerAnimator == null)
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter in playerAnimator.parameters)
        {
            if (parameter.type != AnimatorControllerParameterType.Bool)
            {
                continue;
            }

            if (parameter.nameHash == IsRunningHash)
            {
                hasIsRunningParameter = true;
            }
            else if (parameter.nameHash == IsSneakingHash)
            {
                hasIsSneakingParameter = true;
            }
            else if (parameter.nameHash == IsCrawlingHash)
            {
                hasIsCrawlingParameter = true;
            }
        }
    }

    private void CreateFootstepSourceIfNeeded()
    {
        if (footstepSource != null)
        {
            return;
        }

        Transform existing = transform.Find("FootstepAudioSource");

        if (existing != null)
        {
            footstepSource = existing.GetComponent<AudioSource>();
        }

        if (footstepSource == null)
        {
            GameObject sourceObject = new GameObject("FootstepAudioSource");
            sourceObject.transform.SetParent(transform, false);
            footstepSource = sourceObject.AddComponent<AudioSource>();
        }
    }

    private void SetupAudioSource()
    {
        footstepSource.playOnAwake = false;
        footstepSource.loop = true;
        footstepSource.spatialBlend = spatialBlend;
        footstepSource.volume = walkVolume;
        footstepSource.pitch = walkPitch;
    }

    private bool TryGetGroundHit(out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * 0.25f;

        return Physics.Raycast(
            origin,
            Vector3.down,
            out hit,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore);
    }

    private float GetHorizontalSpeed()
    {
        Vector3 velocity = playerRigidbody.linearVelocity;
        velocity.y = 0f;
        return velocity.magnitude;
    }

    private FootstepState GetFootstepState(float horizontalSpeed)
    {
        if (playerAnimator != null)
        {
            if (hasIsCrawlingParameter && playerAnimator.GetBool(IsCrawlingHash))
            {
                return FootstepState.Crawl;
            }

            if (hasIsSneakingParameter && playerAnimator.GetBool(IsSneakingHash))
            {
                return FootstepState.Crouch;
            }

            if (hasIsRunningParameter && playerAnimator.GetBool(IsRunningHash))
            {
                return FootstepState.Run;
            }
        }

        if (horizontalSpeed >= runSpeedThreshold)
        {
            return FootstepState.Run;
        }

        if (horizontalSpeed <= crawlSpeedMax)
        {
            return FootstepState.Crawl;
        }

        if (horizontalSpeed <= crouchSpeedMax)
        {
            return FootstepState.Crouch;
        }

        return FootstepState.Walk;
    }

    private AudioClip ChooseClip(RaycastHit groundHit, FootstepState state)
    {
        bool isTerrain = groundHit.collider is TerrainCollider;
        bool useGrass = useGrassSoundOnTerrain && isTerrain;

        switch (state)
        {
            case FootstepState.Run:
                if (useGrass && runGrassClip != null)
                {
                    return runGrassClip;
                }

                if (runTunnelClip != null)
                {
                    return runTunnelClip;
                }

                return walkTunnelClip;

            case FootstepState.Crouch:
                if (useGrass && crouchGrassClip != null)
                {
                    return crouchGrassClip;
                }

                if (crouchTunnelClip != null)
                {
                    return crouchTunnelClip;
                }

                return walkTunnelClip;

            case FootstepState.Crawl:
                if (useGrass && crawlGrassClip != null)
                {
                    return crawlGrassClip;
                }

                if (crawlTunnelClip != null)
                {
                    return crawlTunnelClip;
                }

                return walkTunnelClip;

            case FootstepState.Walk:
                if (useGrass && walkGrassClip != null)
                {
                    return walkGrassClip;
                }

                return walkTunnelClip;

            default:
                return null;
        }
    }

    private void PlayLoop(AudioClip targetClip, FootstepState targetState)
    {
        bool shouldRestart =
            currentClip != targetClip ||
            currentState != targetState;

        if (shouldRestart)
        {
            currentClip = targetClip;
            currentState = targetState;

            footstepSource.Stop();
            footstepSource.clip = currentClip;
            footstepSource.loop = true;
            footstepSource.volume = GetVolumeForState(targetState);
            footstepSource.pitch = GetPitchForState(targetState);
            footstepSource.Play();

            if (debugLogs)
            {
                Debug.Log($"Footstep started: {targetState} - {currentClip.name}");
            }

            return;
        }

        footstepSource.volume = GetVolumeForState(targetState);
        footstepSource.pitch = GetPitchForState(targetState);

        if (!footstepSource.isPlaying)
        {
            footstepSource.Play();
        }
    }

    private float GetVolumeForState(FootstepState state)
    {
        switch (state)
        {
            case FootstepState.Run:
                return runVolume;

            case FootstepState.Crouch:
                return crouchVolume;

            case FootstepState.Crawl:
                return crawlVolume;

            case FootstepState.Walk:
                return walkVolume;

            default:
                return 0f;
        }
    }

    private float GetPitchForState(FootstepState state)
    {
        switch (state)
        {
            case FootstepState.Run:
                return runPitch;

            case FootstepState.Crouch:
                return crouchPitch;

            case FootstepState.Crawl:
                return crawlPitch;

            case FootstepState.Walk:
                return walkPitch;

            default:
                return 1f;
        }
    }

    private void StopFootstep()
    {
        if (footstepSource == null || !footstepSource.isPlaying)
        {
            currentClip = null;
            currentState = FootstepState.None;
            return;
        }

        footstepSource.Stop();
        currentClip = null;
        currentState = FootstepState.None;

        if (debugLogs)
        {
            Debug.Log("Footstep stopped.");
        }
    }
}