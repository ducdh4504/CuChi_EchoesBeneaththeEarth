using System.Collections;
using UnityEngine;

public class MapQuestNPC : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("NPC Info")]
    [SerializeField] private string npcDisplayName = "Cựu chiến binh";

    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueIfHasMap;
    [SerializeField] private DialogueData dialogueIfNoMap;

    [Header("Inventory")]
    [SerializeField] private AnInventory inventory;

    [Header("On Has-Map Win")]
    [SerializeField] private bool loadSceneAfterHasMapDialogue = true;
    [SerializeField] private string sceneOnWin = "EndScene";
    [SerializeField] private string transitionMessage = "Hoàn thành nhiệm vụ...";

    private bool waitingForEndDialogue;

    public string GetInteractPrompt() => $"Nhấn E để nói chuyện với {npcDisplayName}";

    public bool CanInteract()
    {
        if (waitingForEndDialogue) return false;
        return DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive;
    }

    public void Interact()
    {
        if (DialogueManager.Instance == null) return;

        var inv = inventory != null ? inventory : FindInventory();
        bool hasMap = inv != null && inv.HasSmallMap;

        DialogueData data = hasMap ? dialogueIfHasMap : dialogueIfNoMap;
        if (data == null)
        {
            Debug.LogWarning($"{name}: missing dialogue for hasMap={hasMap}");
            return;
        }

        DialogueManager.Instance.StartDialogue(data);

        if (hasMap && loadSceneAfterHasMapDialogue && !string.IsNullOrWhiteSpace(sceneOnWin))
        {
            StartCoroutine(WaitForDialogueEndThenLoad());
        }
    }

    private IEnumerator WaitForDialogueEndThenLoad()
    {
        waitingForEndDialogue = true;
        yield return null;
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            yield return null;
        }

        if (SceneTransitionController.Instance != null)
        {
            SceneTransitionController.Instance.LoadScene(sceneOnWin, transitionMessage);
        }
        else
        {
            GameSaveSystem.CapturePlayerOxygenForSceneTransition();
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneOnWin);
        }
    }

    private static AnInventory FindInventory()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        return p != null ? p.GetComponent<AnInventory>() : null;
    }
}
