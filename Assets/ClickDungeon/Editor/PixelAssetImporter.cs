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
            importer.GetSourceTextureWidthAndHeight(out int sourceWidth,out int sourceHeight);
            bool highDefinition=sourceWidth>=512||sourceHeight>=512;
            importer.textureType=TextureImporterType.Sprite;
            importer.mipmapEnabled=false;
            importer.filterMode=highDefinition?FilterMode.Bilinear:FilterMode.Point;
            importer.textureCompression=highDefinition?TextureImporterCompression.CompressedHQ:TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency=true;

            string file=Path.GetFileNameWithoutExtension(path);
            if(file.StartsWith("monster_",StringComparison.Ordinal)&&IsCoreSheet(file))ConfigureCoreSheet(importer,sourceWidth,sourceHeight,new[]{"Attack","Defend","Move"});
            else if(file.StartsWith("hero_",StringComparison.Ordinal)&&IsCoreSheet(file))ConfigureCoreSheet(importer,sourceWidth,sourceHeight,new[]{"Idle","Move","Attack","Defend"});
            else if(file.StartsWith("boss_",StringComparison.Ordinal)&&IsCoreSheet(file))ConfigureCoreSheet(importer,sourceWidth,sourceHeight,new[]{"Attack","Defend","Move"});
            else
            {
                importer.spritePixelsPerUnit=highDefinition?128:64;
                importer.spriteImportMode=SpriteImportMode.Single;
            }
            importer.SaveAndReimport();
        }

        private static void ConfigureCoreSheet(TextureImporter importer,int width,int height,string[] rows)
        {
            const int columns=4;
            if(width<=0||height<=0||width%columns!=0)throw new InvalidDataException($"Core sheet must have four equal columns, got {width}x{height}.");
            int frame=width/columns;
            int expectedHeight=frame*rows.Length;
            if(height!=expectedHeight)throw new InvalidDataException($"Core sheet must be {columns}x{rows.Length} square frames, got {width}x{height}; expected height {expectedHeight}.");
            importer.spritePixelsPerUnit=frame;
            Slice(importer,width,height,frame,rows,columns);
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
