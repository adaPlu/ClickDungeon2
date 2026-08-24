#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.EditorTools
{
    public sealed class ClickDungeonEditorWindow : EditorWindow
    {
        private int _seed=12345; private int _floor=1; private HeroClassId _hero=HeroClassId.Knight; private RouteModifier _route; private Vector2 _scroll; private RunState _state;
        [MenuItem("ClickDungeon/Simulation Inspector")] public static void Open()=>GetWindow<ClickDungeonEditorWindow>("ClickDungeon");
        private void OnGUI()
        {
            GUILayout.Label("Deterministic Simulation Inspector",EditorStyles.boldLabel);_seed=EditorGUILayout.IntField("Seed",_seed);_floor=EditorGUILayout.IntSlider("Floor",_floor,1,50);_hero=(HeroClassId)EditorGUILayout.EnumPopup("Hero",_hero);_route=(RouteModifier)EditorGUILayout.EnumPopup("Route",_route);
            if(GUILayout.Button("Generate")){var g=new FloorGenerator();_state=g.CreateNewRun(unchecked((uint)_seed),_hero);g.GenerateFloor(_state,_floor,_route);}
            if(GUILayout.Button("Validate Content"))ContentValidator.ValidateMenu();if(GUILayout.Button("Run Balance Smoke"))BalanceHarness.RunBatch();
            if(_state==null)return;EditorGUILayout.HelpBox($"Floor {_state.Floor} | {_state.BiomeId} | {_state.ArchetypeId} | Boss={_state.BossRequired} | RNG={_state.FloorRngState}",MessageType.Info);
            _scroll=EditorGUILayout.BeginScrollView(_scroll);for(int r=0;r<5;r++){GUILayout.BeginHorizontal();for(int c=0;c<5;c++){var t=_state.Tiles[r*5+c];GUILayout.Label($"{t.Index}\n{t.ContentId}\n{t.Visibility}",GUILayout.Width(140),GUILayout.Height(54));}GUILayout.EndHorizontal();}EditorGUILayout.EndScrollView();
        }
    }
}
#endif
