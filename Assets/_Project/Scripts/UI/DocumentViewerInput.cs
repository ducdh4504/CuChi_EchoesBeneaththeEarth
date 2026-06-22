using UnityEngine;
using UnityEngine.InputSystem;

public class DocumentViewerInput : MonoBehaviour
{
    private enum DocumentType
    {
        None,
        MorseCode,
        SecretDecree,
        Map
    }

    [Header("References")]
    [SerializeField] private DocumentViewerUI documentViewerUI;
    [SerializeField] private AnInventory playerInventory;

    [Header("Settings")]
    [SerializeField] private bool requireCollectedDocuments = true;

    private DocumentType currentOpenDocument = DocumentType.None;

    private void Start()
    {
        FindReferencesIfNeeded();
    }

    private void Update()
    {
        FindReferencesIfNeeded();

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || documentViewerUI == null)
        {
            return;
        }

        bool morseKeyPressed =
            keyboard.digit1Key.wasPressedThisFrame ||
            keyboard.numpad1Key.wasPressedThisFrame;

        bool secretDecreeKeyPressed =
            keyboard.digit3Key.wasPressedThisFrame ||
            keyboard.numpad3Key.wasPressedThisFrame;

        bool mapKeyPressed =
            keyboard.digit4Key.wasPressedThisFrame ||
            keyboard.numpad4Key.wasPressedThisFrame;

        if (morseKeyPressed)
        {
            HandleDocumentKeyPressed(DocumentType.MorseCode);
            return;
        }

        if (secretDecreeKeyPressed)
        {
            HandleDocumentKeyPressed(DocumentType.SecretDecree);
            return;
        }

        if (mapKeyPressed)
        {
            HandleDocumentKeyPressed(DocumentType.Map);
            return;
        }

        if (documentViewerUI.IsOpen && keyboard.spaceKey.wasPressedThisFrame)
        {
            documentViewerUI.Advance();

            if (!documentViewerUI.IsOpen)
            {
                currentOpenDocument = DocumentType.None;
            }
        }
    }

    private void HandleDocumentKeyPressed(DocumentType requestedDocument)
    {
        if (requestedDocument == DocumentType.None)
        {
            return;
        }

        if (documentViewerUI.IsOpen)
        {
            if (currentOpenDocument == requestedDocument)
            {
                documentViewerUI.Close();
                currentOpenDocument = DocumentType.None;
                return;
            }

            documentViewerUI.Close();
            currentOpenDocument = DocumentType.None;
        }

        if (requireCollectedDocuments && !CanOpenDocument(requestedDocument))
        {
            Debug.Log(GetMissingDocumentMessage(requestedDocument));
            return;
        }

        OpenDocument(requestedDocument);
    }

    private void OpenDocument(DocumentType documentType)
    {
        switch (documentType)
        {
            case DocumentType.MorseCode:
                documentViewerUI.OpenMorseCode();
                currentOpenDocument = DocumentType.MorseCode;
                break;

            case DocumentType.SecretDecree:
                documentViewerUI.OpenSecretDecree();
                currentOpenDocument = DocumentType.SecretDecree;
                break;

            case DocumentType.Map:
                documentViewerUI.OpenMap();
                currentOpenDocument = DocumentType.Map;
                break;
        }
    }

    private bool CanOpenDocument(DocumentType documentType)
    {
        FindReferencesIfNeeded();

        if (playerInventory == null)
        {
            Debug.LogWarning("Cannot check documents because AnInventory was not found.");
            return false;
        }

        switch (documentType)
        {
            case DocumentType.MorseCode:
                return playerInventory.HasMorseCode;

            case DocumentType.SecretDecree:
                return playerInventory.HasSecretDecree;

            case DocumentType.Map:
                return playerInventory.HasSmallMap;

            default:
                return false;
        }
    }

    private string GetMissingDocumentMessage(DocumentType documentType)
    {
        switch (documentType)
        {
            case DocumentType.MorseCode:
                return "Chua co ban huong dan giai ma Morse.";

            case DocumentType.SecretDecree:
                return "Chua co tam sac lenh.";

            case DocumentType.Map:
                return "Chua co tam ban do.";

            default:
                return "Chua co tai lieu nay.";
        }
    }

    private void FindReferencesIfNeeded()
    {
        if (documentViewerUI == null)
        {
            documentViewerUI = Object.FindAnyObjectByType<DocumentViewerUI>(FindObjectsInactive.Include);
        }

        if (playerInventory == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerInventory = player.GetComponent<AnInventory>();
            }
        }
    }
}
