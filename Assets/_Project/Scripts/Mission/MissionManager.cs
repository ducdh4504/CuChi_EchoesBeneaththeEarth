using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }
    public static event Action<string, string> ObjectiveChanged;

    [Header("UI")]
    [SerializeField] private ObjectivePanelUI objectivePanelUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        FindObjectivePanelIfNeeded();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        objectivePanelUI = null;
        FindObjectivePanelIfNeeded();
        RefreshObjectiveUI();
    }

    public void StartMainMissionIfNeeded()
    {
        if (RuntimeMissionState.GetMissionState(MissionIds.Main_FindLostMap) == MissionState.NotStarted)
        {
            RuntimeMissionState.SetMissionState(MissionIds.Main_FindLostMap, MissionState.Active);
        }
    }

    public void StartMission(string missionId)
    {
        if (string.IsNullOrWhiteSpace(missionId))
        {
            return;
        }

        StartMainMissionIfNeeded();

        RuntimeMissionState.SetMissionState(missionId, MissionState.Active);
        RuntimeMissionState.SetCurrentMission(missionId);
        RuntimeMissionState.SetCurrentObjective(GetDefaultObjective(missionId));

        RefreshObjectiveUI();

        Debug.Log($"Mission started: {missionId}");
    }

    public void CompleteMission(string missionId)
    {
        if (string.IsNullOrWhiteSpace(missionId))
        {
            return;
        }

        RuntimeMissionState.SetMissionState(missionId, MissionState.Completed);

        if (missionId == RuntimeMissionState.CurrentMissionId)
        {
            RuntimeMissionState.SetCurrentObjective($"Hoàn thành: {GetMissionTitle(missionId)}");
        }

        RefreshObjectiveUI();

        Debug.Log($"Mission completed: {missionId}");
    }

    public void SetMainObjective(string objective)
    {
        StartMainMissionIfNeeded();

        RuntimeMissionState.SetCurrentMission(MissionIds.Main_FindLostMap);
        RuntimeMissionState.SetCurrentObjective(objective);

        RefreshObjectiveUI();
    }

    public void SetCurrentObjective(string objective)
    {
        RuntimeMissionState.SetCurrentObjective(objective);
        RefreshObjectiveUI();
    }

    public bool IsMissionCompleted(string missionId)
    {
        return RuntimeMissionState.GetMissionState(missionId) == MissionState.Completed;
    }

public void HandleObjectiveId(string objectiveId)
    {
        if (string.IsNullOrWhiteSpace(objectiveId))
        {
            return;
        }

        string resolvedObjectiveId = ResolveObjectiveIdAlias(objectiveId.Trim());

        switch (resolvedObjectiveId)
        {
            case MissionIds.Main_FindLostMap:
                SetMainObjective("Tìm manh mối liên quan đến tấm bản đồ thất lạc."); 
                break;

            case MissionIds.Day1_FindTinBox:
                StartMission(MissionIds.Day1_FindTinBox);
                break;

            case MissionIds.Day1_MorseTransmission:
                StartMission(MissionIds.Day1_MorseTransmission);
                break;

            case MissionIds.Day2_MorseTransmission:
                StartMission(MissionIds.Day2_MorseTransmission);
                break;

            case MissionIds.Day3_FindMap:
                StartMission(MissionIds.Day3_FindMap);
                break;

            case MissionIds.Day3_ReturnMap:
                RuntimeMissionState.SetCurrentMission(MissionIds.Day3_FindMap);
                RuntimeMissionState.SetMissionState(MissionIds.Day3_FindMap, MissionState.Active);
                RuntimeMissionState.SetCurrentObjective("Mang tấm bản đồ lên mặt đất và giao cho Giao liên kỳ cựu.");
                RefreshObjectiveUI(); 
                break;

            case MissionIds.Day3_Complete:
                RuntimeMissionState.SetMissionState(MissionIds.Day3_FindMap, MissionState.Completed);
                RuntimeMissionState.SetMissionState(MissionIds.Main_FindLostMap, MissionState.Completed);
                RuntimeMissionState.SetCurrentMission(MissionIds.Main_FindLostMap);
                RuntimeMissionState.SetCurrentObjective("Đã giao lại tấm bản đồ cho Giao liên kỳ cựu. Nhiệm vụ hoàn thành.");
                RefreshObjectiveUI(); 
                break;

            default:
                StartCustomMission(resolvedObjectiveId);
                break;
        }
    }

private void StartCustomMission(string missionId)
    {
        StartMainMissionIfNeeded();

        RuntimeMissionState.SetMissionState(missionId, MissionState.Active);
        RuntimeMissionState.SetCurrentMission(missionId);

        string objective = GetDefaultObjective(missionId);
        RuntimeMissionState.SetCurrentObjective(
            string.IsNullOrWhiteSpace(objective) ? FormatObjectiveId(missionId) : objective);

        RefreshObjectiveUI();

        Debug.Log($"Custom mission started: {missionId}");
    }


    private void RefreshObjectiveUI()
    {
        string missionId = RuntimeMissionState.CurrentMissionId;
        string objective = RuntimeMissionState.CurrentObjective;

        ObjectiveChanged?.Invoke(missionId, objective);

        FindObjectivePanelIfNeeded();

        if (objectivePanelUI == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(missionId) && string.IsNullOrWhiteSpace(objective))
        {
            objectivePanelUI.ClearObjective();
            return;
        }

        string missionTitle = GetMissionTitle(missionId);
        MissionState state = RuntimeMissionState.GetMissionState(missionId);

        objectivePanelUI.SetMission(missionTitle, objective, state);
    }

    private void FindObjectivePanelIfNeeded()
    {
        if (objectivePanelUI != null)
        {
            return;
        }

        objectivePanelUI = UnityEngine.Object.FindAnyObjectByType<ObjectivePanelUI>(FindObjectsInactive.Include);
    }

private string GetMissionTitle(string missionId)
    {
        switch (missionId)
        {
            case MissionIds.Main_FindLostMap:
                return "Tìm tấm bản đồ thất lạc";

            case MissionIds.Day1_FindTinBox:
                return "Tìm manh mối đầu tiên";

            case MissionIds.Day1_MorseTransmission:
                return "Truyền tin bằng mã Morse";

            case MissionIds.Day2_MorseTransmission:
                return "Truyền tin bằng mã Morse";

            case MissionIds.Day3_FindMap:
                return "Tìm lại tấm bản đồ";

            default:
                return "Nhiệm vụ hiện tại";
        }
    }

private string GetDefaultObjective(string missionId)
    {
        switch (missionId)
        {
            case MissionIds.Day1_FindTinBox:
                return "Tìm hộp thiếc cũ trong địa đạo."; 

            case MissionIds.Day1_MorseTransmission:
                return "Tìm phòng truyền tin và gửi nội dung bằng mã Morse.";

            case MissionIds.Day2_MorseTransmission:
                return "Tìm phòng truyền tin và gửi nội dung sắc lệnh bằng mã Morse."; 

            case MissionIds.Day3_FindMap:
                return "Tìm phòng họp và lấy lại tấm bản đồ thất lạc."; 

            case MissionIds.Main_FindLostMap:
                return "Tìm manh mối liên quan đến tấm bản đồ thất lạc."; 

            default:
                return string.Empty;
        }
    }
    public void ClearObjective()
    {
        RuntimeMissionState.SetCurrentMission(null);
        RuntimeMissionState.SetCurrentObjective(null);
        RefreshObjectiveUI();
    }


private static string ResolveObjectiveIdAlias(string objectiveId)
    {
        if (string.Equals(objectiveId, MissionIds.Day3_FindMap, StringComparison.OrdinalIgnoreCase))
        {
            return MissionIds.Day3_FindMap;
        }

        return objectiveId;
    }

    private static string FormatObjectiveId(string objectiveId)
    {
        if (string.IsNullOrWhiteSpace(objectiveId))
        {
            return string.Empty;
        }

        return objectiveId.Replace('_', ' ');
    }
}
