using UnityEngine;
using UnityEngine.SceneManagement;

public static class TerrainEntranceWaypointBootstrapper
{
    private const string TerrainSceneName = "Terrain";
    private const string Day1PortalName = "Portal_To_Day1";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != TerrainSceneName)
        {
            return;
        }

        QuestWaypointIndicator indicator = Object.FindAnyObjectByType<QuestWaypointIndicator>(FindObjectsInactive.Include);
        GameObject portal = GameObject.Find(Day1PortalName);

        if (indicator == null || portal == null)
        {
            return;
        }

        if (!portal.TryGetComponent(out WaypointTarget waypointTarget))
        {
            waypointTarget = portal.AddComponent<WaypointTarget>();
        }

        indicator.SetTarget(waypointTarget);
    }
}
