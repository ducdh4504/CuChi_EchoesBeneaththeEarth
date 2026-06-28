//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class MorseMissionCompleteHandler : MonoBehaviour
//{
//    [Header("Mission")]
//    [SerializeField] private string missionIdToComplete = MissionIds.Day2_MorseTransmission;

//    [Header("Next Scene")]
//    [SerializeField] private string nextSceneName = "Day3";

//    [TextArea(2, 4)]
//    [SerializeField]
//    private string transitionMessage =
//        "Ngày 3/2/1948 - Tìm lại tấm bản đồ thất lạc";

//    public void CompleteMorseMissionAndLoadNextScene()
//    {
//        if (MissionManager.Instance != null)
//        {
//            MissionManager.Instance.CompleteMission(missionIdToComplete);
//        }

//        if (SceneTransitionController.Instance != null)
//        {
//            SceneTransitionController.Instance.LoadScene(nextSceneName, transitionMessage);
//            return;
//        }

//        SceneManager.LoadScene(nextSceneName);
//    }
//}

using UnityEngine;
using UnityEngine.SceneManagement;

public class MorseMissionCompleteHandler : MonoBehaviour
{
    [Header("Mission")]
    [SerializeField] private string missionIdToComplete = MissionIds.Day2_MorseTransmission;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "Day3";

    [TextArea(2, 4)]
    [SerializeField]
    private string transitionMessage =
        "Ngày 3 - Tìm lại tấm bản đồ thất lạc";

    public void CompleteMorseMissionAndLoadNextScene()
    {
        // Quan trọng: đảm bảo game không bị đứng sau khi chuyển scene
        Time.timeScale = 1f;

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.CompleteMission(missionIdToComplete);
            MissionManager.Instance.StartMission(MissionIds.Day3_FindMap);
        }

        GameSaveSystem.UnlockLevel(nextSceneName);

        if (SceneTransitionController.Instance != null)
        {
            SceneTransitionController.Instance.LoadScene(nextSceneName, transitionMessage);
            return;
        }

        GameSaveSystem.CapturePlayerOxygenForSceneTransition();
        SceneManager.LoadScene(nextSceneName);
    }
}
