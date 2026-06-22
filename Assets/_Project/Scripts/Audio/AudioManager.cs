using System.Collections;
using UnityEngine;

public enum GameMusicCue
{
    None,
    MainMenu,
    GameplayLow,
    MapFound,
    Ending,
    Custom
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip mapFoundMusic;
    [SerializeField] private AudioClip endingMusic;

    [Header("Default Volumes")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float musicVolumeMultiplier = 1f;
    [Range(0f, 1f)][SerializeField] private float ambienceVolumeMultiplier = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolumeMultiplier = 1f;

    [Header("Music Cue Volumes")]
    [Range(0f, 1f)][SerializeField] private float mainMenuMusicVolume = 0.45f;
    [Range(0f, 1f)][SerializeField] private float gameplayMusicVolume = 0.14f;
    [Range(0f, 1f)][SerializeField] private float mapFoundMusicVolume = 0.45f;
    [Range(0f, 1f)][SerializeField] private float endingMusicVolume = 0.65f;

    [Header("Fade Settings")]
    [SerializeField] private float defaultMusicFadeDuration = 1.5f;
    [SerializeField] private float storyMusicFadeDuration = 2f;
    [SerializeField] private float defaultAmbienceFadeDuration = 1f;

    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;

    private Coroutine musicRoutine;
    private Coroutine ambienceRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateSourcesIfNeeded();

        activeMusicSource = musicSourceA;
        inactiveMusicSource = musicSourceB;
    }

    private void CreateSourcesIfNeeded()
    {
        musicSourceA = CreateSourceIfMissing(musicSourceA, "MusicSource_A", true);
        musicSourceB = CreateSourceIfMissing(musicSourceB, "MusicSource_B", true);
        ambienceSource = CreateSourceIfMissing(ambienceSource, "AmbienceSource", true);
        sfxSource = CreateSourceIfMissing(sfxSource, "SFXSource", false);
    }

    private AudioSource CreateSourceIfMissing(AudioSource source, string childName, bool loop)
    {
        if (source == null)
        {
            Transform child = transform.Find(childName);

            if (child == null)
            {
                GameObject childObject = new GameObject(childName);
                childObject.transform.SetParent(transform, false);
                child = childObject.transform;
            }

            source = child.GetComponent<AudioSource>();

            if (source == null)
            {
                source = child.gameObject.AddComponent<AudioSource>();
            }
        }

        SetupSource(source, loop);
        return source;
    }

    private void SetupSource(AudioSource source, bool loop)
    {
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        //source.volume = 0f;
        source.volume = loop ? 0f : 1f;
    }

    public void PlayMusicCue(
        GameMusicCue cue,
        AudioClip customClip = null,
        float customVolume = -1f,
        bool customLoop = true,
        float fadeDuration = -1f)
    {
        switch (cue)
        {
            case GameMusicCue.None:
                return;

            case GameMusicCue.MainMenu:
                PlayMainMenuMusic(fadeDuration);
                return;

            case GameMusicCue.GameplayLow:
                PlayGameplayMusicLow(fadeDuration);
                return;

            case GameMusicCue.MapFound:
                PlayMapFoundMusic(fadeDuration);
                return;

            case GameMusicCue.Ending:
                PlayEndingMusic(fadeDuration);
                return;

            case GameMusicCue.Custom:
                float volume = customVolume >= 0f ? customVolume : gameplayMusicVolume;
                PlayMusic(customClip, volume, customLoop, fadeDuration);
                return;
        }
    }

    public void PlayMainMenuMusic(float fadeDuration = -1f)
    {
        PlayMusic(mainMenuMusic, mainMenuMusicVolume, true, fadeDuration);
    }

    public void PlayGameplayMusicLow(float fadeDuration = -1f)
    {
        PlayMusic(gameplayMusic, gameplayMusicVolume, true, fadeDuration);
    }

    public void PlayMapFoundMusic(float fadeDuration = -1f)
    {
        if (mapFoundMusic != null)
        {
            PlayMusic(mapFoundMusic, mapFoundMusicVolume, true, ResolveFadeDuration(fadeDuration, storyMusicFadeDuration));
            return;
        }

        FadeMusicVolume(mapFoundMusicVolume, ResolveFadeDuration(fadeDuration, storyMusicFadeDuration));
    }

    public void PlayEndingMusic(float fadeDuration = -1f)
    {
        if (endingMusic != null)
        {
            PlayMusic(endingMusic, endingMusicVolume, false, ResolveFadeDuration(fadeDuration, storyMusicFadeDuration));
            return;
        }

        FadeMusicVolume(endingMusicVolume, ResolveFadeDuration(fadeDuration, storyMusicFadeDuration));
    }

    public void PlayMusic(AudioClip clip, float targetVolume, bool loop = true, float fadeDuration = -1f)
    {
        if (clip == null)
        {
            return;
        }

        fadeDuration = ResolveFadeDuration(fadeDuration, defaultMusicFadeDuration);
        targetVolume = GetMusicVolume(targetVolume);

        if (activeMusicSource != null && activeMusicSource.clip == clip)
        {
            activeMusicSource.loop = loop;

            if (!activeMusicSource.isPlaying)
            {
                activeMusicSource.Play();
            }

            StartMusicRoutine(FadeSingleMusicSource(activeMusicSource, targetVolume, fadeDuration));
            return;
        }

        StartMusicRoutine(CrossFadeMusic(clip, targetVolume, loop, fadeDuration));
    }

    public void FadeMusicVolume(float targetVolume, float fadeDuration = -1f)
    {
        if (activeMusicSource == null)
        {
            return;
        }

        targetVolume = GetMusicVolume(targetVolume);
        fadeDuration = ResolveFadeDuration(fadeDuration, defaultMusicFadeDuration);

        StartMusicRoutine(FadeSingleMusicSource(activeMusicSource, targetVolume, fadeDuration));
    }

    public void StopMusic(float fadeDuration = -1f)
    {
        fadeDuration = ResolveFadeDuration(fadeDuration, defaultMusicFadeDuration);
        StartMusicRoutine(StopMusicRoutine(fadeDuration));
    }

    private void StartMusicRoutine(IEnumerator routine)
    {
        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
        }

        musicRoutine = StartCoroutine(routine);
    }

    private IEnumerator CrossFadeMusic(AudioClip newClip, float targetVolume, bool loop, float fadeDuration)
    {
        AudioSource oldSource = activeMusicSource;
        AudioSource newSource = inactiveMusicSource;

        newSource.clip = newClip;
        newSource.loop = loop;
        newSource.volume = 0f;
        newSource.Play();

        float oldStartVolume = oldSource != null ? oldSource.volume : 0f;

        if (fadeDuration <= 0f)
        {
            if (oldSource != null)
            {
                oldSource.Stop();
                oldSource.clip = null;
                oldSource.volume = 0f;
            }

            newSource.volume = targetVolume;

            activeMusicSource = newSource;
            inactiveMusicSource = oldSource;

            musicRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            if (oldSource != null)
            {
                oldSource.volume = Mathf.Lerp(oldStartVolume, 0f, t);
            }

            newSource.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        if (oldSource != null)
        {
            oldSource.Stop();
            oldSource.clip = null;
            oldSource.volume = 0f;
        }

        newSource.volume = targetVolume;

        activeMusicSource = newSource;
        inactiveMusicSource = oldSource;

        musicRoutine = null;
    }

    private IEnumerator FadeSingleMusicSource(AudioSource source, float targetVolume, float fadeDuration)
    {
        if (source == null)
        {
            musicRoutine = null;
            yield break;
        }

        if (source.clip != null && !source.isPlaying)
        {
            source.Play();
        }

        float startVolume = source.volume;

        if (fadeDuration <= 0f)
        {
            source.volume = targetVolume;
            musicRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        source.volume = targetVolume;
        musicRoutine = null;
    }

    private IEnumerator StopMusicRoutine(float fadeDuration)
    {
        float startVolumeA = musicSourceA != null ? musicSourceA.volume : 0f;
        float startVolumeB = musicSourceB != null ? musicSourceB.volume : 0f;

        if (fadeDuration <= 0f)
        {
            StopMusicSource(musicSourceA);
            StopMusicSource(musicSourceB);
            musicRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            if (musicSourceA != null)
            {
                musicSourceA.volume = Mathf.Lerp(startVolumeA, 0f, t);
            }

            if (musicSourceB != null)
            {
                musicSourceB.volume = Mathf.Lerp(startVolumeB, 0f, t);
            }

            yield return null;
        }

        StopMusicSource(musicSourceA);
        StopMusicSource(musicSourceB);

        musicRoutine = null;
    }

    private void StopMusicSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.Stop();
        source.clip = null;
        source.volume = 0f;
    }

    public void PlayAmbience(AudioClip clip, float targetVolume = 0.25f, bool loop = true, float fadeDuration = -1f)
    {
        if (ambienceSource == null)
        {
            return;
        }

        if (clip == null)
        {
            StopAmbience(fadeDuration);
            return;
        }

        targetVolume = GetAmbienceVolume(targetVolume);
        fadeDuration = ResolveFadeDuration(fadeDuration, defaultAmbienceFadeDuration);

        if (ambienceRoutine != null)
        {
            StopCoroutine(ambienceRoutine);
        }

        ambienceRoutine = StartCoroutine(SwitchAmbienceRoutine(clip, targetVolume, loop, fadeDuration));
    }

    public void StopAmbience(float fadeDuration = -1f)
    {
        if (ambienceSource == null)
        {
            return;
        }

        fadeDuration = ResolveFadeDuration(fadeDuration, defaultAmbienceFadeDuration);

        if (ambienceRoutine != null)
        {
            StopCoroutine(ambienceRoutine);
        }

        ambienceRoutine = StartCoroutine(StopAmbienceRoutine(fadeDuration));
    }

    private IEnumerator SwitchAmbienceRoutine(AudioClip clip, float targetVolume, bool loop, float fadeDuration)
    {
        if (ambienceSource.clip == clip)
        {
            ambienceSource.loop = loop;

            if (!ambienceSource.isPlaying)
            {
                ambienceSource.Play();
            }

            yield return FadeAmbienceVolume(targetVolume, fadeDuration);
            ambienceRoutine = null;
            yield break;
        }

        if (ambienceSource.isPlaying)
        {
            yield return FadeAmbienceVolume(0f, fadeDuration * 0.5f);
            ambienceSource.Stop();
        }

        ambienceSource.clip = clip;
        ambienceSource.loop = loop;
        ambienceSource.volume = 0f;
        ambienceSource.Play();

        yield return FadeAmbienceVolume(targetVolume, fadeDuration * 0.5f);

        ambienceRoutine = null;
    }

    private IEnumerator StopAmbienceRoutine(float fadeDuration)
    {
        yield return FadeAmbienceVolume(0f, fadeDuration);

        ambienceSource.Stop();
        ambienceSource.clip = null;

        ambienceRoutine = null;
    }

    private IEnumerator FadeAmbienceVolume(float targetVolume, float fadeDuration)
    {
        float startVolume = ambienceSource.volume;

        if (fadeDuration <= 0f)
        {
            ambienceSource.volume = targetVolume;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            ambienceSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        ambienceSource.volume = targetVolume;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.volume = 1f;
        sfxSource.PlayOneShot(clip, GetSfxVolume(volume));
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(clip, position, GetSfxVolume(volume));
    }

    private float ResolveFadeDuration(float requestedFadeDuration, float fallbackFadeDuration)
    {
        return requestedFadeDuration >= 0f ? requestedFadeDuration : fallbackFadeDuration;
    }

    private float GetMusicVolume(float volume)
    {
        return Mathf.Clamp01(volume * masterVolume * musicVolumeMultiplier);
    }

    private float GetAmbienceVolume(float volume)
    {
        return Mathf.Clamp01(volume * masterVolume * ambienceVolumeMultiplier);
    }

    private float GetSfxVolume(float volume)
    {
        return Mathf.Clamp01(volume * masterVolume * sfxVolumeMultiplier);
    }
}