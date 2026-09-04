#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClickDungeon.EditorTools
{
    public static class PixelAssetImporter
    {
        private const string RuntimeArt="Assets/ClickDungeon/Art/Runtime";
        private static readonly string[] HeroDerivedVariants={"portrait","roster","select","gameplay","idle","attack","hit","victory","defeat"};

        [MenuItem("ClickDungeon/Art/Configure Pixel Imports")]
        public static void ConfigureAll()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{RuntimeArt}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);if(!path.EndsWith(".png",StringComparison.OrdinalIgnoreCase))continue;Configure(path);
            }
            AssetDatabase.SaveAssets();Debug.Log("ClickDungeon art import configuration complete.");
        }

        public static void Configure(string path)
        {
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)return;
            importer.textureType=TextureImporterType.Sprite;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
            string file=Path.GetFileNameWithoutExtension(path);

            if(IsDerivedHeroAsset(file))
            {
                ConfigureDerivedHero(importer,file);importer.SaveAndReimport();return;
            }

            importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.spritePixelsPerUnit=64;
            if(file.StartsWith("monster_",StringComparison.Ordinal)&&IsCoreSheet(file))Slice(importer,256,192,64,new[]{"Attack","Defend","Move"},4);
            else if(file.StartsWith("hero_",StringComparison.Ordinal)&&IsCoreSheet(file))Slice(importer,256,256,64,new[]{"Idle","Move","Attack","Defend"},4);
            else if(file.StartsWith("boss_",StringComparison.Ordinal)&&IsCoreSheet(file)){importer.spritePixelsPerUnit=128;Slice(importer,512,384,128,new[]{"Attack","Defend","Move"},4);}
            else importer.spriteImportMode=SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        private static void ConfigureDerivedHero(TextureImporter importer,string file)
        {
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Compressed;
            importer.spritePixelsPerUnit=100;
            importer.maxTextureSize=DerivedHeroMaxSize(file);
        }

        private static bool IsDerivedHeroAsset(string file)
        {
            if(!file.StartsWith("hero_",StringComparison.OrdinalIgnoreCase)||IsCoreSheet(file))return false;
            foreach(string variant in HeroDerivedVariants)if(file.EndsWith("_"+variant,StringComparison.OrdinalIgnoreCase))return true;
            return false;
        }

        private static int DerivedHeroMaxSize(string file)
        {
            if(file.EndsWith("_select",StringComparison.OrdinalIgnoreCase))return 1024;
            if(file.EndsWith("_roster",StringComparison.OrdinalIgnoreCase))return 256;
            if(file.EndsWith("_portrait",StringComparison.OrdinalIgnoreCase))return 512;
            return 512;
        }

        private static bool IsCoreSheet(string file)=>file.EndsWith("_core",StringComparison.Ordinal)||file.Contains("_core_",StringComparison.Ordinal);

        private static void Slice(TextureImporter importer,int width,int height,int frame,string[] rows,int columns)
        {
            importer.spriteImportMode=SpriteImportMode.Multiple;var sprites=new SpriteMetaData[rows.Length*columns];int n=0;
            for(int row=0;row<rows.Length;row++)for(int col=0;col<columns;col++)sprites[n++]=new SpriteMetaData{name=$"{rows[row]}_{col+1}",alignment=(int)SpriteAlignment.Center,pivot=new Vector2(.5f,.5f),rect=new Rect(col*frame,height-(row+1)*frame,frame,frame)};
#pragma warning disable 618
            importer.spritesheet=sprites;
#pragma warning restore 618
        }
    }
}
#endif
