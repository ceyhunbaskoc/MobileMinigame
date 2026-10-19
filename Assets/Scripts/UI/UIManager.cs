using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Minigame
{
    public class UIManager : MonoBehaviour
    {
        [Header("Main Menu")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private Button _playButton;

        [Header("Minigame Beginning")]
        [SerializeField] private MinigameBeginningUI _minigameBeginningUI;

        [Header("Round Score")]
        [SerializeField] private GameObject _roundScorePanel;
        [SerializeField] private TMP_Text _roundScoreText;
        [SerializeField] private TMP_Text _totalScoreText;
        [SerializeField] private float _roundScoreDuration = 2f;

        [Header("Finish")]
        [SerializeField] private GameObject _finishPanel;
        [SerializeField] private TMP_Text _finalScoreText;
        [SerializeField] private Button _mainMenuButton;

        public event Action OnPlayRequested;
        public event Action OnMainMenuRequested;

        private void Awake()
        {
            _playButton.onClick.AddListener(HandlePlayClicked);
            _mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
            _minigameBeginningUI.Initialize();
            HideAll();
        }

        public void HideAll()
        {
            _mainMenuPanel.SetActive(false);
            _roundScorePanel.SetActive(false);
            _finishPanel.SetActive(false);
            _minigameBeginningUI.Hide();
        }

        public void ShowMainMenu()
        {
            HideAll();
            _mainMenuPanel.SetActive(true);
            _playButton.interactable = true;
        }

        public void SetupSession(List<MinigameDefinition> sessionOrder)
        {
            _minigameBeginningUI.SetupSession(sessionOrder);
        }

        public void ShowMinigameBeginning(int index)
        {
            HideAll();
            _minigameBeginningUI.Show(index);
        }

        public IEnumerator PlayMinigameBeginning()
        {
            yield return _minigameBeginningUI.Play();
        }

        public IEnumerator ShowRoundScore(int score, int totalScore)
        {
            HideAll();
            _roundScoreText.text = $"+{score}";
            _totalScoreText.text = $"Total Score: {totalScore}";
            _roundScorePanel.SetActive(true);

            var panel = _roundScorePanel.transform;
            panel.DOKill();
            panel.localScale = Vector3.one * 0.8f;
            yield return panel.DOScale(1f, 0.3f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .WaitForCompletion();

            yield return new WaitForSecondsRealtime(_roundScoreDuration);
        }

        public void ShowFinish(int totalScore)
        {
            HideAll();
            _finalScoreText.text = totalScore.ToString();
            _finishPanel.SetActive(true);
            _mainMenuButton.interactable = true;
        }

        private void HandlePlayClicked()
        {
            _playButton.interactable = false;
            OnPlayRequested?.Invoke();
        }

        private void HandleMainMenuClicked()
        {
            _mainMenuButton.interactable = false;
            OnMainMenuRequested?.Invoke();
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(HandlePlayClicked);
            _mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
        }
    }
}
