#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ClickDungeon.Presentation;
using ClickDungeon.Presentation.Menu;

namespace ClickDungeon.EditorTools
{
    public static class SceneScaffolder
    {
        [MenuItem("ClickDungeon/Setup/Create Core Scenes")]
        public static void CreateCoreScenes()
        {
            string dir="Assets/ClickDungeon/Scenes";Directory.CreateDirectory(dir);
            Create(Path.Combine(dir,"Boot.unity"),"BootLoader",go=>go.AddComponent<BootLoader>());
            Create(Path.Combine(dir,"Main.unity"),"MainMenu",go=>go.AddComponent<MainMenuUI>());
            Create(Path.Combine(dir,"Game.unity"),"GameBootstrap",go=>go.AddComponent<GameBootstrap>());
            AssetDatabase.Refresh();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Path.Combine(dir,"Boot.unity"),true),new EditorBuildSettingsScene(Path.Combine(dir,"Main.unity"),true),new EditorBuildSettingsScene(Path.Combine(dir,"Game.unity"),true)};
        }
        public static void EnsureCoreScenes(){if(!File.Exists("Assets/ClickDungeon/Scenes/Boot.unity")||!File.Exists("Assets/ClickDungeon/Scenes/Main.unity")||!File.Exists("Assets/ClickDungeon/Scenes/Game.unity"))CreateCoreScenes();}
        private static void Create(string path,string name,System.Action<GameObject> configure){var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var go=new GameObject(name);configure(go);EditorSceneManager.SaveScene(scene,path);}
    }
}
#endif
