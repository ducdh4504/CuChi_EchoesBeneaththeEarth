using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Game gõ Morse theo sắc lệnh: người chơi nhìn ảnh sắc lệnh và gõ lại bằng Morse.
/// Không hiển thị target/hint. Gõ đúng -> ký tự hiện màu xanh. Gõ sai -> nháy đỏ ký tự sai rồi biến mất, gõ lại.
/// Dấu cách trong câu mục tiêu được tự bỏ qua.
/// </summary>
public class MorseChallenge : MonoBehaviour
{
    [Header("Bộ giải Morse cần lắng nghe")]
    [SerializeField] private MorsePuzzle morsePuzzle;

    [Header("Câu mục tiêu (nội dung sắc lệnh)")]
    [SerializeField] private string targetMessage = "VU THANH HAI";

    [Header("UI")]
    [Tooltip("TMP Text hiển thị các ký tự đã gõ đúng (màu xanh) và ký tự sai (màu đỏ nháy). Cần bật Rich Text.")]
    [SerializeField] private TMP_Text typedDisplay;

    [Header("Màu hiển thị")]
    [SerializeField] private Color correctColor = new Color(0.30f, 0.85f, 0.40f);
    [SerializeField] private Color wrongColor = new Color(0.95f, 0.30f, 0.30f);

    [Header("Feedback")]
    [Tooltip("Thời gian (giây) hiển thị ký tự sai màu đỏ trước khi biến mất.")]
    [SerializeField] private float wrongFlashDuration = 0.6f;

    [Header("Sự kiện")]
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
        if (wrongFlashTimer > 0f)
        {
            wrongFlashTimer -= Time.deltaTime;
            if (wrongFlashTimer <= 0f)
            {
                wrongChar = '\0';
                Repaint();
            }
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
        }
        else
        {
            wrongChar = incoming == '?' ? '?' : incoming;
            wrongFlashTimer = wrongFlashDuration;
            Repaint();
        }
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

    public string TargetMessage => targetMessage;
    public int CurrentIndex => currentIndex;
    public bool IsFinished => finished;
}
