using System.Collections;
using UnityEngine;

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
    }

    private void Start()
    {
        StartEnding();
    }

    public void StartEnding()
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
}
