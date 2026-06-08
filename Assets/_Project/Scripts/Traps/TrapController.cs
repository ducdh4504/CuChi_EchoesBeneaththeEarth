using UnityEngine;

public class TrapController : MonoBehaviour
{
    [Tooltip("Kéo thả object có chứa Animator của bẫy vào đây")]
    public Animator trapAnimator;

    [Tooltip("Tên của Trigger bạn đã tạo trong Animator")]
    public string triggerName = "Snap";
    private bool hasTriggered = false; 
    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true; 

            if (trapAnimator != null)
            {
                trapAnimator.SetTrigger(triggerName);
            }
        }
    }

}
