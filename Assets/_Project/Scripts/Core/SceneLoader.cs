using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader 
{
    public enum Scene
    {
        MainMenu,
        Day1,
        Day2,
        Day3,
        LoadedScene,
    }

    private static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        SceneLoader.targetScene = targetScene;
        SceneManager.LoadScene(Scene.LoadedScene.ToString());
    }

    public static void LoaderCallBack()
    {

        SceneManager.LoadScene(targetScene.ToString());
    }
}
