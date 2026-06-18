using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Cầu nối generic giữa AnEquipmentController và Animator.
/// Khi slot đang cầm đổi -> ghi int "heldItemSlot" cho Animator và blend các Rig IK tương ứng.
/// Không hard-code item nào: thêm item mới chỉ cần thêm state/transition trong Animator
/// (hoặc thêm 1 dòng vào rigBindings nếu cần IK riêng).
/// </summary>
public class AnHeldItemAnimationDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnEquipmentController equipmentController;
    [SerializeField] private Animator animator;

    [Header("Animator Parameter")]
    [Tooltip("Tên int param trong Animator. State machine transition dựa trên giá trị này (0=None, 1=Slot1, 2=Slot2, 3=Slot3).")]
    [SerializeField] private string heldItemSlotParameter = "heldItemSlot";

    [Header("Animation Rigging (Optional)")]
    [Tooltip("Mỗi slot có thể gắn 1 Rig IK riêng. Slot không cần IK thì không cần thêm vào list.")]
    [SerializeField] private List<SlotRigBinding> rigBindings = new List<SlotRigBinding>();
    [Tooltip("Tốc độ blend weight Rig khi đổi slot.")]
    [SerializeField] private float rigBlendSpeed = 12f;

    [Serializable]
    public class SlotRigBinding
    {
        public HeldItemSlot slot = HeldItemSlot.None;
        public Rig rig;
    }

    private readonly HashSet<int> animatorParams = new HashSet<int>();
    private int heldItemSlotHash;
    private HeldItemSlot currentTargetSlot = HeldItemSlot.None;

    private void Awake()
    {
        FindReferencesIfNeeded();
        CacheAnimatorParameters();
        CacheParameterHashes();
        ApplySlot(equipmentController != null ? equipmentController.CurrentSlot : HeldItemSlot.None);
    }

    private void OnEnable()
    {
        FindReferencesIfNeeded();

        if (equipmentController != null)
        {
            equipmentController.OnEquippedSlotChanged += ApplySlot;
            ApplySlot(equipmentController.CurrentSlot);
        }
    }

    private void OnDisable()
    {
        if (equipmentController != null)
        {
            equipmentController.OnEquippedSlotChanged -= ApplySlot;
        }
    }

    private void Update()
    {
        BlendRigsTowardTarget();
    }

    private void ApplySlot(HeldItemSlot slot)
    {
        currentTargetSlot = slot;
        SetInt(heldItemSlotHash, (int)slot);
    }

    private void BlendRigsTowardTarget()
    {
        if (rigBindings == null || rigBindings.Count == 0)
        {
            return;
        }

        float step = Time.deltaTime * rigBlendSpeed;
        foreach (SlotRigBinding binding in rigBindings)
        {
            if (binding == null || binding.rig == null)
            {
                continue;
            }

            float targetWeight = binding.slot == currentTargetSlot ? 1f : 0f;
            binding.rig.weight = Mathf.MoveTowards(binding.rig.weight, targetWeight, step);
        }
    }

    private void FindReferencesIfNeeded()
    {
        if (equipmentController == null)
        {
            equipmentController = GetComponentInParent<AnEquipmentController>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
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

    private void CacheParameterHashes()
    {
        heldItemSlotHash = Animator.StringToHash(heldItemSlotParameter);
    }

    private void SetInt(int hash, int value)
    {
        if (animator != null && animatorParams.Contains(hash))
        {
            animator.SetInteger(hash, value);
        }
    }
}
