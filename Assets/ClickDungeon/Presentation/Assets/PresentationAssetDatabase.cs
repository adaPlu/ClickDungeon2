using System;
using UnityEngine;

namespace ClickDungeon.Presentation.Assets
{
    [CreateAssetMenu(menuName="ClickDungeon/Generated Presentation Asset Database",fileName="ClickDungeonPresentationAssets")]
    public sealed class PresentationAssetDatabase : ScriptableObject
    {
        [Serializable] public sealed class SpriteEntry { public string Id=string.Empty; public Sprite Sprite; }
        [Serializable] public sealed class AudioEntry { public string Id=string.Empty; public AudioClip Clip; }
        [SerializeField] private SpriteEntry[] sprites=Array.Empty<SpriteEntry>();
        [SerializeField] private AudioEntry[] audio=Array.Empty<AudioEntry>();

        public Sprite SpriteFor(string id)
        {
            if(string.IsNullOrEmpty(id))return null;
            for(int i=0;i<sprites.Length;i++)if(string.Equals(sprites[i].Id,id,StringComparison.Ordinal))return sprites[i].Sprite;
            return null;
        }

        public AudioClip AudioFor(string id)
        {
            if(string.IsNullOrEmpty(id))return null;
            for(int i=0;i<audio.Length;i++)if(string.Equals(audio[i].Id,id,StringComparison.Ordinal))return audio[i].Clip;
            return null;
        }

#if UNITY_EDITOR
        public void Replace(SpriteEntry[] spriteEntries,AudioEntry[] audioEntries)
        {
            sprites=spriteEntries??Array.Empty<SpriteEntry>();audio=audioEntries??Array.Empty<AudioEntry>();
        }
#endif
    }
}
