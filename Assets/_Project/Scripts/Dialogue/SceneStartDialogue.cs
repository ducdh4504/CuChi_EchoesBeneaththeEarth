using System.Collections;
using UnityEngine;

public class SceneStartDialogue : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;
    [Tooltip("Số giây chờ sau khi scene load xong rồi mới mở thoại.")]
    [SerializeField] private float startDelay = 1.0f;
    [SerializeField] private bool playOnce = true;

    private static readonly System.Collections.Generic.HashSet<string> playedScenes = new System.Collections.Generic.HashSet<string>();

    private IEnumerator Start()
    {
        if (dialogueData == null) yield break;

        string sceneKey = gameObject.scene.name + "::" + (dialogueData != null ? dialogueData.name : "<null>");
        if (playOnce && playedScenes.Contains(sceneKey)) yield break;

        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        float timeout = 3f;
        while (DialogueManager.Instance == null && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (DialogueManager.Instance == null) yield break;

        playedScenes.Add(sceneKey);
        DialogueManager.Instance.StartDialogue(dialogueData);
    }
}
