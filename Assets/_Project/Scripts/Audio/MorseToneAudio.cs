using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MorseToneAudio : MonoBehaviour
{
    [Header("Tone Settings")]
    [SerializeField] private float frequency = 750f;
    [SerializeField, Range(0f, 1f)] private float toneVolume = 0.35f;
    [SerializeField] private int sampleRate = 44100;

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Low Latency")]
    [SerializeField] private bool keepToneRunningSilently = true;

    private AudioClip toneClip;
    private bool isToneOn;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        CreateToneClip();
        SetupAudioSource();
    }

    private void Start()
    {
        // Cho AudioSource chạy sẵn nhưng không nghe thấy.
        // Khi nhấn Space chỉ cần bật volume lên, sẽ phản hồi nhanh hơn AudioSource.Play().
        if (keepToneRunningSilently && audioSource != null && toneClip != null)
        {
            audioSource.volume = 0f;
            audioSource.Play();
        }
    }

    private void CreateToneClip()
    {
        int sampleLength = sampleRate;
        float[] samples = new float[sampleLength];

        for (int i = 0; i < sampleLength; i++)
        {
            float time = (float)i / sampleRate;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * time);
        }

        toneClip = AudioClip.Create(
            "Generated_Morse_Tone",
            sampleLength,
            1,
            sampleRate,
            false);

        toneClip.SetData(samples, 0);
    }

    private void SetupAudioSource()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.clip = toneClip;
        audioSource.volume = 0f;
        audioSource.pitch = 1f;
    }

    public void StartTone()
    {
        if (audioSource == null)
        {
            return;
        }

        isToneOn = true;

        if (keepToneRunningSilently)
        {
            audioSource.volume = toneVolume;

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            return;
        }

        audioSource.volume = toneVolume;
        audioSource.Play();
    }

    public void StopTone()
    {
        if (audioSource == null)
        {
            return;
        }

        isToneOn = false;

        if (keepToneRunningSilently)
        {
            audioSource.volume = 0f;
            return;
        }

        audioSource.Stop();
    }

    private void OnDisable()
    {
        if (audioSource == null)
        {
            return;
        }

        isToneOn = false;
        audioSource.volume = 0f;
    }
}