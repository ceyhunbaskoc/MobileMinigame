using System;
using System.Collections;
using Core.Minigame;
using Core.SceneManagement;
using DG.Tweening;
using UI.Minigame;
using UnityEngine;

namespace Core
{
    public class GameFlowManager : IDisposable
    {
        private readonly FadeController _fadeController;
        private readonly UIManager _uiManager;
        private readonly SceneController _sceneController;
        private readonly MinigameSessionRandomize _sessionRandomize;

        private int? _pendingScore;

        public int TotalScore { get; private set; }

        public GameFlowManager(FadeController fadeController,
            UIManager uiManager,
            SceneController sceneController,
            MinigameSessionRandomize sessionRandomize)
        {
            _fadeController = fadeController;
            _uiManager = uiManager;
            _sceneController = sceneController;
            _sessionRandomize = sessionRandomize;

            MinigameBase.OnFinished += HandleMinigameFinished;
        }

        public IEnumerator RunSession()
        {
            TotalScore = 0;

            yield return _fadeController.FadeOut().WaitForCompletion();
            _uiManager.HideAll();

            var order = _sessionRandomize.CreateOrder();
            if (order.Count == 0)
                Debug.LogWarning("[GameFlowManager] Minigame list is empty.");

            _uiManager.SetupSession(order);

            for (int i = 0; i < order.Count; i++)
            {
                var sceneName = order[i].SceneName;
                _pendingScore = null;

                yield return _sceneController.LoadMinigame(sceneName);
                if (!_sceneController.HasLoadedScene)
                    continue;

                _uiManager.ShowMinigameBeginning(i);
                yield return _fadeController.FadeIn().WaitForCompletion();
                yield return _uiManager.PlayMinigameBeginning();

                if (MinigameBase.Current != null)
                    MinigameBase.Current.Begin();
                else
                    Debug.LogError($"[GameFlowManager] No MinigameBase found in '{sceneName}'.");

                yield return new WaitUntil(() => _pendingScore.HasValue);
                int score = _pendingScore.Value;
                TotalScore += score;

                yield return _uiManager.ShowRoundScore(score, TotalScore);

                yield return _fadeController.FadeOut().WaitForCompletion();
                _uiManager.HideAll();
                Time.timeScale = 1f;

                yield return _sceneController.UnloadCurrent();
            }
        }

        private void HandleMinigameFinished(int score)
        {
            _pendingScore = score;
        }

        public void Dispose()
        {
            MinigameBase.OnFinished -= HandleMinigameFinished;
        }
    }
}
