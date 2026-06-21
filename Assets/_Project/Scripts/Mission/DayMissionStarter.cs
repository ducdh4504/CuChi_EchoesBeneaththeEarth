using System.Collections;
using UnityEngine;

public class DayMissionStarter : MonoBehaviour
{
    private enum DayType
    {
        Day1,
        Day2,
        Day3
    }

    [Header("Day")]
    [SerializeField] private DayType dayType;

    [Header("Settings")]
    [SerializeField] private float startDelay = 0.1f;

    private IEnumerator Start()
    {
        //yield return new WaitForSeconds(startDelay);
        yield return new WaitForSecondsRealtime(startDelay);

        if (MissionManager.Instance == null)
        {
            Debug.LogWarning("MissionManager was not found in scene.");
            yield break;
        }

        if (HasLoadedMissionProgress())
        {
            yield break;
        }

        switch (dayType)
        {
            case DayType.Day1:
                StartDay1Mission();
                break;

            case DayType.Day2:
                StartDay2IntroObjective();
                break;

            case DayType.Day3:
                StartDay3Mission();
                break;
        }
    }

    private static bool HasLoadedMissionProgress()
    {
        return !string.IsNullOrWhiteSpace(RuntimeMissionState.CurrentMissionId)
            || !string.IsNullOrWhiteSpace(RuntimeMissionState.CurrentObjective);
    }

    //private void StartDay1Mission()
    //{
    //    MissionManager.Instance.StartMainMissionIfNeeded();

    //    if (!MissionManager.Instance.IsMissionCompleted(MissionIds.Day1_FindTinBox))
    //    {
    //        MissionManager.Instance.StartMission(MissionIds.Day1_FindTinBox);
    //    }
    //}

    private void StartDay1Mission()
    {
        MissionManager.Instance.StartMainMissionIfNeeded();

        if (!MissionManager.Instance.IsMissionCompleted(MissionIds.Day1_FindTinBox))
        {
            MissionManager.Instance.ClearObjective();
        }
    }

    private void StartDay2IntroObjective()
    {
        MissionManager.Instance.StartMainMissionIfNeeded();

        if (!MissionManager.Instance.IsMissionCompleted(MissionIds.Day2_MorseTransmission))
        {
            MissionManager.Instance.SetMainObjective(
                "Tìm Cựu Chiến Binh trong địa đạo để nhận chỉ dẫn tiếp theo.");
        }
    }

    private void StartDay3Mission()
    {
        MissionManager.Instance.StartMainMissionIfNeeded();

        if (!MissionManager.Instance.IsMissionCompleted(MissionIds.Day3_FindMap))
        {
            MissionManager.Instance.StartMission(MissionIds.Day3_FindMap);
        }
    }
}
