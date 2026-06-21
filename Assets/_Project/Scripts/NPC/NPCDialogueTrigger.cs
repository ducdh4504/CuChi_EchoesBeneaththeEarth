using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("NPC Info")]
    [SerializeField] private string npcDisplayName = "Giao liên kỳ cựu";

    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Settings")]
    [SerializeField] private bool canRepeatDialogue = true;
    
    //Audio
    [Header("Audio")]
    [SerializeField] private bool triggerMusicOnDialogueStart;
    [SerializeField] private GameMusicCue musicCueOnDialogueStart = GameMusicCue.Ending;
    [SerializeField] private AudioClip customDialogueMusic;
    [SerializeField, Range(-1f, 1f)] private float customDialogueMusicVolume = -1f;
    [SerializeField] private bool customDialogueMusicLoop;
    [SerializeField] private float dialogueMusicFadeDuration = 2f;

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
        PlayDialogueStartAudio();
        DialogueManager.Instance.StartDialogue(dialogueData);
    }
    
    //Audio
    private void PlayDialogueStartAudio()
    {
        if (!triggerMusicOnDialogueStart)
        {
            return;
        }

        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlayMusicCue(
            musicCueOnDialogueStart,
            customDialogueMusic,
            customDialogueMusicVolume,
            customDialogueMusicLoop,
            dialogueMusicFadeDuration);
    }
}
