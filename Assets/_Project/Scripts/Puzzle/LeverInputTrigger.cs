using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to the lever pivot GameObject.
/// While Space is held, sets the Animator bool to true and the lever stays pulled down.
/// When Space is released, clears the bool and the release clip plays.
/// </summary>
[RequireComponent(typeof(Animator))]
public class LeverInputTrigger : MonoBehaviour
{
    [Tooltip("Animator bool parameter used to keep the lever held down.")]
    [SerializeField] private string boolName = "IsPulled";

    [Tooltip("Optional explicit animator reference. If null, uses the one on this GameObject.")]
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (animator == null)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.spaceKey.isPressed)
        {
            if (!animator.GetBool(boolName))
            {
                animator.SetBool(boolName, true);
            }
        }
        else
        {
            if (animator.GetBool(boolName))
            {
                animator.SetBool(boolName, false);
            }
        }
    }
}