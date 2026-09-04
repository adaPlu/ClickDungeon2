using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ClickDungeon.Presentation.Assets;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Presentation.UI
{
    /// <summary>
    /// Additive runtime bridge for the production-art remaster. It keeps simulation authority in
    /// GameSession, then layers dedicated hero portrait/gameplay/reaction art over the existing
    /// deterministic 5x5 UI without changing save or class IDs.
    /// </summary>
    public sealed class VisualRemasterRuntimeBridge : MonoBehaviour
    {
        private enum HeroVisualState { Idle, Attack, Hit, Victory, Defeat }

        private GameBootstrap _game;
        private RuntimeGameUI _ui;
        private PresentationAssetDatabase _assets;
        private Image _portrait;
        private Image[] _tileIcons;
        private HeroVisualState _transientState;
        private float _transientUntil;
        private int _lastPlayerIndex = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneHook()
        {
            SceneManager.sceneLoaded -= InstallForScene;
            SceneManager.sceneLoaded += InstallForScene;
        }

        private static void InstallForScene(Scene scene, LoadSceneMode mode)
        {
            var game = FindObjectOfType<GameBootstrap>();
            if (game != null && game.GetComponent<VisualRemasterRuntimeBridge>() == null)
                game.gameObject.AddComponent<VisualRemasterRuntimeBridge>();
        }

        private void Start()
        {
            _game = GetComponent<GameBootstrap>();
            _ui = GetComponent<RuntimeGameUI>();
            _assets = Resources.Load<PresentationAssetDatabase>("ClickDungeonPresentationAssets");
            CacheUiReferences();
            if (_ui != null)
            {
                _ui.CommandExecuted += OnCommandExecuted;
                _ui.CommandResolved += OnCommandResolved;
                _ui.StateChanged += RefreshVisuals;
            }
            RefreshVisuals();
        }

        private void OnDestroy()
        {
            if (_ui == null) return;
            _ui.CommandExecuted -= OnCommandExecuted;
            _ui.CommandResolved -= OnCommandResolved;
            _ui.StateChanged -= RefreshVisuals;
        }

        private void Update()
        {
            if (_game?.Session == null) return;
            if (_portrait == null || _tileIcons == null) CacheUiReferences();
            RefreshVisuals();
            AnimateCurrentHero();
        }

        private void CacheUiReferences()
        {
            Transform root = transform.Find("ClickDungeonCanvas/SafeRoot");
            if (root == null) return;
            _portrait = root.Find("InfoPanel/HeroPortrait")?.GetComponent<Image>();
            Transform board = root.Find("Board");
            if (board == null) return;
            _tileIcons = new Image[RunState.BoardSize * RunState.BoardSize];
            for (int i = 0; i < _tileIcons.Length; i++)
                _tileIcons[i] = board.Find($"Tile_{i}/Icon")?.GetComponent<Image>();
        }

        private void OnCommandExecuted(GameCommand command, CommandResult result)
        {
            if (result == null || !result.Accepted) return;
            if (command is AttackCommand || command is UseAbilityCommand)
                SetTransient(HeroVisualState.Attack, .24f);
        }

        private void OnCommandResolved(CommandResult result)
        {
            if (_game?.Session == null || result == null) return;
            foreach (var evt in result.Events)
            {
                if (evt.Type == "player.damaged" || evt.Type == "trap.triggered")
                {
                    SetTransient(HeroVisualState.Hit, .30f);
                    break;
                }
            }
            if (_game.Session.State.GameOver) SetTransient(HeroVisualState.Defeat, 999f);
            else if (_game.Session.State.CampaignCompleted) SetTransient(HeroVisualState.Victory, 999f);
        }

        private void SetTransient(HeroVisualState state, float duration)
        {
            _transientState = state;
            _transientUntil = Time.unscaledTime + duration;
        }

        private HeroVisualState CurrentState()
        {
            if (_game?.Session == null) return HeroVisualState.Idle;
            if (_game.Session.State.GameOver) return HeroVisualState.Defeat;
            if (_game.Session.State.CampaignCompleted) return HeroVisualState.Victory;
            return Time.unscaledTime < _transientUntil ? _transientState : HeroVisualState.Idle;
        }

        private void RefreshVisuals()
        {
            if (_game?.Session == null || _assets == null) return;
            HeroClassId heroClass = _game.Session.State.HeroClass;
            if (_portrait != null)
            {
                Sprite portrait = HeroPresentationAssets.Portrait(_assets, heroClass);
                if (portrait != null)
                {
                    _portrait.sprite = portrait;
                    _portrait.enabled = true;
                    _portrait.color = Color.white;
                }
            }

            if (_tileIcons == null) return;
            int playerIndex = _game.Session.State.PlayerPosition.Row * RunState.BoardSize + _game.Session.State.PlayerPosition.Col;
            if (playerIndex < 0 || playerIndex >= _tileIcons.Length) return;
            _lastPlayerIndex = playerIndex;
            Image icon = _tileIcons[playerIndex];
            if (icon == null) return;
            Sprite sprite = SpriteForState(heroClass, CurrentState());
            if (sprite == null) return;
            icon.sprite = sprite;
            icon.enabled = true;
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.transform.SetAsLastSibling();
        }

        private Sprite SpriteForState(HeroClassId heroClass, HeroVisualState state)
        {
            string baseId = HeroPresentationAssets.BaseId(heroClass);
            switch (state)
            {
                case HeroVisualState.Attack:
                    return _assets.SpriteFor(baseId + ".attack") ?? HeroPresentationAssets.Gameplay(_assets, heroClass);
                case HeroVisualState.Hit:
                    return _assets.SpriteFor(baseId + ".hit") ?? HeroPresentationAssets.Gameplay(_assets, heroClass);
                case HeroVisualState.Victory:
                    return HeroPresentationAssets.Victory(_assets, heroClass);
                case HeroVisualState.Defeat:
                    return HeroPresentationAssets.Defeat(_assets, heroClass);
                default:
                    return _assets.SpriteFor(baseId + ".idle") ?? HeroPresentationAssets.Gameplay(_assets, heroClass);
            }
        }

        private void AnimateCurrentHero()
        {
            if (_tileIcons == null || _lastPlayerIndex < 0 || _lastPlayerIndex >= _tileIcons.Length) return;
            Image icon = _tileIcons[_lastPlayerIndex];
            if (icon == null) return;
            float t = Time.unscaledTime;
            HeroVisualState state = CurrentState();
            float scale = 1f;
            float rotation = 0f;
            switch (state)
            {
                case HeroVisualState.Attack:
                    scale = 1.12f + Mathf.Sin(t * 38f) * .05f;
                    rotation = Mathf.Sin(t * 32f) * 4f;
                    break;
                case HeroVisualState.Hit:
                    scale = .94f + Mathf.Sin(t * 30f) * .025f;
                    rotation = Mathf.Sin(t * 48f) * 7f;
                    break;
                case HeroVisualState.Victory:
                    scale = 1.10f + Mathf.Sin(t * 7f) * .05f;
                    rotation = Mathf.Sin(t * 5f) * 2f;
                    break;
                case HeroVisualState.Defeat:
                    scale = .94f;
                    rotation = -6f;
                    break;
                default:
                    scale = 1.03f + Mathf.Sin(t * 4.2f) * .025f;
                    rotation = Mathf.Sin(t * 2.2f) * .8f;
                    break;
            }
            icon.rectTransform.localScale = Vector3.one * scale;
            icon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }
    }
}
