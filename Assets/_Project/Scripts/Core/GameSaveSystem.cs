using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSaveSystem
{
    private const string SaveExistsKey = "Save.Exists";
    private const string SceneKey = "Save.Scene";
    private const string HasSmallMapKey = "Save.Inventory.HasSmallMap";
    private const string HasMorseCodeKey = "Save.Inventory.HasMorseCode";
    private const string HasSecretDecreeKey = "Save.Inventory.HasSecretDecree";
    private const string HasFlashlightKey = "Save.Inventory.HasFlashlight";
    private const string MapFragmentCountKey = "Save.Inventory.MapFragmentCount";
    private const string CurrentMissionKey = "Save.Mission.CurrentMission";
    private const string CurrentObjectiveKey = "Save.Mission.CurrentObjective";
    private const string MissionStatePrefix = "Save.Mission.State.";

    private static readonly string[] SavedMissionIds =
    {
        MissionIds.Main_FindLostMap,
        MissionIds.Day1_FindTinBox,
        MissionIds.Day2_MorseTransmission,
        MissionIds.Day3_FindMap
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadOnStartup()
    {
        LoadRuntimeState();
    }

    public static bool HasSave => PlayerPrefs.GetInt(SaveExistsKey, 0) == 1;

    public static string SavedSceneName
    {
        get
        {
            string sceneName = PlayerPrefs.GetString(SceneKey, string.Empty);
            return string.IsNullOrWhiteSpace(sceneName) ? "Day1" : sceneName;
        }
    }

    public static void SaveCurrentGame()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!IsGameplayScene(activeScene.name))
        {
            return;
        }

        PlayerPrefs.SetInt(SaveExistsKey, 1);
        PlayerPrefs.SetString(SceneKey, activeScene.name);

        PlayerPrefs.SetInt(HasSmallMapKey, RuntimeInventoryState.HasSmallMap ? 1 : 0);
        PlayerPrefs.SetInt(HasMorseCodeKey, RuntimeInventoryState.HasMorseCode ? 1 : 0);
        PlayerPrefs.SetInt(HasSecretDecreeKey, RuntimeInventoryState.HasSecretDecree ? 1 : 0);
        PlayerPrefs.SetInt(HasFlashlightKey, RuntimeInventoryState.HasFlashlight ? 1 : 0);
        PlayerPrefs.SetInt(MapFragmentCountKey, RuntimeInventoryState.MapFragmentCount);

        PlayerPrefs.SetString(CurrentMissionKey, RuntimeMissionState.CurrentMissionId ?? string.Empty);
        PlayerPrefs.SetString(CurrentObjectiveKey, RuntimeMissionState.CurrentObjective ?? string.Empty);

        foreach (string missionId in SavedMissionIds)
        {
            PlayerPrefs.SetInt(MissionStatePrefix + missionId, (int)RuntimeMissionState.GetMissionState(missionId));
        }

        PlayerPrefs.Save();
    }

    public static void LoadRuntimeState()
    {
        if (!HasSave)
        {
            return;
        }

        RuntimeInventoryState.SetState(
            PlayerPrefs.GetInt(HasSmallMapKey, 0) == 1,
            PlayerPrefs.GetInt(HasMorseCodeKey, 0) == 1,
            PlayerPrefs.GetInt(HasSecretDecreeKey, 0) == 1,
            PlayerPrefs.GetInt(HasFlashlightKey, 0) == 1,
            PlayerPrefs.GetInt(MapFragmentCountKey, 0));

        RuntimeMissionState.ResetAll();
        foreach (string missionId in SavedMissionIds)
        {
            MissionState state = (MissionState)PlayerPrefs.GetInt(
                MissionStatePrefix + missionId,
                (int)MissionState.NotStarted);
            RuntimeMissionState.SetMissionState(missionId, state);
        }

        string currentMission = PlayerPrefs.GetString(CurrentMissionKey, string.Empty);
        string currentObjective = PlayerPrefs.GetString(CurrentObjectiveKey, string.Empty);
        RuntimeMissionState.SetCurrentProgress(
            string.IsNullOrWhiteSpace(currentMission) ? null : currentMission,
            string.IsNullOrWhiteSpace(currentObjective) ? null : currentObjective);
    }

    public static void LoadSavedGameOrNewGame()
    {
        LoadRuntimeState();
        SceneLoader.Load(HasSave ? SavedSceneName : "Day1");
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveExistsKey);
        PlayerPrefs.DeleteKey(SceneKey);
        PlayerPrefs.DeleteKey(HasSmallMapKey);
        PlayerPrefs.DeleteKey(HasMorseCodeKey);
        PlayerPrefs.DeleteKey(HasSecretDecreeKey);
        PlayerPrefs.DeleteKey(HasFlashlightKey);
        PlayerPrefs.DeleteKey(MapFragmentCountKey);
        PlayerPrefs.DeleteKey(CurrentMissionKey);
        PlayerPrefs.DeleteKey(CurrentObjectiveKey);

        foreach (string missionId in SavedMissionIds)
        {
            PlayerPrefs.DeleteKey(MissionStatePrefix + missionId);
        }

        PlayerPrefs.Save();
        RuntimeInventoryState.Reset();
        RuntimeMissionState.ResetAll();
    }

    private static bool IsGameplayScene(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(sceneName) && sceneName.StartsWith("Day");
    }
}
