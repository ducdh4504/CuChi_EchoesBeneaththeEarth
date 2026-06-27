using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visual")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.92f, 0.45f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.65f, 0.65f, 0.65f, 1f);

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField, Range(0f, 1f)] private float hoverVolume = 0.45f;
    [SerializeField, Range(0f, 1f)] private float clickVolume = 0.55f;

    private static AudioClip fallbackHoverSound;
    private static AudioClip fallbackClickSound;

    private Button button;
    private Graphic buttonGraphic;
    private Text buttonText;
    private Vector3 initialScale;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonGraphic = GetComponent<Graphic>();
        buttonText = GetComponentInChildren<Text>(true);
        initialScale = transform.localScale;

        if (audioSource == null)
        {
            audioSource = GetComponentInParent<AudioSource>();
        }

        ApplyNormalState();
    }

    private void OnEnable()
    {
        ApplyNormalState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable())
        {
            ApplyDisabledState();
            return;
        }

        transform.localScale = initialScale * hoverScale;
        ApplyColor(hoverColor);
        PlayOneShot(hoverSound != null ? hoverSound : GetFallbackHoverSound(), hoverVolume);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyNormalState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsInteractable())
        {
            return;
        }

        PlayOneShot(clickSound != null ? clickSound : GetFallbackClickSound(), clickVolume);
    }

    private bool IsInteractable()
    {
        return button != null && button.interactable;
    }

    private void ApplyNormalState()
    {
        transform.localScale = initialScale == Vector3.zero ? Vector3.one : initialScale;
        ApplyColor(IsInteractable() ? normalColor : disabledColor);
    }

    private void ApplyDisabledState()
    {
        transform.localScale = initialScale == Vector3.zero ? Vector3.one : initialScale;
        ApplyColor(disabledColor);
    }

    private void ApplyColor(Color color)
    {
        if (buttonGraphic != null)
        {
            buttonGraphic.color = color;
        }

        if (buttonText != null)
        {
            buttonText.color = color;
        }
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, volume);
    }

    private static AudioClip GetFallbackHoverSound()
    {
        if (fallbackHoverSound == null)
        {
            fallbackHoverSound = CreateTone("Menu Hover", 880f, 0.055f, 0.2f);
        }

        return fallbackHoverSound;
    }

    private static AudioClip GetFallbackClickSound()
    {
        if (fallbackClickSound == null)
        {
            fallbackClickSound = CreateTone("Menu Click", 520f, 0.075f, 0.26f);
        }

        return fallbackClickSound;
    }

    private static AudioClip CreateTone(string clipName, float frequency, float duration, float amplitude)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = 1f - (i / (float)sampleCount);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude * envelope;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
