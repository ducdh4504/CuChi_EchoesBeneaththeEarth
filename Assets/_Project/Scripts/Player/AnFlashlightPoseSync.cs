using UnityEngine;
using UnityEngine.Animations.Rigging;

[DefaultExecutionOrder(50)]
public class AnFlashlightPoseSync : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnFlashlight flashlight;
    [SerializeField] private Rig armRig;
    [SerializeField] private Animator animator;

    [Header("Animator Layer")]
    [SerializeField] private string holdingLayerName = "HoldingItem";

    [Header("Blend")]
    [SerializeField] private float blendSpeed = 10f;

    private int holdingLayerIndex = -1;
    private float targetWeight;
    private float currentWeight;

    private void Reset()
    {
        if (flashlight == null) flashlight = GetComponentInParent<AnFlashlight>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (flashlight != null) flashlight.OnFlashlightStateChanged += HandleStateChanged;
        if (animator != null) holdingLayerIndex = animator.GetLayerIndex(holdingLayerName);
        currentWeight = 0f;
        targetWeight = 0f;
        ApplyWeight(0f);
    }

    private void OnDisable()
    {
        if (flashlight != null) flashlight.OnFlashlightStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        targetWeight = (flashlight != null && flashlight.IsEquipped) ? 1f : 0f;
    }

    private void Update()
    {
        if (Mathf.Approximately(currentWeight, targetWeight)) return;
        currentWeight = Mathf.MoveTowards(currentWeight, targetWeight, blendSpeed * Time.deltaTime);
        ApplyWeight(currentWeight);
    }

    private void HandleStateChanged(bool hasFlashlight, bool isEquipped, bool isLightOn)
    {
        targetWeight = (hasFlashlight && isEquipped) ? 1f : 0f;
    }

    private void ApplyWeight(float w)
    {
        if (armRig != null) armRig.weight = w;
        if (animator != null && holdingLayerIndex >= 0)
            animator.SetLayerWeight(holdingLayerIndex, w);
    }
}
