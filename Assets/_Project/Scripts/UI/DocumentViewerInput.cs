using UnityEngine;
using UnityEngine.InputSystem;

public class DocumentViewerInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DocumentViewerUI documentViewerUI;
    [SerializeField] private AnInventory playerInventory;

    [Header("Settings")]
    [SerializeField] private bool requireCollectedDocuments = true;

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

        bool documentKeyPressed =
            keyboard.digit1Key.wasPressedThisFrame ||
            keyboard.numpad1Key.wasPressedThisFrame;

        if (documentKeyPressed)
        {
            HandleDocumentKeyPressed();
            return;
        }

        if (documentViewerUI.IsOpen && keyboard.spaceKey.wasPressedThisFrame)
        {
            documentViewerUI.Advance();
        }
    }

    private void HandleDocumentKeyPressed()
    {
        if (documentViewerUI.IsOpen)
        {
            documentViewerUI.Close();
            return;
        }

        if (requireCollectedDocuments && !CanOpenDocuments())
        {
            Debug.Log("Chưa có đủ tài liệu: cần bảng mã Morse và mảnh sắc lệnh.");
            return;
        }

        documentViewerUI.OpenMorseCode();
    }

    private bool CanOpenDocuments()
    {
        FindReferencesIfNeeded();

        if (playerInventory == null)
        {
            Debug.LogWarning("Cannot check documents because AnInventory was not found.");
            return false;
        }

        return playerInventory.HasMorsePuzzleDocuments;
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