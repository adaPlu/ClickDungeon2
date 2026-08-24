#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ClickDungeon.EditorTools
{
    public static class BuildAutomation
    {
        private static readonly string[] Scenes={"Assets/ClickDungeon/Scenes/Boot.unity","Assets/ClickDungeon/Scenes/Main.unity","Assets/ClickDungeon/Scenes/Game.unity"};
        public static void BuildWindows()=>Build(BuildTarget.StandaloneWindows64,"Builds/Windows/ClickDungeon2.exe");
        public static void BuildAndroid()=>Build(BuildTarget.Android,"Builds/Android/ClickDungeon2.aab");
        public static void BuildWeb()=>Build(BuildTarget.WebGL,"Builds/Web");
        public static void ExportIos()=>Build(BuildTarget.iOS,"Builds/iOS");
        private static void Build(BuildTarget target,string path)
        {
            BuildPlayerSettings.Apply(target);
            TextMeshProResourceBootstrap.Ensure();
            SceneScaffolder.EnsureCoreScenes();
            ContentAssetGenerator.Generate(false);
            PresentationAssetGenerator.Generate(false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Directory.CreateDirectory(Path.GetDirectoryName(path)??path);
            var options=new BuildPlayerOptions{scenes=Scenes,locationPathName=path,target=target,options=BuildOptions.None};
            if(target==BuildTarget.Android)EditorUserBuildSettings.buildAppBundle=true;
            BuildReport report=BuildPipeline.BuildPlayer(options);
            if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception($"Build failed: {report.summary.result}");
            Debug.Log($"Build succeeded {target}: {path}");
        }
    }
}
#endif
