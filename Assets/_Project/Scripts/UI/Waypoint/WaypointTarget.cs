using UnityEngine;

public class WaypointTarget : MonoBehaviour
{
    [Tooltip("Use the mission id here. Examples: Day1_FindTinBox, Day2_MorseTransmission, Day3_FindMap.")]
    [SerializeField] private string waypointId;
    [Tooltip("Small world-space offset so the arrow points above the target instead of the floor.")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [Tooltip("If the player gets this close, this waypoint is considered reached and the indicator hides.")]
    [SerializeField] private float touchDistance = 2.5f;
    [Tooltip("Only objects with this tag can complete the waypoint through trigger/collision contact.")]
    [SerializeField] private string playerTag = "Player";

    public string WaypointId => waypointId;
    public Vector3 Position => transform.position + worldOffset;
    public bool HasBeenReached { get; private set; }

    public void MarkReached()
    {
        HasBeenReached = true;
    }

    public bool IsCloseEnoughToReach(Transform viewer)
    {
        if (HasBeenReached || viewer == null)
        {
            return HasBeenReached;
        }

        Vector3 offsetToViewer = viewer.position - transform.position;
        float horizontalDistance = new Vector2(offsetToViewer.x, offsetToViewer.z).magnitude;

        if (horizontalDistance <= touchDistance || offsetToViewer.magnitude <= touchDistance)
        {
            MarkReached();
        }

        return HasBeenReached;
    }

    public static bool TryFind(string id, out WaypointTarget target)
    {
        target = null;

        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        WaypointTarget[] targets = FindObjectsByType<WaypointTarget>(FindObjectsInactive.Exclude);

        for (int i = 0; i < targets.Length; i++)
        {
            WaypointTarget candidate = targets[i];
            if (candidate != null && candidate.WaypointId == id)
            {
                target = candidate;
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(Position, 0.35f);
        Gizmos.DrawLine(transform.position, Position);

        Gizmos.color = new Color(0.2f, 1f, 0.35f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, touchDistance);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryMarkReachedByContact(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryMarkReachedByContact(collision.gameObject);
    }

    private void TryMarkReachedByContact(GameObject other)
    {
        if (other != null && other.CompareTag(playerTag))
        {
            MarkReached();
        }
    }
}
