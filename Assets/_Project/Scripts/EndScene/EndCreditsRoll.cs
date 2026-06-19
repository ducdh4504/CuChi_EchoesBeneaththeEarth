using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndCreditsRoll : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform creditsContent;
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] private CanvasGroup endPromptGroup;

    [Header("Motion")]
    [SerializeField] private float startY = -520f;
    [SerializeField] private float endY = 920f;
    [SerializeField] private float rollDuration = 55f;
    [SerializeField] private AnimationCurve rollCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Finish")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool allowReturnToMenu = true;

    private float elapsed;
    private bool isPlaying;
    private bool isFinished;

    private const string DefaultCredits =
        "SẮC LỆNH SỐ 8\n\n" +
        "Sắc lệnh cuối cùng đã được truyền đi.\n" +
        "Những căn phòng im lặng trở thành nhân chứng\n" +
        "cho một nhiệm vụ đã hoàn thành trong bóng tối.\n\n" +
        "An đã trở về từ đường hầm.\n" +
        "Bản đồ được giao lại.\n" +
        "Tín hiệu đã đến được với đồng đội.\n\n" +
        "Cảm ơn bạn đã chơi.\n\n\n" +
        "THỰC HIỆN\n" +
        "Thanh Hải và đồng đội\n\n" +
        "THE END";

    private void Awake()
    {
        FindReferencesIfNeeded();
        PrepareCreditsText();
        SetPromptVisible(false);
    }

    private void Update()
    {
        if (isPlaying)
        {
            UpdateRoll();
        }

        if (isFinished && allowReturnToMenu && WasReturnToMenuPressed())
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

private bool WasReturnToMenuPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        return keyboard.enterKey.wasPressedThisFrame
            || keyboard.numpadEnterKey.wasPressedThisFrame
            || keyboard.spaceKey.wasPressedThisFrame
            || keyboard.escapeKey.wasPressedThisFrame;
    }


    public void Play()
    {
        FindReferencesIfNeeded();
        elapsed = 0f;
        isPlaying = true;
        isFinished = false;
        SetPromptVisible(false);
        SetCreditsY(startY);
    }

    private void UpdateRoll()
    {
        elapsed += Time.deltaTime;
        float duration = Mathf.Max(0.1f, rollDuration);
        float t = Mathf.Clamp01(elapsed / duration);
        float curvedT = rollCurve != null ? rollCurve.Evaluate(t) : t;

        SetCreditsY(Mathf.Lerp(startY, endY, curvedT));

        if (t >= 1f)
        {
            isPlaying = false;
            isFinished = true;
            SetPromptVisible(true);
        }
    }

    private void FindReferencesIfNeeded()
    {
        if (creditsContent == null)
        {
            Transform found = transform.Find("CreditsContent");
            if (found != null)
            {
                creditsContent = found as RectTransform;
            }
        }

        if (creditsText == null && creditsContent != null)
        {
            creditsText = creditsContent.GetComponentInChildren<TMP_Text>(true);
        }

        if (endPromptGroup == null)
        {
            Transform foundPrompt = transform.Find("EndPrompt");
            if (foundPrompt != null)
            {
                endPromptGroup = foundPrompt.GetComponent<CanvasGroup>();
            }
        }
    }

    private void PrepareCreditsText()
    {
        if (creditsText != null && string.IsNullOrWhiteSpace(creditsText.text))
        {
            creditsText.text = DefaultCredits;
        }
    }

    private void SetCreditsY(float y)
    {
        if (creditsContent == null)
        {
            return;
        }

        Vector2 position = creditsContent.anchoredPosition;
        position.y = y;
        creditsContent.anchoredPosition = position;
    }

    private void SetPromptVisible(bool visible)
    {
        if (endPromptGroup == null)
        {
            return;
        }

        endPromptGroup.alpha = visible ? 1f : 0f;
        endPromptGroup.interactable = visible;
        endPromptGroup.blocksRaycasts = visible;
    }
}
