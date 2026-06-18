using UnityEngine;

/// <summary>
/// Lái vị trí + xoay của IK target theo stance hiện tại (Standing/Sneaking/Crawling).
/// Mỗi stance trỏ vào 1 anchor Transform — drag anchor trong Scene view để chỉnh pose tay.
/// </summary>
[DefaultExecutionOrder(100)]
public class AnIKTargetStancer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnMovement movement;
    [SerializeField] private Transform armIKTarget;

    [Header("Anchors theo stance")]
    [SerializeField] private Transform standingAnchor;
    [SerializeField] private Transform sneakingAnchor;
    [SerializeField] private Transform crawlingAnchor;

    [Header("Blend")]
    [Tooltip("Tốc độ chuyển pose khi đổi stance (cao = chuyển nhanh).")]
    [SerializeField] private float blendSpeed = 12f;

    private void Reset()
    {
        if (movement == null)
        {
            movement = GetComponentInParent<AnMovement>();
        }
    }

    private void LateUpdate()
    {
        if (armIKTarget == null || movement == null)
        {
            return;
        }

        Transform source = standingAnchor;
        if (movement.IsCrawling && crawlingAnchor != null)
        {
            source = crawlingAnchor;
        }
        else if (movement.IsSneaking && sneakingAnchor != null)
        {
            source = sneakingAnchor;
        }

        if (source == null)
        {
            return;
        }

        float t = Time.deltaTime * blendSpeed;
        armIKTarget.position = Vector3.Lerp(armIKTarget.position, source.position, t);
        armIKTarget.rotation = Quaternion.Slerp(armIKTarget.rotation, source.rotation, t);
    }
}
