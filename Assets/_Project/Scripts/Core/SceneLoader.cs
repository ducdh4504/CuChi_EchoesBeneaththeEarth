using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader 
{
    public enum Scene
    {
        MainMenu,
        Terrain,
        Day1,
        Day2,
        Day3,
        LoadedScene,
    }

    private static Scene targetScene;
    private static string targetSceneName;

    public static void Load(Scene targetScene)
    {
        SceneLoader.targetScene = targetScene;
        targetSceneName = targetScene.ToString();
        SceneManager.LoadScene(Scene.LoadedScene.ToString());
    }

    public static void Load(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            sceneName = Scene.Terrain.ToString();
        }

        targetSceneName = sceneName;
        SceneManager.LoadScene(Scene.LoadedScene.ToString());
    }

    public static void LoaderCallBack()
    {

        SceneManager.LoadScene(string.IsNullOrWhiteSpace(targetSceneName) ? targetScene.ToString() : targetSceneName);
    }
}
