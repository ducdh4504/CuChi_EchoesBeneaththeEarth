using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocumentViewerUI : MonoBehaviour
{
    private enum DocumentPage
    {
        None,
        MorseCode,
        SecretDecree,
        Map
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

    [Header("Map")]
    [SerializeField] private Sprite mapImageSprite;
    [SerializeField] private Image mapImage;

    [Header("Display Text")]
    [SerializeField] private string morseCodeTitle = "Bang huong dan ma Morse";
    [SerializeField] private string secretDecreeTitle = "Tam sac lenh";
    [SerializeField] private string mapTitle = "Tam ban do dia dao";

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

    public void OpenMap()
    {
        OpenDocument(DocumentPage.Map);
    }

    public void Advance()
    {
        Close();
    }

    public void Close()
    {
        BlockGameplaySpaceThisFrame();
        HideImmediate();
    }

    private void OpenDocument(DocumentPage page)
    {
        if (!CanOpenPage(page))
        {
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

    private bool CanOpenPage(DocumentPage page)
    {
        switch (page)
        {
            case DocumentPage.MorseCode:
                if (documentData == null || documentData.morseCodeImage == null)
                {
                    Debug.LogWarning("Cannot open Morse Code because morseCodeImage is not assigned.");
                    return false;
                }
                return true;

            case DocumentPage.SecretDecree:
                if (documentData == null || documentData.secretLetterImage == null)
                {
                    Debug.LogWarning("Cannot open Secret Decree because secretLetterImage is not assigned.");
                    return false;
                }
                return true;

            case DocumentPage.Map:
                if (mapImageSprite == null)
                {
                    Debug.LogWarning("Cannot open Map because mapImageSprite is not assigned.");
                    return false;
                }

                if (mapImage == null)
                {
                    Debug.LogWarning("Cannot open Map because mapImage Image is not assigned.");
                    return false;
                }
                return true;

            default:
                return false;
        }
    }

    private void SetPage(DocumentPage page)
    {
        currentPage = page;

        bool showMorseCode = page == DocumentPage.MorseCode;
        bool showSecretDecree = page == DocumentPage.SecretDecree;
        bool showMap = page == DocumentPage.Map;

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
            else if (showMap)
            {
                titleText.text = mapTitle;
            }
        }

        if (continueHintText != null)
        {
            if (showMorseCode)
            {
                continueHintText.text = "Space / phim 1: dong";
            }
            else if (showSecretDecree)
            {
                continueHintText.text = "Space / phim 3: dong";
            }
            else if (showMap)
            {
                continueHintText.text = "Space / phim 4: dong";
            }
        }

        if (morseCodeImage != null)
        {
            morseCodeImage.sprite = documentData != null ? documentData.morseCodeImage : null;
            morseCodeImage.preserveAspect = true;
            morseCodeImage.gameObject.SetActive(showMorseCode && morseCodeImage.sprite != null);
        }

        if (secretLetterImage != null)
        {
            secretLetterImage.sprite = documentData != null ? documentData.secretLetterImage : null;
            secretLetterImage.preserveAspect = true;
            secretLetterImage.gameObject.SetActive(showSecretDecree && secretLetterImage.sprite != null);
        }

        if (mapImage != null)
        {
            mapImage.sprite = mapImageSprite;
            mapImage.preserveAspect = true;
            mapImage.gameObject.SetActive(showMap && mapImage.sprite != null);
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

        if (mapImage != null)
        {
            mapImage.gameObject.SetActive(false);
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
