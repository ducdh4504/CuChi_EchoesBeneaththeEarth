using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "CuChi/Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Info")]
    public string dialogueId;
    public string dialogueTitle;

    [Header("Lines")]
    public DialogueLine[] lines;

    [Header("After Dialogue")]
    public string objectiveIdAfterDialogue;

    [TextArea(2, 4)]
    public string messageAfterDialogue;
}