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
    private const string CheckpointExistsKey = "Save.Checkpoint.Exists";
    private const string CheckpointPositionXKey = "Save.Checkpoint.PositionX";
    private const string CheckpointPositionYKey = "Save.Checkpoint.PositionY";
    private const string CheckpointPositionZKey = "Save.Checkpoint.PositionZ";
    private const string OxygenKey = "Save.Player.Oxygen";
    private const string RestoreOxygenOnContinueKey = "Save.Player.RestoreOxygenOnContinue";
    private const string LevelUnlockedPrefix = "Save.LevelUnlocked.";

    private static readonly string[] SavedMissionIds =
    {
        MissionIds.Main_FindLostMap,
        MissionIds.Day1_FindTinBox,
        MissionIds.Day2_MorseTransmission,
        MissionIds.Day3_FindMap
    };

    private static readonly string[] GameplaySceneNames =
    {
        "Terrain",
        "Day1",
        "Day2",
        "Day3"
    };

    private static bool loadingFromSave;
    private static bool hasRuntimeOxygen;
    private static float runtimeOxygen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Application.quitting -= SaveCurrentGame;
        Application.quitting += SaveCurrentGame;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        LoadRuntimeState();
    }

    public static bool HasSave => PlayerPrefs.GetInt(SaveExistsKey, 0) == 1;
    public static bool HasCheckpoint => PlayerPrefs.GetInt(CheckpointExistsKey, 0) == 1;

    public static string SavedSceneName
    {
        get
        {
            string sceneName = PlayerPrefs.GetString(SceneKey, string.Empty);
            return string.IsNullOrWhiteSpace(sceneName) ? "Terrain" : sceneName;
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
        UnlockLevel(activeScene.name, saveImmediately: false);
        SavePlayerCheckpointIfAvailable();
        SavePlayerOxygenIfAvailable();

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
        if (HasSave)
        {
            LoadRuntimeState();
            loadingFromSave = true;
            SceneLoader.Load(SavedSceneName);
            return;
        }

        LoadLevel("Terrain");
    }

    public static void LoadNewGame()
    {
        DeleteSave();
        LoadLevel("Terrain");
    }

    public static void LoadLevel(string sceneName)
    {
        if (!IsLevelUnlocked(sceneName))
        {
            Debug.LogWarning($"Level is locked and cannot be loaded: {sceneName}");
            return;
        }

        loadingFromSave = false;
        hasRuntimeOxygen = false;
        ResetRuntimeStateForLevel(sceneName);
        SceneLoader.Load(sceneName);
    }

    public static bool IsLevelUnlocked(string sceneName)
    {
        if (sceneName == "Terrain" || sceneName == "Day1")
        {
            return true;
        }

        if (!IsGameplayScene(sceneName))
        {
            return false;
        }

        return PlayerPrefs.GetInt(LevelUnlockedPrefix + sceneName, 0) == 1;
    }

    public static void UnlockLevel(string sceneName)
    {
        UnlockLevel(sceneName, saveImmediately: true);
    }

    public static bool TryApplySavedCheckpoint(GameObject player)
    {
        if (!loadingFromSave || player == null || !HasSave || !HasCheckpoint)
        {
            return false;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != SavedSceneName)
        {
            return false;
        }

        Vector3 position = new Vector3(
            PlayerPrefs.GetFloat(CheckpointPositionXKey, player.transform.position.x),
            PlayerPrefs.GetFloat(CheckpointPositionYKey, player.transform.position.y),
            PlayerPrefs.GetFloat(CheckpointPositionZKey, player.transform.position.z));

        if (player.TryGetComponent(out CharacterController characterController))
        {
            characterController.enabled = false;
            player.transform.position = position;
            characterController.enabled = true;
        }
        else
        {
            player.transform.position = position;
        }

        if (player.TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        return true;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsGameplayScene(scene.name))
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        TryApplySavedCheckpoint(player);
        ApplyOxygenState(player);
    }

    private static void ResetRuntimeStateForLevel(string sceneName)
    {
        RuntimeInventoryState.Reset();
        RuntimeMissionState.ResetAll();

        switch (sceneName)
        {
            case "Day2":
                RuntimeInventoryState.SetState(
                    hasSmallMap: false,
                    hasMorseCode: true,
                    hasSecretDecree: true,
                    hasFlashlight: false,
                    mapFragmentCount: 0);
                RuntimeMissionState.SetMissionState(MissionIds.Main_FindLostMap, MissionState.Active);
                RuntimeMissionState.SetMissionState(MissionIds.Day1_FindTinBox, MissionState.Completed);
                break;

            case "Day3":
                RuntimeInventoryState.SetState(
                    hasSmallMap: false,
                    hasMorseCode: true,
                    hasSecretDecree: true,
                    hasFlashlight: false,
                    mapFragmentCount: 0);
                RuntimeMissionState.SetMissionState(MissionIds.Main_FindLostMap, MissionState.Active);
                RuntimeMissionState.SetMissionState(MissionIds.Day1_FindTinBox, MissionState.Completed);
                RuntimeMissionState.SetMissionState(MissionIds.Day2_MorseTransmission, MissionState.Completed);
                break;
        }
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
        PlayerPrefs.DeleteKey(CheckpointExistsKey);
        PlayerPrefs.DeleteKey(CheckpointPositionXKey);
        PlayerPrefs.DeleteKey(CheckpointPositionYKey);
        PlayerPrefs.DeleteKey(CheckpointPositionZKey);
        PlayerPrefs.DeleteKey(OxygenKey);
        PlayerPrefs.DeleteKey(RestoreOxygenOnContinueKey);

        foreach (string missionId in SavedMissionIds)
        {
            PlayerPrefs.DeleteKey(MissionStatePrefix + missionId);
        }

        foreach (string gameplaySceneName in GameplaySceneNames)
        {
            if (gameplaySceneName != "Terrain" && gameplaySceneName != "Day1")
            {
                PlayerPrefs.DeleteKey(LevelUnlockedPrefix + gameplaySceneName);
            }
        }

        PlayerPrefs.Save();
        RuntimeInventoryState.Reset();
        RuntimeMissionState.ResetAll();
        loadingFromSave = false;
        hasRuntimeOxygen = false;
    }

    public static void CapturePlayerOxygenForSceneTransition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || !player.TryGetComponent(out AnOxygen oxygen))
        {
            return;
        }

        runtimeOxygen = oxygen.CurrentOxygen;
        hasRuntimeOxygen = true;
    }

    public static void MarkRestoreOxygenOnNextContinue()
    {
        PlayerPrefs.SetInt(RestoreOxygenOnContinueKey, 1);
        PlayerPrefs.Save();
        hasRuntimeOxygen = false;
    }

    private static void SavePlayerCheckpointIfAvailable()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            PlayerPrefs.DeleteKey(CheckpointExistsKey);
            PlayerPrefs.DeleteKey(CheckpointPositionXKey);
            PlayerPrefs.DeleteKey(CheckpointPositionYKey);
            PlayerPrefs.DeleteKey(CheckpointPositionZKey);
            return;
        }

        Vector3 position = player.transform.position;
        PlayerPrefs.SetInt(CheckpointExistsKey, 1);
        PlayerPrefs.SetFloat(CheckpointPositionXKey, position.x);
        PlayerPrefs.SetFloat(CheckpointPositionYKey, position.y);
        PlayerPrefs.SetFloat(CheckpointPositionZKey, position.z);
    }

    private static void SavePlayerOxygenIfAvailable()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || !player.TryGetComponent(out AnOxygen oxygen))
        {
            return;
        }

        PlayerPrefs.SetFloat(OxygenKey, oxygen.CurrentOxygen);
    }

    private static void ApplyOxygenState(GameObject player)
    {
        if (player == null || !player.TryGetComponent(out AnOxygen oxygen))
        {
            return;
        }

        if (loadingFromSave && PlayerPrefs.GetInt(RestoreOxygenOnContinueKey, 0) == 1)
        {
            oxygen.SetCurrentOxygen(oxygen.MaxOxygen);
            PlayerPrefs.DeleteKey(RestoreOxygenOnContinueKey);
            PlayerPrefs.SetFloat(OxygenKey, oxygen.MaxOxygen);
            PlayerPrefs.Save();
            hasRuntimeOxygen = false;
            return;
        }

        if (hasRuntimeOxygen)
        {
            oxygen.SetCurrentOxygen(runtimeOxygen);
            return;
        }

        if (loadingFromSave && HasSave && PlayerPrefs.HasKey(OxygenKey))
        {
            oxygen.SetCurrentOxygen(PlayerPrefs.GetFloat(OxygenKey, oxygen.CurrentOxygen));
        }
    }

    private static void UnlockLevel(string sceneName, bool saveImmediately)
    {
        if (!IsGameplayScene(sceneName))
        {
            return;
        }

        if (sceneName != "Terrain" && sceneName != "Day1")
        {
            PlayerPrefs.SetInt(LevelUnlockedPrefix + sceneName, 1);
        }

        if (saveImmediately)
        {
            PlayerPrefs.Save();
        }
    }

    private static bool IsGameplayScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        foreach (string gameplaySceneName in GameplaySceneNames)
        {
            if (sceneName == gameplaySceneName)
            {
                return true;
            }
        }

        return false;
    }
}
