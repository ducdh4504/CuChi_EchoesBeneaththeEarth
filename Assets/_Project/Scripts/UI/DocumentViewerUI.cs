//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class DocumentViewerUI : MonoBehaviour
//{
//    private enum DocumentPage
//    {
//        None,
//        MorseCode,
//        SecretDecree
//    }

//    [Header("Data")]
//    [SerializeField] private StoryDiscoveryData documentData;

//    [Header("Root")]
//    [SerializeField] private GameObject panelRoot;
//    [SerializeField] private CanvasGroup canvasGroup;

//    [Header("Panels")]
//    [SerializeField] private GameObject backgroundDim;
//    [SerializeField] private GameObject contentPanel;
//    [SerializeField] private GameObject monologuePromptPanel;

//    [Header("Texts")]
//    [SerializeField] private TMP_Text titleText;
//    [SerializeField] private TMP_Text continueHintText;

//    [Header("Images")]
//    [SerializeField] private Image secretLetterImage;
//    [SerializeField] private Image morseCodeImage;

//    [Header("Display Text")]
//    [SerializeField] private string morseCodeTitle = "Bảng hướng dẫn mã Morse";
//    [SerializeField] private string secretDecreeTitle = "Tấm sắc lệnh";

//    private DocumentPage currentPage = DocumentPage.None;

//    public static bool IsAnyViewerOpen { get; private set; }

//    private static int blockGameplaySpaceFrame = -1;

//    public static bool ShouldBlockGameplaySpace =>
//        IsAnyViewerOpen || blockGameplaySpaceFrame == Time.frameCount;

//    public bool IsOpen => currentPage != DocumentPage.None;

//    private void Awake()
//    {
//        HideImmediate();
//    }

//    public void OpenMorseCode()
//    {
//        if (documentData == null)
//        {
//            Debug.LogWarning("Cannot open document viewer because StoryDiscoveryData is not assigned.");
//            return;
//        }

//        if (panelRoot != null)
//        {
//            panelRoot.SetActive(true);
//        }

//        SetCanvasVisible(true);
//        SetPage(DocumentPage.MorseCode);

//        IsAnyViewerOpen = true;
//    }

//    public void Advance()
//    {
//        BlockGameplaySpaceThisFrame();

//        switch (currentPage)
//        {
//            case DocumentPage.MorseCode:
//                SetPage(DocumentPage.SecretDecree);
//                break;

//            case DocumentPage.SecretDecree:
//                Close();
//                break;
//        }
//    }

//    public void Close()
//    {
//        BlockGameplaySpaceThisFrame();
//        HideImmediate();
//    }

//    private void SetPage(DocumentPage page)
//    {
//        currentPage = page;

//        bool showMorseCode = page == DocumentPage.MorseCode;
//        bool showSecretDecree = page == DocumentPage.SecretDecree;

//        SetActive(backgroundDim, true);
//        SetActive(contentPanel, true);
//        SetActive(monologuePromptPanel, false);

//        if (titleText != null)
//        {
//            if (showMorseCode)
//            {
//                titleText.text = morseCodeTitle;
//            }
//            else if (showSecretDecree)
//            {
//                titleText.text = secretDecreeTitle;
//            }
//        }

//        if (continueHintText != null)
//        {
//            if (showMorseCode)
//            {
//                continueHintText.text = "Space: xem mảnh sắc lệnh  |  1: đóng";
//            }
//            else if (showSecretDecree)
//            {
//                continueHintText.text = "Space: đóng  |  1: đóng";
//            }
//        }

//        if (morseCodeImage != null)
//        {
//            morseCodeImage.sprite = documentData.morseCodeImage;
//            morseCodeImage.preserveAspect = true;
//            morseCodeImage.gameObject.SetActive(showMorseCode && morseCodeImage.sprite != null);
//        }

//        if (secretLetterImage != null)
//        {
//            secretLetterImage.sprite = documentData.secretLetterImage;
//            secretLetterImage.preserveAspect = true;
//            secretLetterImage.gameObject.SetActive(showSecretDecree && secretLetterImage.sprite != null);
//        }
//    }

//    private void HideImmediate()
//    {
//        currentPage = DocumentPage.None;
//        IsAnyViewerOpen = false;

//        if (panelRoot != null)
//        {
//            panelRoot.SetActive(false);
//        }

//        SetCanvasVisible(false);

//        SetActive(backgroundDim, false);
//        SetActive(contentPanel, false);
//        SetActive(monologuePromptPanel, false);

//        if (secretLetterImage != null)
//        {
//            secretLetterImage.gameObject.SetActive(false);
//        }

//        if (morseCodeImage != null)
//        {
//            morseCodeImage.gameObject.SetActive(false);
//        }
//    }

//    private void SetCanvasVisible(bool visible)
//    {
//        if (canvasGroup == null)
//        {
//            return;
//        }

//        canvasGroup.alpha = visible ? 1f : 0f;
//        canvasGroup.interactable = visible;
//        canvasGroup.blocksRaycasts = visible;
//    }

//    private static void SetActive(GameObject target, bool active)
//    {
//        if (target != null)
//        {
//            target.SetActive(active);
//        }
//    }

//    private static void BlockGameplaySpaceThisFrame()
//    {
//        blockGameplaySpaceFrame = Time.frameCount;
//    }
//}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocumentViewerUI : MonoBehaviour
{
    private enum DocumentPage
    {
        None,
        MorseCode,
        SecretDecree
    }

    [Header("Data")]
    [SerializeField] private StoryDiscoveryData documentData;

    [Header("Root")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Panels")]
    [SerializeField] private GameObject backgroundDim;
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private GameObject monologuePromptPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text continueHintText;

    [Header("Images")]
    [SerializeField] private Image secretLetterImage;
    [SerializeField] private Image morseCodeImage;

    [Header("Display Text")]
    [SerializeField] private string morseCodeTitle = "Bảng hướng dẫn mã Morse";
    [SerializeField] private string secretDecreeTitle = "Tấm sắc lệnh";

    private DocumentPage currentPage = DocumentPage.None;

    public static bool IsAnyViewerOpen { get; private set; }

    private static int blockGameplaySpaceFrame = -1;

    public static bool ShouldBlockGameplaySpace =>
        IsAnyViewerOpen || blockGameplaySpaceFrame == Time.frameCount;

    public bool IsOpen => currentPage != DocumentPage.None;

    private void Awake()
    {
        HideImmediate();
    }

    public void OpenMorseCode()
    {
        OpenDocument(DocumentPage.MorseCode);
    }

    public void OpenSecretDecree()
    {
        OpenDocument(DocumentPage.SecretDecree);
    }

    public void Advance()
    {
        // Hiện tại Space chỉ dùng để đóng tài liệu đang xem.
        // Không chuyển từ Morse sang sắc lệnh nữa.
        Close();
    }

    public void Close()
    {
        BlockGameplaySpaceThisFrame();
        HideImmediate();
    }

    private void OpenDocument(DocumentPage page)
    {
        if (documentData == null)
        {
            Debug.LogWarning("Cannot open document viewer because StoryDiscoveryData is not assigned.");
            return;
        }

        if (page == DocumentPage.MorseCode && documentData.morseCodeImage == null)
        {
            Debug.LogWarning("Cannot open Morse Code because morseCodeImage is not assigned in StoryDiscoveryData.");
            return;
        }

        if (page == DocumentPage.SecretDecree && documentData.secretLetterImage == null)
        {
            Debug.LogWarning("Cannot open Secret Decree because secretLetterImage is not assigned in StoryDiscoveryData.");
            return;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        SetCanvasVisible(true);
        SetPage(page);

        IsAnyViewerOpen = true;
    }

    private void SetPage(DocumentPage page)
    {
        currentPage = page;

        bool showMorseCode = page == DocumentPage.MorseCode;
        bool showSecretDecree = page == DocumentPage.SecretDecree;

        SetActive(backgroundDim, true);
        SetActive(contentPanel, true);
        SetActive(monologuePromptPanel, false);

        if (titleText != null)
        {
            if (showMorseCode)
            {
                titleText.text = morseCodeTitle;
            }
            else if (showSecretDecree)
            {
                titleText.text = secretDecreeTitle;
            }
        }

        if (continueHintText != null)
        {
            if (showMorseCode)
            {
                continueHintText.text = "Space / phím 1: đóng";
            }
            else if (showSecretDecree)
            {
                continueHintText.text = "Space / phím 3: đóng";
            }
        }

        if (morseCodeImage != null)
        {
            morseCodeImage.sprite = documentData.morseCodeImage;
            morseCodeImage.preserveAspect = true;
            morseCodeImage.gameObject.SetActive(showMorseCode && morseCodeImage.sprite != null);
        }

        if (secretLetterImage != null)
        {
            secretLetterImage.sprite = documentData.secretLetterImage;
            secretLetterImage.preserveAspect = true;
            secretLetterImage.gameObject.SetActive(showSecretDecree && secretLetterImage.sprite != null);
        }
    }

    private void HideImmediate()
    {
        currentPage = DocumentPage.None;
        IsAnyViewerOpen = false;

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

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private static void BlockGameplaySpaceThisFrame()
    {
        blockGameplaySpaceFrame = Time.frameCount;
    }
}