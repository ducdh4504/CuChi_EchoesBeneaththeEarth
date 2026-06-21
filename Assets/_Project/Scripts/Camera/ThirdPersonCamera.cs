using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Mục tiêu theo dõi")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform cinemachineTarget;

    [Header("Thông số camera")]
    [SerializeField] private float distance = 1f;     
    [SerializeField] private float standingHeight = 0.8f;       
    [SerializeField] private float sneakingHeight = 0.35f;       
    [SerializeField] private float crawlingHeight = 0.2f;       
    [SerializeField] private float heightTransitionSpeed = 8f;
    [SerializeField] private float mouseSensitivity = 0.2f; //delta của Input System lớn hơn nên giá trị này nhỏ hơn
    [SerializeField] private float minPitch = -30f;   // Góc nhìn xuống tối đa
    [SerializeField] private float maxPitch = 60f;    // Góc nhìn lên tối đa

    private float yaw;   // Xoay trái/phải
    private float pitch; // Ngẩng lên/cúi xuống

    private AnStanceController stanceController;
    private float currentHeight;

    private void Start()
    {
        // Khóa và ẩn con trỏ chuột cho giống game third-person.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Transform drivenTransform = cinemachineTarget != null ? cinemachineTarget : transform;
        Vector3 angles = drivenTransform.eulerAngles;
        yaw = angles.y;
        pitch = NormalizePitch(angles.x);

        currentHeight = standingHeight;
        if (target != null)
        {
            stanceController = target.GetComponent<AnStanceController>();
            if (stanceController == null) stanceController = target.GetComponentInParent<AnStanceController>();
        }
    }

    // Dùng LateUpdate để camera cập nhật SAU khi nhân vật đã di chuyển xong.
    private void LateUpdate()
    {
        if (target == null) return;
        if (Time.timeScale <= 0f) return;

        // Đọc chuyển động chuột qua Input System.
        // Nếu không có chuột (null) thì coi như không xoay.
        Vector2 mouseDelta = Mouse.current != null
            ? Mouse.current.delta.ReadValue()
            : Vector2.zero;

        // Xoay góc nhìn theo chuột.
        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Xác định chiều cao mục tiêu dựa trên tư thế
        float targetHeight = standingHeight;
        if (stanceController != null)
        {
            if (stanceController.CurrentStance == AnStance.Crawling)
                targetHeight = crawlingHeight;
            else if (stanceController.CurrentStance == AnStance.Sneaking)
                targetHeight = sneakingHeight;
        }

        // Nội suy mượt mà chiều cao hiện tại tới chiều cao mục tiêu
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * heightTransitionSpeed);

        // Tính vị trí camera dựa trên góc xoay.
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = target.position + Vector3.up * currentHeight;

        if (cinemachineTarget != null)
        {
            cinemachineTarget.SetPositionAndRotation(focusPoint, rotation);
            return;
        }

        Vector3 camPosition = focusPoint - rotation * Vector3.forward * distance;

        transform.position = camPosition;
        transform.LookAt(focusPoint);
    }

    private static float NormalizePitch(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
