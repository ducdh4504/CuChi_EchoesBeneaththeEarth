using System.Collections.Generic;

public static class RuntimeMissionState
{
    private static readonly Dictionary<string, MissionState> missionStates =
        new Dictionary<string, MissionState>();

    public static string CurrentMissionId { get; private set; }
    public static string CurrentObjective { get; private set; }

    public static MissionState GetMissionState(string missionId)
    {
        if (string.IsNullOrWhiteSpace(missionId))
        {
            return MissionState.NotStarted;
        }

        if (missionStates.TryGetValue(missionId, out MissionState state))
        {
            return state;
        }

        return MissionState.NotStarted;
    }

    public static void SetMissionState(string missionId, MissionState state)
    {
        if (string.IsNullOrWhiteSpace(missionId))
        {
            return;
        }

        missionStates[missionId] = state;
    }

    public static void SetCurrentMission(string missionId)
    {
        CurrentMissionId = missionId;
    }

    public static void SetCurrentObjective(string objective)
    {
        CurrentObjective = objective;
    }

    public static void SetCurrentProgress(string missionId, string objective)
    {
        CurrentMissionId = missionId;
        CurrentObjective = objective;
    }

    public static void ResetAll()
    {
        missionStates.Clear();
        CurrentMissionId = null;
        CurrentObjective = null;
    }
}
