using UnityEngine;

public class DocumentPanelAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Audio")]
    [SerializeField] private AudioClip openPaperSfx;
    [SerializeField] private AudioClip closePaperSfx;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

    private bool wasVisible;

    private void Start()
    {
        wasVisible = IsPanelVisible();
    }

    private void Update()
    {
        bool isVisible = IsPanelVisible();

        if (isVisible == wasVisible)
        {
            return;
        }

        if (isVisible)
        {
            PlaySfx(openPaperSfx);
        }
        else
        {
            PlaySfx(closePaperSfx);
        }

        wasVisible = isVisible;
    }

    private bool IsPanelVisible()
    {
        if (panelRoot == null)
        {
            return false;
        }

        if (!panelRoot.activeInHierarchy)
        {
            return false;
        }

        if (canvasGroup != null)
        {
            return canvasGroup.alpha > 0.01f;
        }

        return true;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(clip, volume);
    }
}