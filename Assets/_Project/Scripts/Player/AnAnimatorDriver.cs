using System.Collections.Generic;
using UnityEngine;

public class AnAnimatorDriver : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    private static readonly int IsSneakingHash = Animator.StringToHash("isSneaking");
    private static readonly int IsCrawlingHash = Animator.StringToHash("isCrawling");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int IsJumpingHash = Animator.StringToHash("isJumping");
    private static readonly int MoveSpeedHash = Animator.StringToHash("moveSpeed");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("verticalSpeed");

    private readonly HashSet<int> animatorParams = new HashSet<int>();
    private Animator animator;

    public void Initialize(Animator animator)
    {
        this.animator = animator;
        CacheAnimatorParameters();
    }

    public void UpdateState(bool isGrounded, bool isMoving, bool isRunning, AnStance stance, Rigidbody rb)
    {
        if (animator == null || rb == null)
        {
            return;
        }

        Vector3 velocity = rb.linearVelocity;
        float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;

        SetBool(IsWalkingHash, isGrounded && stance == AnStance.Standing && isMoving);
        SetBool(IsRunningHash, isGrounded && isRunning);
        SetBool(IsSneakingHash, isGrounded && stance == AnStance.Sneaking && isMoving);
        SetBool(IsCrawlingHash, stance == AnStance.Crawling);
        SetBool(IsGroundedHash, isGrounded);
        SetBool(IsJumpingHash, !isGrounded && velocity.y > 0.05f);
        SetFloat(MoveSpeedHash, horizontalSpeed);
        SetFloat(VerticalSpeedHash, velocity.y);
    }

    public void ResetMovementState(bool isGrounded, AnStance stance)
    {
        if (animator == null)
        {
            return;
        }

        SetBool(IsWalkingHash, false);
        SetBool(IsRunningHash, false);
        SetBool(IsSneakingHash, false);
        SetBool(IsCrawlingHash, stance == AnStance.Crawling);
        SetBool(IsGroundedHash, isGrounded);
        SetBool(IsJumpingHash, false);
        SetFloat(MoveSpeedHash, 0f);
        SetFloat(VerticalSpeedHash, 0f);
    }

    private void CacheAnimatorParameters()
    {
        animatorParams.Clear();
        if (animator == null)
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            animatorParams.Add(parameter.nameHash);
        }
    }

    private void SetBool(int hash, bool value)
    {
        if (animatorParams.Contains(hash))
        {
            animator.SetBool(hash, value);
        }
    }

    private void SetFloat(int hash, float value)
    {
        if (animatorParams.Contains(hash))
        {
            animator.SetFloat(hash, value);
        }
    }
}
