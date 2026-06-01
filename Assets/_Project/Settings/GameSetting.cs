using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "CuChi/Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Game Info")]
    public string gameTitle = "Cu Chi: Echoes Beneath the Earth";
    public string version = "0.1.0";

    [Header("Default Chapter")]
    public string firstSceneName = "Chapter_01_SecretEntrance";

    [Header("Gameplay")]
    public bool enableTutorial = true;
    public bool enablePause = true;
    public bool enableGameOver = true;

    [Header("Difficulty")]
    public float oxygenMultiplier = 1f;
    public float enemyDetectionMultiplier = 1f;
}