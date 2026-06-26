//using UnityEngine;
//using UnityEngine.InputSystem;

//public class DialogueManager : MonoBehaviour
//{
//    public static DialogueManager Instance { get; private set; }

//    [Header("References")]
//    [SerializeField] private DialogueUI dialogueUI;
//    [SerializeField] private AnMovement playerMovement;

//    [Header("Input")]
//    [SerializeField] private float inputDelayAfterStart = 0.15f;

//    private DialogueData currentDialogue;
//    private int currentLineIndex;
//    private bool isDialogueActive;
//    private float canAdvanceAtTime;

//    public bool IsDialogueActive => isDialogueActive;

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//    }

//    private void Start()
//    {
//        FindPlayerMovementIfNeeded();

//        if (dialogueUI != null)
//        {
//            dialogueUI.Hide();
//        }
//    }

//    private void Update()
//    {
//        if (!isDialogueActive)
//        {
//            return;
//        }

//        if (Time.time < canAdvanceAtTime)
//        {
//            return;
//        }

//        if (WasAdvancePressed())
//        {
//            ShowNextLine();
//        }
//    }

//    public void StartDialogue(DialogueData dialogueData)
//    {
//        if (dialogueData == null)
//        {
//            Debug.LogWarning("Cannot start dialogue because DialogueData is null.");
//            return;
//        }

//        if (dialogueData.lines == null || dialogueData.lines.Length == 0)
//        {
//            Debug.LogWarning($"Dialogue '{dialogueData.name}' has no lines.");
//            return;
//        }

//        currentDialogue = dialogueData;
//        currentLineIndex = 0;
//        isDialogueActive = true;
//        canAdvanceAtTime = Time.time + inputDelayAfterStart;

//        SetPlayerControlEnabled(false);
//        ShowCurrentLine();
//    }

//    private void ShowCurrentLine()
//    {
//        if (currentDialogue == null)
//        {
//            EndDialogue();
//            return;
//        }

//        if (currentLineIndex < 0 || currentLineIndex >= currentDialogue.lines.Length)
//        {
//            EndDialogue();
//            return;
//        }

//        if (dialogueUI == null)
//        {
//            Debug.LogWarning("DialogueUI is not assigned in DialogueManager.");
//            return;
//        }

//        dialogueUI.ShowLine(currentDialogue.lines[currentLineIndex]);
//    }

//    private void ShowNextLine()
//    {
//        currentLineIndex++;

//        if (currentDialogue == null || currentLineIndex >= currentDialogue.lines.Length)
//        {
//            EndDialogue();
//            return;
//        }

//        ShowCurrentLine();
//    }
//    private void EndDialogue()
//    {
//        DialogueData completedDialogue = currentDialogue;

//        if (dialogueUI != null)
//        {
//            dialogueUI.Hide();
//        }

//        if (completedDialogue != null && !string.IsNullOrWhiteSpace(completedDialogue.messageAfterDialogue))
//        {
//            Debug.Log(completedDialogue.messageAfterDialogue);
//        }

//        currentDialogue = null;
//        currentLineIndex = 0;
//        isDialogueActive = false;

//        SetPlayerControlEnabled(true);

//        if (
//            completedDialogue != null &&
//            !string.IsNullOrWhiteSpace(completedDialogue.objectiveIdAfterDialogue) &&
//            MissionManager.Instance != null
//        )
//        {
//            MissionManager.Instance.HandleObjectiveId(completedDialogue.objectiveIdAfterDialogue);
//        }
//    }

//    private void SetPlayerControlEnabled(bool enabled)
//    {
//        FindPlayerMovementIfNeeded();

//        if (playerMovement != null)
//        {
//            playerMovement.enabled = enabled;
//        }

//        if (!enabled && playerMovement != null)
//        {
//            Rigidbody rb = playerMovement.GetComponent<Rigidbody>();
//            if (rb != null)
//            {
//                rb.linearVelocity = Vector3.zero;
//            }
//        }
//    }

//    private void FindPlayerMovementIfNeeded()
//    {
//        if (playerMovement != null)
//        {
//            return;
//        }

//        GameObject player = GameObject.FindGameObjectWithTag("Player");
//        if (player == null)
//        {
//            return;
//        }

//        playerMovement = player.GetComponent<AnMovement>();
//    }

//    private bool WasAdvancePressed()
//    {
//        Keyboard keyboard = Keyboard.current;
//        if (keyboard != null)
//        {
//            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame)
//            {
//                return true;
//            }
//        }

//        Mouse mouse = Mouse.current;
//        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
//        {
//            return true;
//        }

//        return false;
//    }
//}

using System.Collections;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private AnMovement playerMovement;

    [Header("Voice")]
    [SerializeField] private AudioSource voiceAudioSource;

    [Header("Auto Advance Settings")]
    [SerializeField] private float defaultDelayAfterLine = 0.15f;
    [SerializeField] private float minimumFallbackDuration = 1.5f;
    [SerializeField] private float secondsPerCharacterFallback = 0.045f;
    [SerializeField] private float maximumFallbackDuration = 6f;

    [Header("Gameplay UI Lock")]
    [Tooltip("Optional. Nếu muốn ẩn HUD gameplay khi thoại, kéo GameplayHUDRoot vào đây.")]
    [SerializeField] private GameObject gameplayHudRoot;

    [SerializeField] private bool hideGameplayHudDuringDialogue = false;

    private DialogueData currentDialogue;
    private int currentLineIndex;
    private bool isDialogueActive;
    private Coroutine dialogueRoutine;

    public bool IsDialogueActive => isDialogueActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (voiceAudioSource == null)
        {
            voiceAudioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        FindPlayerMovementIfNeeded();

        if (dialogueUI != null)
        {
            dialogueUI.Hide();
        }

        SetGameplayHudVisible(true);
    }

    public void StartDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("Cannot start dialogue because DialogueData is null.");
            return;
        }

        if (dialogueData.lines == null || dialogueData.lines.Length == 0)
        {
            Debug.LogWarning($"Dialogue '{dialogueData.name}' has no lines.");
            return;
        }

        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }

        if (voiceAudioSource != null)
        {
            voiceAudioSource.Stop();
        }

        currentDialogue = dialogueData;
        currentLineIndex = 0;
        isDialogueActive = true;

        SetPlayerControlEnabled(false);
        SetGameplayHudVisible(false);

        dialogueRoutine = StartCoroutine(PlayDialogueRoutine());
    }

    private IEnumerator PlayDialogueRoutine()
    {
        while (
            currentDialogue != null &&
            currentDialogue.lines != null &&
            currentLineIndex >= 0 &&
            currentLineIndex < currentDialogue.lines.Length
        )
        {
            DialogueLine line = currentDialogue.lines[currentLineIndex];

            if (dialogueUI == null)
            {
                Debug.LogWarning("DialogueUI is not assigned in DialogueManager.");
                EndDialogue();
                yield break;
            }

            dialogueUI.ShowLine(line);

            float waitDuration = GetLineDuration(line);

            if (voiceAudioSource != null && line != null && line.voiceClip != null)
            {
                voiceAudioSource.clip = line.voiceClip;
                voiceAudioSource.loop = false;
                voiceAudioSource.Play();

                waitDuration = line.voiceClip.length;
            }

            if (waitDuration <= 0f)
            {
                waitDuration = minimumFallbackDuration;
            }

            yield return new WaitForSeconds(waitDuration);

            float delayAfterLine = line != null ? line.delayAfterLine : defaultDelayAfterLine;
            if (delayAfterLine <= 0f)
            {
                delayAfterLine = defaultDelayAfterLine;
            }

            if (delayAfterLine > 0f)
            {
                yield return new WaitForSeconds(delayAfterLine);
            }

            currentLineIndex++;
        }

        EndDialogue();
    }

    private float GetLineDuration(DialogueLine line)
    {
        if (line == null)
        {
            return minimumFallbackDuration;
        }

        if (line.voiceClip != null)
        {
            return line.voiceClip.length;
        }

        if (line.fallbackDuration > 0f)
        {
            return line.fallbackDuration;
        }

        string content = line.content;
        if (string.IsNullOrWhiteSpace(content))
        {
            return minimumFallbackDuration;
        }

        float calculatedDuration = minimumFallbackDuration + content.Length * secondsPerCharacterFallback;
        return Mathf.Clamp(calculatedDuration, minimumFallbackDuration, maximumFallbackDuration);
    }

    private void EndDialogue()
    {
        DialogueData completedDialogue = currentDialogue;

        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }

        if (voiceAudioSource != null)
        {
            voiceAudioSource.Stop();
            voiceAudioSource.clip = null;
        }

        if (dialogueUI != null)
        {
            dialogueUI.Hide();
        }

        if (completedDialogue != null && !string.IsNullOrWhiteSpace(completedDialogue.messageAfterDialogue))
        {
            Debug.Log(completedDialogue.messageAfterDialogue);
        }

        currentDialogue = null;
        currentLineIndex = 0;
        isDialogueActive = false;

        SetGameplayHudVisible(true);
        SetPlayerControlEnabled(true);

        if (
            completedDialogue != null &&
            !string.IsNullOrWhiteSpace(completedDialogue.objectiveIdAfterDialogue) &&
            MissionManager.Instance != null
        )
        {
            MissionManager.Instance.HandleObjectiveId(completedDialogue.objectiveIdAfterDialogue);
        }
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        FindPlayerMovementIfNeeded();

        if (playerMovement != null)
        {
            playerMovement.enabled = enabled;
        }

        if (!enabled && playerMovement != null)
        {
            Rigidbody rb = playerMovement.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void SetGameplayHudVisible(bool visible)
    {
        if (!hideGameplayHudDuringDialogue)
        {
            return;
        }

        if (gameplayHudRoot != null)
        {
            gameplayHudRoot.SetActive(visible);
        }
    }

    private void FindPlayerMovementIfNeeded()
    {
        if (playerMovement != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }

        playerMovement = player.GetComponent<AnMovement>();
    }
}