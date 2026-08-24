#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClickDungeon.EditorTools
{
    public static class BuildVerification
    {
        public static void Verify()
        {
            BuildPlayerSettings.Apply(EditorUserBuildSettings.activeBuildTarget);
            TextMeshProResourceBootstrap.Ensure();
            PrototypeAssetBootstrap.Ensure();
            ContentValidator.ValidateOrThrow();
            ContentAssetGenerator.Generate(false);
            PresentationAssetGenerator.Generate(false);
            SceneScaffolder.EnsureCoreScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if(!File.Exists(TextMeshProResourceBootstrap.SettingsPath))throw new InvalidDataException("TMP Settings asset is missing after build verification.");
            if(!File.Exists("Assets/ClickDungeon/Scenes/Boot.unity")||!File.Exists("Assets/ClickDungeon/Scenes/Main.unity")||!File.Exists("Assets/ClickDungeon/Scenes/Game.unity"))throw new InvalidDataException("One or more core scenes are missing after build verification.");
            if(AssetDatabase.LoadAssetAtPath<ClickDungeon.Application.Content.GeneratedContentDatabase>(ContentAssetGenerator.AssetPath)==null)throw new InvalidDataException("Generated content database is missing after build verification.");
            if(AssetDatabase.LoadAssetAtPath<ClickDungeon.Presentation.Assets.PresentationAssetDatabase>(PresentationAssetGenerator.AssetPath)==null)throw new InvalidDataException("Presentation asset database is missing after build verification.");

            Debug.Log("CLICKDUNGEON_BUILD_VERIFICATION_OK");
        }
    }
}
#endif
