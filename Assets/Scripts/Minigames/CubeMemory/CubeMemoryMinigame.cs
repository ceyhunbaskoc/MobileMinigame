using System;
using System.Collections;
using System.Collections.Generic;
using Core.Minigame;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Minigames.CubeMemory
{
    public class CubeMemoryMinigame : MinigameBase
    {
        [Header("Grids")]
        [SerializeField] private CubeGrid _playerGrid;
        [SerializeField] private CubeGrid _referenceGrid;

        [Header("Reference View")]
        [SerializeField] private Camera _referenceCamera;
        [SerializeField] private CanvasGroup _referenceCanvasGroup;
        [SerializeField] private float _referenceFadeDuration = 0.2f;

        [Header("3D Buttons")]
        [SerializeField] private PressableButton3D _doneButton;
        [SerializeField] private PressableButton3D _resetButton;

        [Header("Stages")]
        [SerializeField] private int[] _hollowCountPerStage = { 2, 3, 5, 7 };
        [SerializeField] private float _memorizeDuration = 3f;
        [SerializeField] private float _carveDuration = 15f;
        [SerializeField] private float _graceDuration = 5f;
        [SerializeField] private float _confirmDelay = 0.3f;
        [SerializeField] private float _resultDuration = 1.5f;
        [SerializeField] private int _maxScore = 100;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _phaseText;
        [SerializeField] private TextMeshProUGUI _phaseTimerText;
        [SerializeField] private TextMeshProUGUI _stageText;

        private readonly HashSet<Vector3Int> _targetPattern = new HashSet<Vector3Int>();
        private float _totalScore;
        private bool _canCarve;
        private bool _isDonePressed;
        private bool _isDoneConfirmed;
        private float _donePressedTime;
        private int _shownTimerSeconds = -1;

        protected override void Awake()
        {
            base.Awake();

            _playerGrid.OnCellClicked += _onCellClicked;
            _doneButton.OnClicked += _onDoneClicked;
            _resetButton.OnClicked += _onResetClicked;

            _setCarving(false);
            _referenceCanvasGroup.alpha = 0f;
            _referenceCamera.enabled = false;
            _phaseTimerText.text = string.Empty;
        }

        protected override void OnBegin()
        {
            _totalScore = 0f;
            StartCoroutine(_runStages());
        }

        protected override void OnEnd()
        {
            StopAllCoroutines();
            _setCarving(false);
        }

        private IEnumerator _runStages()
        {
            int stageCount = _hollowCountPerStage.Length;

            for (int stage = 0; stage < stageCount; stage++)
            {
                _stageText.text = $"{stage + 1}/{stageCount}";
                _createPattern(_hollowCountPerStage[stage]);
                _referenceGrid.ApplyPattern(_targetPattern, false);
                _playerGrid.ResetCells(true);

                _phaseText.text = "Memorize!";
                _setReferenceVisible(true);
                yield return _runTimer(_memorizeDuration);
                _setReferenceVisible(false);

                _phaseText.text = "Carve!";
                _isDonePressed = false;
                _isDoneConfirmed = false;
                _setCarving(true);
                yield return _runTimer(_carveDuration, () => _isDonePressed);

                if (_isDonePressed)
                {
                    _phaseText.text = "Last chance!";
                    yield return _runTimer(_graceDuration, () => _isDoneConfirmed);
                }

                _setCarving(false);

                float accuracy = _evaluate();
                _totalScore += accuracy * _maxScore / stageCount;
                _phaseText.text = accuracy >= 1f ? "Perfect!" : $"%{Mathf.RoundToInt(accuracy * 100f)}";

                _setReferenceVisible(true);
                yield return _runTimer(_resultDuration, null, false);
                _setReferenceVisible(false);
            }

            Finish();
        }

        private IEnumerator _runTimer(float duration, Func<bool> stopCondition = null, bool showTimer = true)
        {
            float timeLeft = duration;
            while (timeLeft > 0f)
            {
                if (stopCondition != null && stopCondition())
                    break;

                _setTimerText(showTimer ? timeLeft : 0f, showTimer);
                timeLeft -= Time.deltaTime;
                yield return null;
            }

            _setTimerText(0f, false);
        }

        private void _setTimerText(float timeLeft, bool isVisible)
        {
            int seconds = isVisible ? Mathf.CeilToInt(timeLeft) : -1;
            if (seconds == _shownTimerSeconds)
                return;

            _shownTimerSeconds = seconds;
            _phaseTimerText.text = isVisible ? seconds.ToString() : string.Empty;
        }

        private void _createPattern(int hollowCount)
        {
            var candidates = new List<Vector3Int>(_playerGrid.VisibleCoords);
            for (int i = 0; i < candidates.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, candidates.Count);
                (candidates[i], candidates[randomIndex]) = (candidates[randomIndex], candidates[i]);
            }

            _targetPattern.Clear();
            int count = Mathf.Min(hollowCount, candidates.Count);
            for (int i = 0; i < count; i++)
                _targetPattern.Add(candidates[i]);
        }

        private float _evaluate()
        {
            var carved = _playerGrid.GetHollowCoords();

            int correct = 0;
            foreach (var coord in carved)
            {
                if (_targetPattern.Contains(coord))
                    correct++;
            }

            int wrong = carved.Count - correct;
            return Mathf.Clamp01((float)(correct - wrong) / _targetPattern.Count);
        }

        private void _setCarving(bool canCarve)
        {
            _canCarve = canCarve;
            _playerGrid.IsInteractable = canCarve;
            _doneButton.Interactable = canCarve;
            _resetButton.Interactable = canCarve;
        }

        private void _setReferenceVisible(bool isVisible)
        {
            _referenceCanvasGroup.DOKill();

            if (isVisible)
                _referenceCamera.enabled = true;

            _referenceCanvasGroup.DOFade(isVisible ? 1f : 0f, _referenceFadeDuration)
                .OnComplete(() => _referenceCamera.enabled = isVisible)
                .SetLink(gameObject);
        }

        private void _onCellClicked(CubeCell cell)
        {
            if (!_canCarve)
                return;

            cell.SetHollow(true, true);
        }

        private void _onDoneClicked()
        {
            if (!_canCarve)
                return;

            if (!_isDonePressed)
            {
                _isDonePressed = true;
                _donePressedTime = Time.time;
                return;
            }

            if (Time.time - _donePressedTime >= _confirmDelay)
                _isDoneConfirmed = true;
        }

        private void _onResetClicked()
        {
            if (!_canCarve)
                return;

            _playerGrid.ResetCells(true);
        }

        protected override int CalculateScore()
        {
            return Mathf.Max(_minScore, Mathf.RoundToInt(_totalScore));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _playerGrid.OnCellClicked -= _onCellClicked;
            _doneButton.OnClicked -= _onDoneClicked;
            _resetButton.OnClicked -= _onResetClicked;
        }
    }
}
