using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuIntroController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;

    [Header("Menu Reveal")]
    [SerializeField] private CanvasGroup menuGroup;
    [SerializeField] private float revealDelay = 0.1f;
    [SerializeField] private float fadeDuration = 0.75f;

    [Header("Playback Safety")]
    [SerializeField] private float prepareTimeout = 12f;
    [SerializeField] private float stallRestartDelay = 0.75f;
    [SerializeField] private float endTolerance = 0.2f;

    private Coroutine playRoutine;
    private Coroutine revealRoutine;
    private bool menuRevealed;
    private double lastObservedTime;
    private float stallTimer;
    private float introStartedAt;

    private void Awake()
    {
        Application.runInBackground = true;

        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
    }

    private void OnEnable()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += HandleVideoFinished;
            videoPlayer.errorReceived += HandleVideoError;
        }

        PlayIntro();
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= HandleVideoFinished;
            videoPlayer.errorReceived -= HandleVideoError;
        }
    }

    private void Update()
    {
        if (menuRevealed || videoPlayer == null || !videoPlayer.isPrepared)
        {
            return;
        }

        if (IsVideoAtEnd())
        {
            HandleVideoFinished(videoPlayer);
            return;
        }

        if (!videoPlayer.isPlaying)
        {
            stallTimer += Time.unscaledDeltaTime;
            if (stallTimer >= stallRestartDelay)
            {
                videoPlayer.Play();
                stallTimer = 0f;
            }

            return;
        }

        double currentTime = videoPlayer.time;
        if (currentTime <= lastObservedTime + 0.01d)
        {
            stallTimer += Time.unscaledDeltaTime;
            if (stallTimer >= stallRestartDelay)
            {
                videoPlayer.Pause();
                videoPlayer.Play();
                stallTimer = 0f;
            }
        }
        else
        {
            lastObservedTime = currentTime;
            stallTimer = 0f;
        }
    }

    private void PlayIntro()
    {
        menuRevealed = false;
        stallTimer = 0f;
        lastObservedTime = 0d;
        introStartedAt = Time.realtimeSinceStartup;
        SetMenuVisible(false, 0f);

        if (videoPlayer == null || videoPlayer.clip == null)
        {
            RevealMenu();
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = false;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        videoPlayer.timeReference = VideoTimeReference.InternalTime;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        if (videoImage != null)
        {
            videoImage.enabled = true;
        }

        videoPlayer.Stop();
        videoPlayer.time = 0d;
        videoPlayer.frame = 0;

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }

        videoPlayer.Prepare();
        playRoutine = StartCoroutine(PlayWhenPrepared());
    }

    private IEnumerator PlayWhenPrepared()
    {
        float elapsed = 0f;
        while (videoPlayer != null && !videoPlayer.isPrepared && elapsed < prepareTimeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (videoPlayer == null)
        {
            RevealMenu();
            yield break;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("Main menu video could not be prepared. Menu will be shown.");
            RevealMenu();
            yield break;
        }

        introStartedAt = Time.realtimeSinceStartup;
        lastObservedTime = 0d;
        videoPlayer.time = 0d;
        videoPlayer.frame = 0;
        videoPlayer.Play();
    }

    private void HandleVideoFinished(VideoPlayer source)
    {
        if (source != null && source.clip != null)
        {
            float elapsed = Time.realtimeSinceStartup - introStartedAt;
            double expectedLength = source.clip.length;

            if (expectedLength > 0d && elapsed < expectedLength - endTolerance)
            {
                double correctedTime = Mathf.Clamp(elapsed, 0f, (float)expectedLength - endTolerance);
                source.Pause();
                source.time = correctedTime;
                lastObservedTime = correctedTime;
                source.Play();
                return;
            }

            source.Pause();
        }

        RevealMenu();
    }

    private bool IsVideoAtEnd()
    {
        if (videoPlayer == null || videoPlayer.clip == null)
        {
            return false;
        }

        ulong frameCount = videoPlayer.clip.frameCount;
        if (frameCount > 0 && videoPlayer.frame >= (long)frameCount - 2)
        {
            return true;
        }

        return videoPlayer.clip.length > 0d && videoPlayer.time >= videoPlayer.clip.length - endTolerance;
    }

    private void HandleVideoError(VideoPlayer source, string message)
    {
        Debug.LogWarning($"Main menu video error: {message}");
        RevealMenu();
    }

    private void RevealMenu()
    {
        if (menuRevealed)
        {
            return;
        }

        menuRevealed = true;

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
        }

        revealRoutine = StartCoroutine(RevealMenuRoutine());
    }

    private IEnumerator RevealMenuRoutine()
    {
        if (revealDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(revealDelay);
        }

        if (menuGroup == null)
        {
            yield break;
        }

        menuGroup.blocksRaycasts = true;
        menuGroup.interactable = true;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            menuGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        menuGroup.alpha = 1f;
    }

    private void SetMenuVisible(bool visible, float alpha)
    {
        if (menuGroup == null)
        {
            return;
        }

        menuGroup.alpha = alpha;
        menuGroup.interactable = visible;
        menuGroup.blocksRaycasts = visible;
    }
}
