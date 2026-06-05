using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private AnMovement playerMovement;

    [Header("Input")]
    [SerializeField] private float inputDelayAfterStart = 0.15f;

    private DialogueData currentDialogue;
    private int currentLineIndex;
    private bool isDialogueActive;
    private float canAdvanceAtTime;

    public bool IsDialogueActive => isDialogueActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        FindPlayerMovementIfNeeded();

        if (dialogueUI != null)
        {
            dialogueUI.Hide();
        }
    }

    private void Update()
    {
        if (!isDialogueActive)
        {
            return;
        }

        if (Time.time < canAdvanceAtTime)
        {
            return;
        }

        if (WasAdvancePressed())
        {
            ShowNextLine();
        }
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

        currentDialogue = dialogueData;
        currentLineIndex = 0;
        isDialogueActive = true;
        canAdvanceAtTime = Time.time + inputDelayAfterStart;

        SetPlayerControlEnabled(false);
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentDialogue == null)
        {
            EndDialogue();
            return;
        }

        if (currentLineIndex < 0 || currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        if (dialogueUI == null)
        {
            Debug.LogWarning("DialogueUI is not assigned in DialogueManager.");
            return;
        }

        dialogueUI.ShowLine(currentDialogue.lines[currentLineIndex]);
    }

    private void ShowNextLine()
    {
        currentLineIndex++;

        if (currentDialogue == null || currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void EndDialogue()
    {
        if (dialogueUI != null)
        {
            dialogueUI.Hide();
        }

        if (currentDialogue != null && !string.IsNullOrWhiteSpace(currentDialogue.messageAfterDialogue))
        {
            Debug.Log(currentDialogue.messageAfterDialogue);
        }

        currentDialogue = null;
        currentLineIndex = 0;
        isDialogueActive = false;

        SetPlayerControlEnabled(true);
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
            }
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

    private bool WasAdvancePressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame)
            {
                return true;
            }
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }
}