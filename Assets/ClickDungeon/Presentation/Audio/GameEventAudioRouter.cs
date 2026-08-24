using System.Collections.Generic;
using UnityEngine;
using ClickDungeon.Presentation.Assets;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Presentation.Audio
{
    public sealed class GameEventAudioRouter : MonoBehaviour
    {
        private AudioSource _source;
        private PresentationAssetDatabase _assets;

        public void Initialize()
        {
            _assets=Resources.Load<PresentationAssetDatabase>("ClickDungeonPresentationAssets");
            _source=gameObject.GetComponent<AudioSource>()??gameObject.AddComponent<AudioSource>();_source.playOnAwake=false;_source.spatialBlend=0f;
        }

        public void Present(IReadOnlyList<GameEvent> events)
        {
            if(_source==null)Initialize();if(_assets==null||events==null)return;
            foreach(var evt in events)
            {
                string key=KeyFor(evt);if(string.IsNullOrEmpty(key))continue;var clip=_assets.AudioFor(key);if(clip!=null)_source.PlayOneShot(clip);
            }
        }

        private static string KeyFor(GameEvent evt)
        {
            if(evt.Type=="key.small.collected"||evt.Type=="key.big.collected")return "event.key.collected";
            if(evt.Type=="ability.used"||evt.Type.StartsWith("ability."))return "event.ability.used";
            if(evt.Type=="trap.triggered"&&evt.Id.StartsWith("trap."))return "event.trap."+evt.Id.Substring(5);
            return "event."+evt.Type;
        }
    }
}
