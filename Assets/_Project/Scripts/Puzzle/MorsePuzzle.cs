using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Bộ giải mã Morse tự do: giữ phím Space ngắn = chấm (.), giữ lâu = gạch (-).
/// Nhả tay đủ lâu sẽ chốt 1 chữ cái và giải mã ra ký tự; nhả lâu hơn nữa = dấu cách.
/// Gắn script này vào một GameObject trong scene và kéo các Text/đèn báo vào Inspector.
/// </summary>
public class MorsePuzzle : MonoBehaviour
{
    [Header("Ngưỡng thời gian (giây)")]
    [Tooltip("Giữ Space ngắn hơn mức này = chấm (.), lâu hơn = gạch (-).")]
    [SerializeField] private float dashThreshold = 0.3f;
    [Tooltip("Nhả tay im lặng đủ lâu sẽ chốt và giải mã chữ cái hiện tại.")]
    [SerializeField] private float letterGap = 0.7f;
    [Tooltip("Nhả tay im lặng lâu hơn nữa sẽ thêm một dấu cách (hết từ).")]
    [SerializeField] private float wordGap = 1.5f;
    [Tooltip("Tự thêm dấu cách vào buffer giải mã khi im lặng vượt wordGap. Tắt nếu dùng kèm MorseChallenge để game tự xử lý dấu cách.")]
    [SerializeField] private bool autoAppendWordSpace = true;

    [Header("UI (TextMeshPro)")]
    [Tooltip("Hiển thị buffer chấm/gạch đang gõ, vd: .-")]
    [SerializeField] private TMP_Text typingText;
    [Tooltip("Hiển thị chuỗi đã giải mã, vd: HELLO")]
    [SerializeField] private TMP_Text resultText;

    [Header("Đèn báo (tùy chọn)")]
    [Tooltip("UI Image sẽ đổi màu khi đang giữ Space.")]
    [SerializeField] private Image indicatorImage;
    [SerializeField] private Color indicatorOnColor = Color.green;
    [SerializeField] private Color indicatorOffColor = Color.gray;

    // Âm thanh morse
    [Header("Audio")]
    [SerializeField] private MorseToneAudio morseToneAudio;

    private static readonly Dictionary<string, char> MorseTable = new Dictionary<string, char>
    {
        { ".-", 'A' },   { "-...", 'B' }, { "-.-.", 'C' }, { "-..", 'D' },  { ".", 'E' },
        { "..-.", 'F' }, { "--.", 'G' },  { "....", 'H' }, { "..", 'I' },   { ".---", 'J' },
        { "-.-", 'K' },  { ".-..", 'L' }, { "--", 'M' },   { "-.", 'N' },   { "---", 'O' },
        { ".--.", 'P' }, { "--.-", 'Q' }, { ".-.", 'R' },  { "...", 'S' },  { "-", 'T' },
        { "..-", 'U' },  { "...-", 'V' }, { ".--", 'W' },  { "-..-", 'X' }, { "-.--", 'Y' },
        { "--..", 'Z' },
        { ".----", '1' }, { "..---", '2' }, { "...--", '3' }, { "....-", '4' }, { ".....", '5' },
        { "-....", '6' }, { "--...", '7' }, { "---..", '8' }, { "----.", '9' }, { "-----", '0' },
    };

    private readonly StringBuilder currentLetter = new StringBuilder();
    private readonly StringBuilder decoded = new StringBuilder();

    private bool isHolding;
    private float holdStartTime;
    private float silenceTimer;
    private bool letterPending;   // buffer đang chờ được chốt thành chữ cái
    private bool wordSpaceAdded;   // đã thêm dấu cách cho lần im lặng này chưa

    /// <summary>Phát mỗi khi một chữ cái được chốt (kể cả '?' khi mã không hợp lệ).</summary>
    public event Action<char> LetterCommitted;

    private void Start()
    {
        UpdateUI();
        SetIndicator(false);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        // Vừa nhấn Space -> bắt đầu đếm thời gian giữ.
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            isHolding = true;
            holdStartTime = Time.time;
            silenceTimer = 0f;
            wordSpaceAdded = false;
            SetIndicator(true);

            // ÂM thanh
            if (morseToneAudio != null)
            {
                morseToneAudio.StartTone();
            }
        }

        // Vừa nhả Space -> đo thời gian giữ rồi thêm chấm hoặc gạch.
        if (keyboard.spaceKey.wasReleasedThisFrame && isHolding)
        {
            isHolding = false;

            //âm thanh
            if (morseToneAudio != null)
            {
                morseToneAudio.StopTone();
            }

            float held = Time.time - holdStartTime;
            currentLetter.Append(held >= dashThreshold ? '-' : '.');
            letterPending = true;
            silenceTimer = 0f;
            SetIndicator(false);
            UpdateUI();
        }

        // Đang nhả tay -> đếm thời gian im lặng để chốt chữ cái / thêm dấu cách.
        if (!isHolding)
        {
            silenceTimer += Time.deltaTime;

            if (letterPending && silenceTimer >= letterGap)
            {
                CommitLetter();
            }

            if (autoAppendWordSpace && !letterPending && !wordSpaceAdded && silenceTimer >= wordGap && decoded.Length > 0)
            {
                if (decoded[decoded.Length - 1] != ' ')
                {
                    decoded.Append(' ');
                    wordSpaceAdded = true;
                    UpdateUI();
                }
            }
        }
    }

    private void CommitLetter()
    {
        string code = currentLetter.ToString();
        char decodedChar = '\0';
        if (code.Length > 0)
        {
            decodedChar = MorseTable.TryGetValue(code, out char c) ? c : '?';
            decoded.Append(decodedChar);
        }

        currentLetter.Clear();
        letterPending = false;
        UpdateUI();

        if (decodedChar != '\0')
        {
            LetterCommitted?.Invoke(decodedChar);
        }
    }

    private void UpdateUI()
    {
        if (typingText != null)
        {
            typingText.text = currentLetter.ToString();
        }

        if (resultText != null)
        {
            resultText.text = decoded.ToString();
        }
    }

    private void SetIndicator(bool on)
    {
        if (indicatorImage != null)
        {
            indicatorImage.color = on ? indicatorOnColor : indicatorOffColor;
        }
    }

    /// <summary>Xóa toàn bộ để gõ lại từ đầu (có thể gọi từ một nút Reset).</summary>
    public void ResetDecoder()
    {
        currentLetter.Clear();
        decoded.Clear();
        letterPending = false;
        wordSpaceAdded = false;
        silenceTimer = 0f;
        UpdateUI();
    }

    /// <summary>Chuỗi đã giải mã hiện tại (để puzzle khác kiểm tra đáp án).</summary>
    public string DecodedText => decoded.ToString();

    // Hàm này để tránh trường hợp người chơi đang giữ Space mà UI Morse bị đóng, âm beep vẫn còn kêu.
    private void OnDisable()
    {
        if (morseToneAudio != null)
        {
            morseToneAudio.StopTone();
        }
    }
}
