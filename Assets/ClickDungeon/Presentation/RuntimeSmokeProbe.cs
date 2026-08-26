using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClickDungeon.Presentation
{
    public sealed class RuntimeSmokeProbe : MonoBehaviour
    {
        private const string Flag = "-cd2RuntimeSmoke";
        private const string Prefix = "[CD2_RUNTIME_SMOKE]";
        private const float TimeoutSeconds = 45f;

        private bool _sawBoot;
        private bool _sawMain;
        private bool _sawGame;
        private bool _finished;
        private string _failure;
        private float _deadline;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
#if !UNITY_EDITOR
            if (!HasFlag()) return;
            var go = new GameObject(nameof(RuntimeSmokeProbe));
            DontDestroyOnLoad(go);
            go.AddComponent<RuntimeSmokeProbe>();
#endif
        }

        private static bool HasFlag()
        {
            foreach (string arg in Environment.GetCommandLineArgs())
                if (string.Equals(arg, Flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private void Awake()
        {
            _deadline = Time.realtimeSinceStartup + TimeoutSeconds;
            SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.Application.logMessageReceived += OnLogMessage;
            Debug.Log($"{Prefix} START");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.Application.logMessageReceived -= OnLogMessage;
        }

        private void Update()
        {
            if (_finished) return;
            if (!string.IsNullOrEmpty(_failure))
            {
                Fail(_failure);
                return;
            }
            if (Time.realtimeSinceStartup > _deadline)
                Fail($"timed out; boot={_sawBoot} main={_sawMain} game={_sawGame}");
        }

        private void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            if (_finished || !string.IsNullOrEmpty(_failure)) return;
            if (type == LogType.Exception || type == LogType.Assert)
                _failure = $"runtime {type}: {condition}";
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_finished) return;
            Debug.Log($"{Prefix} SCENE {scene.name}");

            if (scene.name == "Boot")
            {
                _sawBoot = true;
                return;
            }

            if (scene.name == "Main")
            {
                if (!_sawBoot)
                {
                    _failure = "Main loaded before Boot was observed";
                    return;
                }
                _sawMain = true;
                StartCoroutine(AdvanceFromMain());
                return;
            }

            if (scene.name == "Game")
            {
                if (!_sawBoot || !_sawMain)
                {
                    _failure = "Game loaded before the Boot -> Main path completed";
                    return;
                }
                _sawGame = true;
                StartCoroutine(VerifyGameReady());
            }
        }

        private IEnumerator AdvanceFromMain()
        {
            // Let MainMenuUI.Start() build the real menu before taking the CI-only path into Game.
            yield return null;
            yield return null;
            if (_finished || !string.IsNullOrEmpty(_failure)) yield break;
            if (SceneManager.GetActiveScene().name != "Main") yield break;

            PlayerPrefs.SetInt("cd2.slot", 1);
            PlayerPrefs.SetInt("cd2.continue", 0);
            PlayerPrefs.SetInt("cd2.abyss", 0);
            PlayerPrefs.SetInt("cd2.class", 0);
            PlayerPrefs.SetString("cd2.seed", "424242");
            PlayerPrefs.Save();
            Debug.Log($"{Prefix} MAIN_READY");
            SceneManager.LoadScene("Game");
        }

        private IEnumerator VerifyGameReady()
        {
            // GameBootstrap.Awake() initializes the real simulation/content/UI during scene load.
            yield return null;
            yield return null;
            if (_finished || !string.IsNullOrEmpty(_failure)) yield break;

            var bootstrap = FindFirstObjectByType<GameBootstrap>();
            if (bootstrap == null)
            {
                Fail("Game scene has no GameBootstrap");
                yield break;
            }
            if (bootstrap.Session == null || bootstrap.Content == null)
            {
                Fail("GameBootstrap did not initialize Session and Content");
                yield break;
            }

            Debug.Log($"{Prefix} GAME_READY floor={bootstrap.Session.State.Floor}");
            Pass();
        }

        private void Pass()
        {
            if (_finished) return;
            if (!_sawBoot || !_sawMain || !_sawGame)
            {
                Fail($"incomplete scene path; boot={_sawBoot} main={_sawMain} game={_sawGame}");
                return;
            }
            _finished = true;
            Debug.Log($"{Prefix} PASS");
            StartCoroutine(QuitAfterLogFlush(0));
        }

        private void Fail(string reason)
        {
            if (_finished) return;
            _finished = true;
            Debug.LogError($"{Prefix} FAIL {reason}");
            StartCoroutine(QuitAfterLogFlush(1));
        }

        private static IEnumerator QuitAfterLogFlush(int exitCode)
        {
            yield return null;
            UnityEngine.Application.Quit(exitCode);
        }
    }
}
