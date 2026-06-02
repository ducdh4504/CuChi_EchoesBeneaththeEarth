using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AnMovement))]
public class AnController : MonoBehaviour
{
    public AnMovement Movement { get; private set; }
    public bool IsGrounded => Movement != null && Movement.IsGrounded;
    public bool IsMoving => Movement != null && Movement.IsMoving;
    public bool IsRunning => Movement != null && Movement.IsRunning;
    public bool IsSneaking => Movement != null && Movement.IsSneaking;
    public bool IsCrawling => Movement != null && Movement.IsCrawling;

    private void Awake()
    {
        Movement = GetComponent<AnMovement>();
    }
}
