using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("NPC Info")]
    [SerializeField] private string npcDisplayName = "Giao liên kỳ cựu";

    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Settings")]
    [SerializeField] private bool canRepeatDialogue = true;

    private bool hasTalked;

    public string GetInteractPrompt()
    {
        return $"Nhấn E để nói chuyện với {npcDisplayName}";
    }

    public bool CanInteract()
    {
        if (dialogueData == null)
        {
            return false;
        }

        if (!canRepeatDialogue && hasTalked)
        {
            return false;
        }

        return DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning($"{gameObject.name} has no DialogueData assigned.");
            return;
        }

        if (!canRepeatDialogue && hasTalked)
        {
            Debug.Log($"{npcDisplayName} has nothing new to say.");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueManager was not found in the scene.");
            return;
        }

        hasTalked = true;
        DialogueManager.Instance.StartDialogue(dialogueData);
    }
}
