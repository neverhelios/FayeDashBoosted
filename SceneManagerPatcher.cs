using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;


namespace FayeDashBoosted;

class SceneManagerLogger
{
    public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        Plugin.Logger.LogInfo($"Scene loaded: {scene.name} {mode}");
        Plugin.Logger.LogInfo($"Scene active: {SceneManager.GetActiveScene().name}");

        foreach(GameObject rootObj in scene.GetRootGameObjects())
        {
            Plugin.Logger.LogInfo(rootObj);
            for (int i = 0; i < rootObj.transform.childCount; i++)
            {
                var currChild = rootObj.transform.GetChild(i);
                Plugin.Logger.LogInfo($"+ Child[{i}]: {currChild.gameObject}");
            }
            for (int i = 0; i < rootObj.GetComponentCount(); i++)
            {
                var currComponent = rootObj.GetComponentAtIndex(i);
                Plugin.Logger.LogInfo($"-> Component[{i}]: {currComponent.GetType()}");
            }
        }
        Plugin.Logger.LogInfo("-----------------------------------------------------------------");
    }

    public static void OnSceneUnloaded(Scene scene)
    {
        Plugin.Logger.LogInfo($"Scene unloaded: {scene.name}");
    }

    public static void OnActiveSceneChanged(Scene sceneStart, Scene sceneFinished)
    {
        Plugin.Logger.LogInfo($"Scene changed from {sceneStart.name} to {sceneFinished.name}");
    }
}