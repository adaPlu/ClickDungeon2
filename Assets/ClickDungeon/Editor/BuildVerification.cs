#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ClickDungeon.EditorTools
{
    public static class BuildVerification
    {
        public static void Verify()
        {
            ContentValidator.ValidateOrThrow();ContentAssetGenerator.Generate(false);PresentationAssetGenerator.Generate(false);SceneScaffolder.EnsureCoreScenes();
            Debug.Log("CLICKDUNGEON_BUILD_VERIFICATION_OK");
        }
    }
}
#endif
