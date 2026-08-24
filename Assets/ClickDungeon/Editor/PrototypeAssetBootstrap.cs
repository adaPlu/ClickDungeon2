#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClickDungeon.EditorTools
{
    /// <summary>
    /// Creates deterministic DEVELOPMENT-ONLY placeholder media when a clean CI checkout has no
    /// binary runtime assets. Generated names retain the _placeholder suffix so release validation
    /// can never mistake them for production assets. Existing runtime media is never overwritten.
    /// </summary>
    public static class PrototypeAssetBootstrap
    {
        private const string ArtRoot="Assets/ClickDungeon/Art/Runtime";
        private const string AudioRoot="Assets/ClickDungeon/Audio/Runtime";

        private static readonly string[] Monsters={"rat","slime","goblin","skeleton","spider","wolf","bandit","cultist","warlock","wraith","golem","vampire","demon","hellhound","revenant","orc","troll","witch","dragon","lich","archdemon","ancient_wyrm","bat"};
        private static readonly string[] Heroes={"knight","ranger","thief","wizard"};
        private static readonly string[] Bosses={"lich_sovereign","rootbound_leviathan","frostbog_colossus","archdemon_overlord","primal_ancient_wyrm"};
        private static readonly string[] Biomes={"cavern","crypt","sunken_temple","thorn_wilds","mire","frozen_ruins","storm_plateau","lava_field","arcane_nexus","ash_wastes"};
        private static readonly string[] Icons={"big_key","small_key","key_big","gold","chest_closed","chest_open","sealed_vault","safe_exit","forbidden_exit","exit_forbidden","merchant","healing_potion","trap_disarm_kit","clue_danger","clue_opportunity","clue_passage","shrine_hp","shrine_attack","shrine_defense","trap_acid","trap_fire","trap_freeze","trap_pitfall","trap_poison"};
        private static readonly string[] Sfx={"ability","attack_player","boss_phase","chest_open","defeat","defend_player","forbidden_exit","merchant","monster_hit","pickup_gold","pickup_key","safe_exit","shrine","trap_acid","trap_fire","trap_freeze","trap_pitfall","trap_poison","ui_reveal","victory"};
        private static readonly string[] Music={"music_boss","music_combat","music_defeat","music_exploration","music_final_boss","music_menu","music_victory"};

        [MenuItem("ClickDungeon/Assets/Ensure Prototype Runtime Media")]
        public static void Ensure()
        {
            bool artMissing=!Directory.Exists(ArtRoot)||Directory.GetFiles(ArtRoot,"*.png").Length==0;
            bool audioMissing=!Directory.Exists(AudioRoot)||Directory.GetFiles(AudioRoot,"*.wav").Length==0;
            if(artMissing)GenerateArt();
            if(audioMissing)GenerateAudio();
            if(artMissing||audioMissing)AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
            if(artMissing)Debug.LogWarning("Generated ClickDungeon DEVELOPMENT placeholder art because runtime binary art was absent. Release gate must reject these assets.");
            if(audioMissing)Debug.LogWarning("Generated ClickDungeon DEVELOPMENT placeholder audio because runtime binary audio was absent. Release gate must reject these assets.");
        }

        private static void GenerateArt()
        {
            Directory.CreateDirectory(ArtRoot);
            foreach(string id in Monsters)WriteSheet($"monster_{id}_core_placeholder.png",256,192,64,3,4);
            foreach(string id in Heroes)WriteSheet($"hero_{id}_core_placeholder.png",256,256,64,4,4);
            foreach(string id in Bosses)WriteSheet($"boss_{id}_core_placeholder.png",512,384,128,3,4);
            foreach(string id in Biomes)WriteCard($"biome_{id}_master_placeholder.png",640,360);
            foreach(string id in Icons)WriteCard($"{id}_placeholder.png",64,64);
        }

        private static void GenerateAudio()
        {
            Directory.CreateDirectory(AudioRoot);
            foreach(string id in Sfx)WriteTone($"{id}_placeholder.wav",0.14f,Frequency(id),0.12f);
            foreach(string id in Biomes)WriteTone($"ambience_{id}_placeholder.wav",0.8f,Frequency(id)/2f,0.045f);
            foreach(string id in Music)WriteTone($"{id}_placeholder.wav",1.0f,Frequency(id),0.05f);
        }

        private static void WriteSheet(string file,int width,int height,int frame,int rows,int columns)
        {
            string path=Path.Combine(ArtRoot,file);if(File.Exists(path))return;
            var texture=NewTransparent(width,height);Color32 color=ColorFor(file);
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++)
            {
                int x0=col*frame+frame/4+(col*2);int y0=height-(row+1)*frame+frame/4+(row*2);int w=frame/2;int h=frame/2;
                FillRect(texture,x0,y0,w,h,color);FillRect(texture,x0+w/3,y0+h,w/3,Math.Max(2,frame/10),new Color32(255,255,255,255));
            }
            SavePng(texture,path);
        }

        private static void WriteCard(string file,int width,int height)
        {
            string path=Path.Combine(ArtRoot,file);if(File.Exists(path))return;
            var texture=NewTransparent(width,height);Color32 color=ColorFor(file);FillRect(texture,width/8,height/8,width*3/4,height*3/4,color);FillRect(texture,width/4,height/4,width/2,height/8,new Color32(255,255,255,210));SavePng(texture,path);
        }

        private static Texture2D NewTransparent(int width,int height)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);var pixels=new Color32[width*height];for(int i=0;i<pixels.Length;i++)pixels[i]=new Color32(0,0,0,0);texture.SetPixels32(pixels);return texture;
        }

        private static void FillRect(Texture2D texture,int x,int y,int width,int height,Color32 color)
        {
            int maxX=Math.Min(texture.width,x+width),maxY=Math.Min(texture.height,y+height);for(int py=Math.Max(0,y);py<maxY;py++)for(int px=Math.Max(0,x);px<maxX;px++)texture.SetPixel(px,py,color);
        }

        private static void SavePng(Texture2D texture,string path)
        {
            texture.Apply(false,false);File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        }

        private static Color32 ColorFor(string value)
        {
            unchecked{uint h=2166136261u;foreach(char c in value){h^=c;h*=16777619u;}return new Color32((byte)(70+(h&127)),(byte)(70+((h>>8)&127)),(byte)(70+((h>>16)&127)),255);}
        }

        private static float Frequency(string value)
        {
            unchecked{uint h=2166136261u;foreach(char c in value){h^=c;h*=16777619u;}return 180f+(h%420u);}
        }

        private static void WriteTone(string file,float seconds,float frequency,float amplitude)
        {
            string path=Path.Combine(AudioRoot,file);if(File.Exists(path))return;const int sampleRate=48000;int samples=Math.Max(1,(int)(sampleRate*seconds));short channels=1,bits=16;int dataSize=samples*channels*(bits/8);
            using(var stream=File.Create(path))using(var writer=new BinaryWriter(stream))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+dataSize);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));writer.Write(16);writer.Write((short)1);writer.Write(channels);writer.Write(sampleRate);writer.Write(sampleRate*channels*(bits/8));writer.Write((short)(channels*(bits/8)));writer.Write(bits);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(dataSize);
                for(int i=0;i<samples;i++){double envelope=Math.Min(1.0,i/(sampleRate*0.01))*Math.Min(1.0,(samples-i)/(sampleRate*0.02));double sample=Math.Sin(2.0*Math.PI*frequency*i/sampleRate)*amplitude*envelope;writer.Write((short)Math.Max(short.MinValue,Math.Min(short.MaxValue,sample*short.MaxValue)));}
            }
        }
    }
}
#endif
