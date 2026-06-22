using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneDirector : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform shotsRoot;
    [SerializeField] private float secondsPerShot = 6f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool loopShots = true;

    [Header("Credits")]
    [SerializeField] private EndCreditsRoll creditsRoll;

    [Header("Scene Fade")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float initialBlackHoldDuration = 0.35f;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 1.2f;

    private Coroutine tourRoutine;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (shotsRoot == null)
        {
            GameObject foundShotsRoot = GameObject.Find("EndCameraShots");
            if (foundShotsRoot != null)
            {
                shotsRoot = foundShotsRoot.transform;
            }
        }

        if (creditsRoll == null)
        {
            creditsRoll = FindAnyObjectByType<EndCreditsRoll>();
        }

        if (fadeGroup == null)
        {
            GameObject foundFade = GameObject.Find("EndingFade");
            if (foundFade != null)
            {
                fadeGroup = foundFade.GetComponent<CanvasGroup>();
            }
        }

        if (fadeGroup != null)
        {
            fadeGroup.transform.SetAsLastSibling();
        }

        SetFade(1f);
    }

    private void Start()
    {
        StartCoroutine(StartEndingRoutine());
    }

    public void StartEnding()
    {
        StartCoroutine(StartEndingRoutine());
    }

    public IEnumerator FadeOutThenLoadScene(string sceneName)
    {
        yield return FadeTo(1f, fadeOutDuration);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator StartEndingRoutine()
    {
        if (creditsRoll != null)
        {
            creditsRoll.Play();
        }

        if (tourRoutine != null)
        {
            StopCoroutine(tourRoutine);
        }

        tourRoutine = StartCoroutine(CameraTourRoutine());

        if (initialBlackHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(initialBlackHoldDuration);
        }

        yield return FadeTo(0f, fadeInDuration);
    }

    private IEnumerator CameraTourRoutine()
    {
        if (targetCamera == null || shotsRoot == null || shotsRoot.childCount == 0)
        {
            yield break;
        }

        Transform firstShot = shotsRoot.GetChild(0);
        targetCamera.transform.SetPositionAndRotation(firstShot.position, firstShot.rotation);

        int shotIndex = 0;
        do
        {
            Transform fromShot = shotsRoot.GetChild(shotIndex);
            Transform toShot = shotsRoot.GetChild((shotIndex + 1) % shotsRoot.childCount);

            yield return MoveCamera(fromShot, toShot);

            shotIndex = (shotIndex + 1) % shotsRoot.childCount;
        }
        while (loopShots || shotIndex != 0);
    }

    private IEnumerator MoveCamera(Transform fromShot, Transform toShot)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.1f, secondsPerShot);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = moveCurve != null ? moveCurve.Evaluate(t) : t;

            targetCamera.transform.position = Vector3.Lerp(fromShot.position, toShot.position, curvedT);
            targetCamera.transform.rotation = Quaternion.Slerp(fromShot.rotation, toShot.rotation, curvedT);

            yield return null;
        }

        targetCamera.transform.SetPositionAndRotation(toShot.position, toShot.rotation);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (fadeGroup == null)
        {
            yield break;
        }

        float startAlpha = fadeGroup.alpha;
        float safeDuration = Mathf.Max(0.01f, duration);
        float startTime = Time.realtimeSinceStartup;

        while (true)
        {
            yield return null;

            float elapsed = Time.realtimeSinceStartup - startTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            SetFade(Mathf.Lerp(startAlpha, targetAlpha, t));

            if (t >= 1f)
            {
                break;
            }
        }

        SetFade(targetAlpha);
    }

    private void SetFade(float alpha)
    {
        if (fadeGroup == null)
        {
            return;
        }

        fadeGroup.alpha = alpha;
        fadeGroup.blocksRaycasts = alpha > 0.01f;
        fadeGroup.interactable = alpha > 0.01f;
    }
}
