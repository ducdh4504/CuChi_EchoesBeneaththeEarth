using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MorseChallenge : MonoBehaviour
{
    [Header("Morse Decoder")]
    [SerializeField] private MorsePuzzle morsePuzzle;

    [Header("Target Message")]
    [SerializeField] private string targetMessage = "VU THANH HAI";

    [Header("UI")]
    [Tooltip("Shows correct typed letters in green and the latest wrong letter in red.")]
    [SerializeField] private TMP_Text typedDisplay;
    [Tooltip("Shows the full targetMessage as a faint hint so players know what to transmit.")]
    [SerializeField] private TMP_Text targetHintDisplay;

    [Header("Display Colors")]
    [SerializeField] private Color correctColor = new Color(0.30f, 0.85f, 0.40f);
    [SerializeField] private Color wrongColor = new Color(0.95f, 0.30f, 0.30f);
    [SerializeField] private Color pendingHintColor = new Color(1f, 1f, 1f, 0.28f);
    [SerializeField] private Color currentHintColor = new Color(1f, 0.88f, 0.30f, 0.92f);

    [Header("Hint")]
    [SerializeField] private bool showTargetHint = true;
    [SerializeField] private string targetHintPrefix = "Thong tin can truyen: ";

    [Header("Feedback")]
    [Tooltip("Seconds to show a wrong decoded letter before hiding it.")]
    [SerializeField] private float wrongFlashDuration = 0.6f;

    [Header("Events")]
    public UnityEvent onWin;

    private string normalizedTarget = string.Empty;
    private int currentIndex;
    private float wrongFlashTimer;
    private char wrongChar;
    private bool finished;

    private void OnEnable()
    {
        if (morsePuzzle != null)
        {
            morsePuzzle.LetterCommitted += HandleLetterCommitted;
        }
    }

    private void OnDisable()
    {
        if (morsePuzzle != null)
        {
            morsePuzzle.LetterCommitted -= HandleLetterCommitted;
        }
    }

    private void Start()
    {
        SetTarget(targetMessage);
    }

    private void Update()
    {
        if (wrongFlashTimer <= 0f)
        {
            return;
        }

        wrongFlashTimer -= Time.deltaTime;
        if (wrongFlashTimer <= 0f)
        {
            wrongChar = '\0';
            Repaint();
        }
    }

    public void SetTarget(string newTarget)
    {
        targetMessage = newTarget ?? string.Empty;
        normalizedTarget = targetMessage.ToUpperInvariant();
        currentIndex = 0;
        finished = false;
        wrongFlashTimer = 0f;
        wrongChar = '\0';
        SkipSpaces();

        if (morsePuzzle != null)
        {
            morsePuzzle.ResetDecoder();
        }

        Repaint();
    }

    public void ResetGame()
    {
        SetTarget(targetMessage);
    }

    private void HandleLetterCommitted(char decoded)
    {
        if (finished || normalizedTarget.Length == 0 || currentIndex >= normalizedTarget.Length)
        {
            return;
        }

        char incoming = char.ToUpperInvariant(decoded);
        char expected = normalizedTarget[currentIndex];

        if (incoming == expected)
        {
            currentIndex++;
            wrongChar = '\0';
            wrongFlashTimer = 0f;
            SkipSpaces();
            Repaint();

            if (currentIndex >= normalizedTarget.Length)
            {
                finished = true;
                onWin?.Invoke();
            }

            return;
        }

        wrongChar = incoming == '?' ? '?' : incoming;
        wrongFlashTimer = wrongFlashDuration;
        Repaint();
    }

    private void SkipSpaces()
    {
        while (currentIndex < normalizedTarget.Length && normalizedTarget[currentIndex] == ' ')
        {
            currentIndex++;
        }
    }

    private void Repaint()
    {
        RepaintTypedDisplay();
        RepaintTargetHint();
    }

    private void RepaintTypedDisplay()
    {
        if (typedDisplay == null)
        {
            return;
        }

        StringBuilder sb = new StringBuilder();
        string correctHex = ColorUtility.ToHtmlStringRGB(correctColor);
        string wrongHex = ColorUtility.ToHtmlStringRGB(wrongColor);

        for (int i = 0; i < currentIndex; i++)
        {
            char c = normalizedTarget[i];
            string token = c == ' ' ? "  " : c.ToString();
            sb.Append("<color=#").Append(correctHex).Append('>').Append(token).Append("</color>");
        }

        if (wrongChar != '\0')
        {
            sb.Append("<color=#").Append(wrongHex).Append('>').Append(wrongChar).Append("</color>");
        }

        typedDisplay.text = sb.ToString();
    }

    private void RepaintTargetHint()
    {
        if (targetHintDisplay == null)
        {
            return;
        }

        if (!showTargetHint || normalizedTarget.Length == 0)
        {
            targetHintDisplay.text = string.Empty;
            return;
        }

        StringBuilder sb = new StringBuilder(targetHintPrefix);
        string correctHex = ColorUtility.ToHtmlStringRGBA(correctColor);
        string currentHex = ColorUtility.ToHtmlStringRGBA(currentHintColor);
        string pendingHex = ColorUtility.ToHtmlStringRGBA(pendingHintColor);

        for (int i = 0; i < normalizedTarget.Length; i++)
        {
            char c = normalizedTarget[i];
            string token = c == ' ' ? " " : c.ToString();

            if (i < currentIndex)
            {
                AppendColoredToken(sb, token, correctHex, false);
            }
            else if (i == currentIndex)
            {
                AppendColoredToken(sb, token, currentHex, true);
            }
            else
            {
                AppendColoredToken(sb, token, pendingHex, false);
            }
        }

        targetHintDisplay.text = sb.ToString();
    }

    private static void AppendColoredToken(StringBuilder sb, string token, string hex, bool bold)
    {
        if (bold)
        {
            sb.Append("<b>");
        }

        sb.Append("<color=#").Append(hex).Append('>').Append(token).Append("</color>");

        if (bold)
        {
            sb.Append("</b>");
        }
    }

    public string TargetMessage => targetMessage;
    public int CurrentIndex => currentIndex;
    public bool IsFinished => finished;
}
