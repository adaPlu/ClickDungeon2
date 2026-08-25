#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ClickDungeon.EditorTools
{
    public static class AnimationClipGenerator
    {
        private const string RuntimeArt="Assets/ClickDungeon/Art/Runtime";
        private const string Output="Assets/ClickDungeon/Art/GeneratedAnimations";

        [MenuItem("ClickDungeon/Art/Generate Sprite Animation Clips")]
        public static void GenerateAll()
        {
            PixelAssetImporter.ConfigureAll();Directory.CreateDirectory(Output);
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{RuntimeArt}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);string file=Path.GetFileNameWithoutExtension(path);
                if(!(IsCoreSheet(file)&&(file.StartsWith("monster_")||file.StartsWith("hero_")||file.StartsWith("boss_"))))continue;
                GenerateForSheet(path,file);
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("ClickDungeon animation clips generated.");
        }

        private static bool IsCoreSheet(string file)=>file.EndsWith("_core",StringComparison.Ordinal)||file.Contains("_core_",StringComparison.Ordinal);

        private static void GenerateForSheet(string sheetPath,string actorName)
        {
            var sprites=AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();if(sprites.Length==0)throw new InvalidDataException($"No sliced sprites found at {sheetPath}");
            var groups=sprites.GroupBy(s=>s.name.Split('_')[0],StringComparer.Ordinal);
            string actorDir=$"{Output}/{actorName}";Directory.CreateDirectory(actorDir);
            foreach(var group in groups)
            {
                var frames=group.OrderBy(s=>FrameNumber(s.name)).ToArray();var clip=new AnimationClip{frameRate=10};
                var curve=new ObjectReferenceKeyframe[frames.Length+1];for(int i=0;i<frames.Length;i++)curve[i]=new ObjectReferenceKeyframe{time=i/10f,value=frames[i]};curve[frames.Length]=new ObjectReferenceKeyframe{time=frames.Length/10f,value=frames[group.Key=="Move"||group.Key=="Idle"?0:frames.Length-1]};
                EditorCurveBinding binding=new EditorCurveBinding{type=typeof(SpriteRenderer),path=string.Empty,propertyName="m_Sprite"};AnimationUtility.SetObjectReferenceCurve(clip,binding,curve);
                var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=group.Key=="Move"||group.Key=="Idle";AnimationUtility.SetAnimationClipSettings(clip,settings);
                string clipPath=$"{actorDir}/{group.Key}.anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);if(existing==null)AssetDatabase.CreateAsset(clip,clipPath);else EditorUtility.CopySerialized(clip,existing);
            }
        }

        private static int FrameNumber(string name){int i=name.LastIndexOf('_');return i>=0&&int.TryParse(name.Substring(i+1),out int n)?n:0;}
    }
}
#endif
