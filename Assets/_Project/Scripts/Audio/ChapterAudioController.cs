using UnityEngine;

public class ChapterAudioController : MonoBehaviour
{
    [Header("Music On Scene Start")]
    [SerializeField] private bool stopMusicOnStart;
    [SerializeField] private GameMusicCue musicCueOnStart = GameMusicCue.None;

    [Header("Custom Music")]
    [SerializeField] private AudioClip customMusicClip;
    [Range(0f, 1f)][SerializeField] private float customMusicVolume = 0.2f;
    [SerializeField] private bool customMusicLoop = true;
    [SerializeField] private float customMusicFadeDuration = 1.5f;

    [Header("Ambience On Scene Start")]
    [SerializeField] private bool stopAmbienceOnStart;
    [SerializeField] private bool playAmbienceOnStart = true;
    [SerializeField] private AudioClip ambienceClip;
    [Range(0f, 1f)][SerializeField] private float ambienceVolume = 0.25f;
    [SerializeField] private bool ambienceLoop = true;
    [SerializeField] private float ambienceFadeDuration = 1f;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager was not found. Add GameAudio prefab to the scene.");
            return;
        }

        HandleMusic();
        HandleAmbience();
    }

    private void HandleMusic()
    {
        if (stopMusicOnStart)
        {
            AudioManager.Instance.StopMusic(customMusicFadeDuration);
            return;
        }

        if (musicCueOnStart == GameMusicCue.None)
        {
            return;
        }

        AudioManager.Instance.PlayMusicCue(
            musicCueOnStart,
            customMusicClip,
            customMusicVolume,
            customMusicLoop,
            customMusicFadeDuration);
    }

    private void HandleAmbience()
    {
        if (stopAmbienceOnStart)
        {
            AudioManager.Instance.StopAmbience(ambienceFadeDuration);
            return;
        }

        if (!playAmbienceOnStart)
        {
            return;
        }

        if (ambienceClip == null)
        {
            return;
        }

        AudioManager.Instance.PlayAmbience(
            ambienceClip,
            ambienceVolume,
            ambienceLoop,
            ambienceFadeDuration);
    }
}