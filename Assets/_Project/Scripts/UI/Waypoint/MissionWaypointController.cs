using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionWaypointController : MonoBehaviour
{
    [Serializable]
    private class WaypointBinding
    {
        public string missionId;
        public string objectiveContains;
        public string targetWaypointId;
        public WaypointTarget target;
    }

    [SerializeField] private QuestWaypointIndicator indicator;
    [SerializeField] private List<WaypointBinding> waypointBindings = new List<WaypointBinding>();
    [SerializeField] private bool useMissionIdAsTargetId = true;
    [SerializeField] private bool hideCompletedMissions = true;
    [SerializeField] private float rescanInterval = 1f;

    private string activeMissionId;
    private float nextRescanTime;

    private void OnEnable()
    {
        MissionManager.ObjectiveChanged += HandleObjectiveChanged;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        SyncWaypoint(RuntimeMissionState.CurrentMissionId);
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRescanTime)
        {
            return;
        }

        nextRescanTime = Time.unscaledTime + rescanInterval;

        if (indicator == null)
        {
            indicator = FindAnyObjectByType<QuestWaypointIndicator>(FindObjectsInactive.Include);
        }

        if (activeMissionId != RuntimeMissionState.CurrentMissionId)
        {
            SyncWaypoint(RuntimeMissionState.CurrentMissionId);
        }
    }

    private void OnDisable()
    {
        MissionManager.ObjectiveChanged -= HandleObjectiveChanged;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleObjectiveChanged(string missionId, string objective)
    {
        SyncWaypoint(missionId);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        indicator = null;
        SyncWaypoint(RuntimeMissionState.CurrentMissionId);
    }

    private void SyncWaypoint(string missionId)
    {
        activeMissionId = missionId;

        if (indicator == null)
        {
            indicator = FindAnyObjectByType<QuestWaypointIndicator>(FindObjectsInactive.Include);
        }

        if (indicator == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(missionId)
            || (hideCompletedMissions && RuntimeMissionState.GetMissionState(missionId) == MissionState.Completed))
        {
            indicator.ClearTarget();
            return;
        }

        if (TryResolveBoundTarget(missionId, RuntimeMissionState.CurrentObjective, out WaypointTarget boundTarget))
        {
            indicator.SetTarget(boundTarget);
            return;
        }

        if (useMissionIdAsTargetId)
        {
            indicator.SetTargetById(missionId);
            return;
        }

        indicator.ClearTarget();
    }

    private bool TryResolveBoundTarget(string missionId, string objective, out WaypointTarget target)
    {
        target = null;

        for (int i = 0; i < waypointBindings.Count; i++)
        {
            WaypointBinding binding = waypointBindings[i];
            if (binding == null || binding.missionId != missionId)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(binding.objectiveContains)
                && (string.IsNullOrWhiteSpace(objective)
                    || !objective.Contains(binding.objectiveContains, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (binding.target != null)
            {
                target = binding.target;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(binding.targetWaypointId)
                && WaypointTarget.TryFind(binding.targetWaypointId, out target))
            {
                return true;
            }
        }

        return false;
    }
}
