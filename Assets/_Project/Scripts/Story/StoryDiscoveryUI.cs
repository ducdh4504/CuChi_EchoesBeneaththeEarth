using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StoryDiscoveryUI : MonoBehaviour
{
    private enum DiscoveryStep
    {
        None,
        Monologue,
        SecretLetter,
        MorseCode
    }

    [Header("Root")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Panels")]
    [SerializeField] private GameObject backgroundDim;
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private GameObject monologuePromptPanel;

    [Header("Monologue Prompt")]
    [SerializeField] private TMP_Text monologuePromptText;
    [SerializeField] private TMP_Text monologuePromptHintText;

    [Header("Content Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text continueHintText;

    [Header("Images")]
    [SerializeField] private Image secretLetterImage;
    [SerializeField] private Image morseCodeImage;

    [Header("Input")]
    [SerializeField] private float inputDelayAfterShow = 0.2f;

    //Audio
    [Header("Audio")]
    [SerializeField] private AudioClip paperUseSfx;
    [SerializeField, Range(0f, 1f)] private float paperUseSfxVolume = 0.8f;

    private StoryDiscoveryData currentData;
    private DiscoveryStep currentStep = DiscoveryStep.None;
    private bool isShowing;
    private float canAdvanceAtTime;
    private Action onClosed;

    public bool IsShowing => isShowing;

    private void Awake()
    {
        HideImmediate();
    }

    private void Update()
    {
        if (!isShowing)
        {
            return;
        }

        if (Time.time < canAdvanceAtTime)
        {
            return;
        }

        if (WasAdvancePressed())
        {
            AdvanceStep();
        }
    }

    public void Show(StoryDiscoveryData data, Action closedCallback = null)
    {
        if (data == null)
        {
            Debug.LogWarning("Cannot show story discovery because data is null.");
            return;
        }

        currentData = data;
        onClosed = closedCallback;
        isShowing = true;
        canAdvanceAtTime = Time.time + inputDelayAfterShow;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        SetCanvasVisible(true);

        SetImageSprite(secretLetterImage, data.secretLetterImage);
        SetImageSprite(morseCodeImage, data.morseCodeImage);

        SetStep(DiscoveryStep.Monologue);
    }

    private void AdvanceStep()
    {
        canAdvanceAtTime = Time.time + inputDelayAfterShow;

        switch (currentStep)
        {
            case DiscoveryStep.Monologue:
                SetStep(DiscoveryStep.SecretLetter);
                break;

            case DiscoveryStep.SecretLetter:
                SetStep(DiscoveryStep.MorseCode);
                break;

            case DiscoveryStep.MorseCode:
                Hide();
                break;
        }
    }

    private void SetStep(DiscoveryStep step)
    {
        currentStep = step;

        bool showMonologue = step == DiscoveryStep.Monologue;
        bool showSecretLetter = step == DiscoveryStep.SecretLetter;
        bool showMorseCode = step == DiscoveryStep.MorseCode;
        bool showImageContent = showSecretLetter || showMorseCode;

        SetActive(backgroundDim, showImageContent);
        SetActive(contentPanel, showImageContent);
        SetActive(monologuePromptPanel, showMonologue);

        if (monologuePromptText != null)
        {
            monologuePromptText.text = currentData.monologue;
        }

        if (monologuePromptHintText != null)
        {
            monologuePromptHintText.text = "Nhấn E / Space để xem mật lệnh";
        }

        if (titleText != null)
        {
            if (showSecretLetter)
            {
                titleText.text = "Mật lệnh bí mật";
            }
            else if (showMorseCode)
            {
                titleText.text = "Bảng mã Morse";
            }
        }

        if (continueHintText != null)
        {
            if (showSecretLetter)
            {
                continueHintText.text = "Nhấn E / Space để xem bảng mã Morse";
            }
            else if (showMorseCode)
            {
                continueHintText.text = "Nhấn E / Space để tiếp tục";
            }
        }

        if (secretLetterImage != null)
        {
            secretLetterImage.gameObject.SetActive(showSecretLetter && secretLetterImage.sprite != null);
        }

        if (morseCodeImage != null)
        {
            morseCodeImage.gameObject.SetActive(showMorseCode && morseCodeImage.sprite != null);
        }
        PlayStepAudio(step);
    }
    //Audio
    private void PlayStepAudio(DiscoveryStep step)
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        if (step == DiscoveryStep.SecretLetter || step == DiscoveryStep.MorseCode)
        {
            AudioManager.Instance.PlaySFX(paperUseSfx, paperUseSfxVolume);
        }
    }

    public void Hide()
    {
        if (!isShowing)
        {
            return;
        }

        isShowing = false;
        currentStep = DiscoveryStep.None;

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        SetCanvasVisible(false);

        Action callback = onClosed;
        onClosed = null;
        currentData = null;

        callback?.Invoke();
    }

    private void HideImmediate()
    {
        isShowing = false;
        currentStep = DiscoveryStep.None;

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        SetCanvasVisible(false);

        SetActive(backgroundDim, false);
        SetActive(contentPanel, false);
        SetActive(monologuePromptPanel, false);

        if (secretLetterImage != null)
        {
            secretLetterImage.gameObject.SetActive(false);
        }

        if (morseCodeImage != null)
        {
            morseCodeImage.gameObject.SetActive(false);
        }
    }

    private void SetCanvasVisible(bool visible)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private static void SetImageSprite(Image targetImage, Sprite sprite)
    {
        if (targetImage == null)
        {
            return;
        }

        targetImage.sprite = sprite;
        targetImage.preserveAspect = true;
        targetImage.gameObject.SetActive(false);
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private static bool WasAdvancePressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (
                keyboard.eKey.wasPressedThisFrame ||
                keyboard.spaceKey.wasPressedThisFrame ||
                keyboard.enterKey.wasPressedThisFrame
            )
            {
                return true;
            }
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }
}