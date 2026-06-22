using UnityEngine;

public class MapPickupInteractable : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [SerializeField] private string promptText = "Nhấn E để nhặt bản đồ";
    [SerializeField] private DialogueData pickupDialogue;
    [SerializeField] private GameObject visualToHideOnPickup;
    [SerializeField] private AnInventory inventory;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Range(0f, 1f)] private float pickupSfxVolume = 0.8f;
    [SerializeField] private bool playMapFoundMusicOnPickup = true;
    [SerializeField] private float mapFoundMusicFadeDuration = 2f;

    private bool pickedUp;

    public string GetInteractPrompt() => promptText;

    public bool CanInteract() => !pickedUp;

    public void Interact()
    {
        if (pickedUp) return;
        pickedUp = true;

        PlayPickupAudio();

        var inv = inventory != null ? inventory : FindInventory();
        if (inv != null) inv.SetHasMap(true);

        if (visualToHideOnPickup != null) visualToHideOnPickup.SetActive(false);
        else gameObject.SetActive(false);

        if (pickupDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(pickupDialogue);
        }
    }

    private void PlayPickupAudio()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        if (pickupSfx != null)
        {
            AudioManager.Instance.PlaySFX(pickupSfx, pickupSfxVolume);
        }

        if (playMapFoundMusicOnPickup)
        {
            AudioManager.Instance.PlayMapFoundMusic(mapFoundMusicFadeDuration);
        }
    }

    private static AnInventory FindInventory()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        return p != null ? p.GetComponent<AnInventory>() : null;
    }
}
