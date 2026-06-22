using UnityEngine;
using UnityEngine.UI;

public class QuestWaypointIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform indicatorRoot;
    [SerializeField] private Image arrowImage;
    [SerializeField] private Image reachedImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string playerTag = "Player";

    [Header("Layout")]
    [SerializeField] private float edgeMargin = 72f;
    [Tooltip("How close to the screen center the target must be before the X icon appears.")]
    [SerializeField] private float lookAtViewportRadius = 0.12f;
    [SerializeField] private float spriteForwardOffset = 180f;

    private WaypointTarget currentTarget;
    private RectTransform canvasRect;

    private void Awake()
    {
        CacheReferences();
        SetVisible(false);
    }

    private void LateUpdate()
    {
        if (currentTarget == null)
        {
            SetVisible(false);
            return;
        }

        CacheReferences();

        if (worldCamera == null || canvasRect == null || indicatorRoot == null)
        {
            SetVisible(false);
            return;
        }

        Transform reachViewer = playerTransform != null ? playerTransform : worldCamera.transform;
        if (currentTarget.IsCloseEnoughToReach(reachViewer) || currentTarget.HasBeenReached)
        {
            SetVisible(false);
            return;
        }

        Vector3 viewport = worldCamera.WorldToViewportPoint(currentTarget.Position);
        bool behindCamera = viewport.z < 0f;

        if (behindCamera)
        {
            viewport.x = 1f - viewport.x;
            viewport.y = 1f - viewport.y;
        }

        Vector2 direction = new Vector2(viewport.x - 0.5f, viewport.y - 0.5f);
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.up;
        }

        bool lookingAtTarget = IsLookingAtTarget(viewport);
        Rect rect = canvasRect.rect;

        if (lookingAtTarget)
        {
            indicatorRoot.anchoredPosition = ViewportToCanvasPosition(viewport, rect, edgeMargin);
            indicatorRoot.localRotation = Quaternion.identity;
        }
        else
        {
            indicatorRoot.anchoredPosition = GetEdgePosition(direction.normalized, rect, edgeMargin);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spriteForwardOffset;
            indicatorRoot.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (arrowImage != null)
        {
            arrowImage.enabled = !lookingAtTarget || reachedImage == null;
        }

        if (reachedImage != null)
        {
            reachedImage.enabled = lookingAtTarget;
        }

        SetVisible(true);
    }

    public void SetTarget(WaypointTarget target)
    {
        currentTarget = target;
        SetVisible(currentTarget != null);
    }

    public void SetTargetById(string waypointId)
    {
        if (WaypointTarget.TryFind(waypointId, out WaypointTarget target))
        {
            SetTarget(target);
            return;
        }

        ClearTarget();
    }

    public void ClearTarget()
    {
        currentTarget = null;
        SetVisible(false);
    }

    private void CacheReferences()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (canvasRect == null && canvas != null)
        {
            canvasRect = canvas.transform as RectTransform;
        }

        if (indicatorRoot == null)
        {
            indicatorRoot = transform as RectTransform;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (playerTransform == null && !string.IsNullOrWhiteSpace(playerTag))
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            return;
        }

        gameObject.SetActive(visible);
    }

    private static Vector2 GetEdgePosition(Vector2 direction, Rect rect, float margin)
    {
        float halfWidth = Mathf.Max(0f, rect.width * 0.5f - margin);
        float halfHeight = Mathf.Max(0f, rect.height * 0.5f - margin);

        float scaleX = Mathf.Abs(direction.x) > 0.0001f
            ? halfWidth / Mathf.Abs(direction.x)
            : float.PositiveInfinity;
        float scaleY = Mathf.Abs(direction.y) > 0.0001f
            ? halfHeight / Mathf.Abs(direction.y)
            : float.PositiveInfinity;

        float scale = Mathf.Min(scaleX, scaleY);
        return direction * scale;
    }

    private bool IsLookingAtTarget(Vector3 viewport)
    {
        if (viewport.z < 0f)
        {
            return false;
        }

        Vector2 fromCenter = new Vector2(viewport.x - 0.5f, viewport.y - 0.5f);
        return fromCenter.magnitude <= lookAtViewportRadius;
    }

    private static Vector2 ViewportToCanvasPosition(Vector3 viewport, Rect rect, float margin)
    {
        float x = Mathf.Lerp(rect.xMin, rect.xMax, viewport.x);
        float y = Mathf.Lerp(rect.yMin, rect.yMax, viewport.y);

        x = Mathf.Clamp(x, rect.xMin + margin, rect.xMax - margin);
        y = Mathf.Clamp(y, rect.yMin + margin, rect.yMax - margin);

        return new Vector2(x, y);
    }
}
