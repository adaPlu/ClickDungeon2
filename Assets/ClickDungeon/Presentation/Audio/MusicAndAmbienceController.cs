using UnityEngine;
using ClickDungeon.Presentation.Assets;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Presentation.Audio
{
    public sealed class MusicAndAmbienceController : MonoBehaviour
    {
        private AudioSource _music;
        private AudioSource _ambience;
        private PresentationAssetDatabase _assets;
        private GameContent _content;
        private string _musicKey=string.Empty;
        private string _ambienceKey=string.Empty;

        public void Initialize(GameContent content,RunState state)
        {
            _content=content;_assets=Resources.Load<PresentationAssetDatabase>("ClickDungeonPresentationAssets");_music=gameObject.AddComponent<AudioSource>();_ambience=gameObject.AddComponent<AudioSource>();_music.loop=true;_ambience.loop=true;_music.spatialBlend=0;_ambience.spatialBlend=0;_music.volume=.48f;_ambience.volume=.28f;Refresh(state);
        }

        public void Refresh(RunState state)
        {
            if(state==null||_assets==null)return;var biome=_content?.Biome(state.BiomeId);string ambience=biome!=null&&!string.IsNullOrEmpty(biome.AmbienceId)?biome.AmbienceId:"ambience."+(state.BiomeId??"biome.cavern").Replace("biome.",string.Empty);string music=MusicKey(state);
            if(ambience!=_ambienceKey){_ambienceKey=ambience;PlayLoop(_ambience,_assets.AudioFor(ambience));}
            if(music!=_musicKey){_musicKey=music;PlayLoop(_music,_assets.AudioFor(music));}
        }

        private string MusicKey(RunState state)
        {
            bool activeBoss=state.BossRequired&&!state.BossDefeated; if(activeBoss&&state.Mode==RunMode.Campaign&&state.Floor>=_content.Balance.CampaignFloors)return "music.final_boss";if(activeBoss)return "music.boss";if(HasAdjacentCombat(state))return "music.combat";return "music.exploration";
        }

        private static bool HasAdjacentCombat(RunState state)
        {
            for(int i=0;i<state.Tiles.Count;i++)
            {
                var tile=state.Tiles[i];if(tile.Occupancy!=OccupancyKind.Monster||tile.MonsterHp<=0||tile.Visibility!=TileVisibility.Revealed)continue;
                var pos=new GridPosition(i/RunState.BoardSize,i%RunState.BoardSize);if(state.PlayerPosition.IsOrthogonallyAdjacent(pos))return true;
            }
            return false;
        }

        private static void PlayLoop(AudioSource source,AudioClip clip){source.Stop();source.clip=clip;if(clip!=null)source.Play();}
    }
}
