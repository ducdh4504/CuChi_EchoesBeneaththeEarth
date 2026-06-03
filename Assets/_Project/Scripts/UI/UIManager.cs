using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private AnOxygen anOxygen;

    [Header("UI References")]
    [SerializeField] private OxygenBarUI oxygenBarUI;

    private void Awake()
    {
        FindReferencesIfNeeded();
    }

    private void OnEnable()
    {
        FindReferencesIfNeeded();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Start()
    {
        UpdateOxygenBar();
    }

    private void SubscribeEvents()
    {
        if (anOxygen == null)
        {
            return;
        }

        anOxygen.OnOxygenChanged += HandleOxygenChanged;
        anOxygen.OnOutOfOxygen += HandleOutOfOxygen;
    }

    private void UnsubscribeEvents()
    {
        if (anOxygen == null)
        {
            return;
        }

        anOxygen.OnOxygenChanged -= HandleOxygenChanged;
        anOxygen.OnOutOfOxygen -= HandleOutOfOxygen;
    }

    private void HandleOxygenChanged(float currentOxygen, float maxOxygen, float oxygenPercent)
    {
        if (oxygenBarUI != null)
        {
            oxygenBarUI.UpdateOxygenBar(currentOxygen, maxOxygen, oxygenPercent);
        }
    }

    private void HandleOutOfOxygen()
    {
        Debug.Log("UIManager received: Player is out of oxygen.");
    }

    private void UpdateOxygenBar()
    {
        if (anOxygen == null || oxygenBarUI == null)
        {
            return;
        }

        oxygenBarUI.UpdateOxygenBar(
            anOxygen.CurrentOxygen,
            anOxygen.MaxOxygen,
            anOxygen.OxygenPercent);
    }

    private void FindReferencesIfNeeded()
    {
        if (anOxygen == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                anOxygen = player.GetComponent<AnOxygen>();
            }
        }

        if (oxygenBarUI == null)
        {
            oxygenBarUI = FindAnyObjectByType<OxygenBarUI>();
        }
    }
}
