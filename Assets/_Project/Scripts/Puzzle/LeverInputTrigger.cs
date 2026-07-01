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

    [Tooltip("Optional Morse decoder to sync with. If null, this script searches the prefab root.")]
    [SerializeField] private MorsePuzzle morsePuzzle;

    [Header("Direct Lever Motion")]
    [SerializeField] private bool useDirectTransform = true;
    [SerializeField] private RectTransform visualPivot;
    [SerializeField] private float releasedEulerZ = -20.34375f;
    [SerializeField] private float pulledEulerZ = -58f;
    [SerializeField] private float directMotionSpeed = 16f;

    private bool subscribedToPuzzle;
    private bool isPulled;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (morsePuzzle == null && transform.root != null)
        {
            morsePuzzle = transform.root.GetComponentInChildren<MorsePuzzle>(true);
        }

        if (visualPivot == null)
        {
            visualPivot = transform as RectTransform;
        }

        if (useDirectTransform && animator != null)
        {
            animator.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (morsePuzzle != null)
        {
            morsePuzzle.HoldingChanged += SetPulled;
            subscribedToPuzzle = true;
            SetPulled(morsePuzzle.IsHolding);
        }
    }

    private void OnDisable()
    {
        if (subscribedToPuzzle && morsePuzzle != null)
        {
            morsePuzzle.HoldingChanged -= SetPulled;
        }

        subscribedToPuzzle = false;
        SetPulled(false);
    }

    private void Update()
    {
        if (animator == null && !useDirectTransform)
        {
            return;
        }

        if (morsePuzzle == null)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                SetPulled(keyboard.spaceKey.isPressed);
            }
        }

        UpdateDirectMotion();
    }

    private void SetPulled(bool pulled)
    {
        isPulled = pulled;

        if (useDirectTransform)
        {
            return;
        }

        if (animator != null && animator.GetBool(boolName) != pulled)
        {
            animator.SetBool(boolName, pulled);
        }
    }

    private void UpdateDirectMotion()
    {
        if (visualPivot == null)
        {
            visualPivot = transform as RectTransform;
        }

        if (!useDirectTransform || visualPivot == null)
        {
            return;
        }

        float targetZ = isPulled ? pulledEulerZ : releasedEulerZ;
        Vector3 euler = visualPivot.localEulerAngles;
        euler.z = Mathf.LerpAngle(euler.z, targetZ, 1f - Mathf.Exp(-directMotionSpeed * Time.deltaTime));
        visualPivot.localEulerAngles = euler;
    }
}
