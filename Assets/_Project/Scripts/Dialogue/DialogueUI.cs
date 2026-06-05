using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private TMP_Text continueHintText;

    [Header("Settings")]
    [SerializeField] private string continueHint = "Nhấn Space / E để tiếp tục";

    private void Awake()
    {
        Hide();
    }

    public void ShowLine(DialogueLine line)
    {
        if (line == null)
        {
            return;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (speakerNameText != null)
        {
            speakerNameText.text = line.speakerName;
        }

        if (contentText != null)
        {
            contentText.text = line.content;
        }

        if (continueHintText != null)
        {
            continueHintText.text = continueHint;
        }
    }

    public void Hide()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
}