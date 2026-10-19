using System.Collections;
using Core.Minigame;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames.Newspaper
{
    public class NewspaperMinigame : MinigameBase
    {
        [SerializeField] private NewspaperReader _reader;
        [SerializeField] private PlayerEater _player;
        [SerializeField] private Button _eatButton;

        [Header("Food")]
        [SerializeField] private GameObject[] _foodPieces;
        [SerializeField] private int _bitesToFinish = 20;

        [Header("Reader Timing")]
        [SerializeField] private Vector2 _readDurationRange = new Vector2(1.5f, 3.5f);
        [SerializeField] private float _warningDuration = 0.6f;
        [SerializeField] private Vector2 _watchDurationRange = new Vector2(1f, 2f);
        [SerializeField] private float _raiseDuration = 0.4f;
        [SerializeField] private float _readSpeedUpPerCycle = 0.93f;

        [Header("Fake Out")]
        [SerializeField, Range(0f, 1f)] private float _fakeOutChance = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _fakeOutLowerAmount = 0.35f;

        [Header("End")]
        [SerializeField] private float _endDelay = 1.5f;

        [Header("Score")]
        [SerializeField] private int _maxProgressScore = 80;
        [SerializeField] private int _maxTimeBonus = 20;

        private Coroutine _readerRoutine;
        private int _biteCount;
        private bool _isWatching;
        private bool _isRoundOver;
        private float _finishTimeLeft;
        private float _readDurationMultiplier;

        protected override void Awake()
        {
            base.Awake();
            _eatButton.onClick.AddListener(_onEatClicked);
            _updateFood();
        }

        protected override void OnBegin()
        {
            _biteCount = 0;
            _isWatching = false;
            _isRoundOver = false;
            _readDurationMultiplier = 1f;

            _updateFood();
            _eatButton.interactable = true;
            _readerRoutine = StartCoroutine(_readerLoop());
        }

        protected override void OnTick()
        {
            if (!_isRoundOver && _isWatching && _player.IsEating)
                _onCaught();
        }

        protected override void OnEnd()
        {
            _stopReader();
            _eatButton.interactable = false;
        }

        #region Reader

        private IEnumerator _readerLoop()
        {
            while (true)
            {
                float readDuration = Random.Range(_readDurationRange.x, _readDurationRange.y) * _readDurationMultiplier;
                yield return new WaitForSeconds(readDuration);

                if (Random.value < _fakeOutChance)
                {
                    yield return _reader.TweenLowerAmount(_fakeOutLowerAmount, _warningDuration * 0.6f).WaitForCompletion();
                    yield return _reader.TweenLowerAmount(0f, _raiseDuration).WaitForCompletion();
                    continue;
                }

                yield return _reader.TweenLowerAmount(1f, _warningDuration).WaitForCompletion();

                _isWatching = true;
                yield return new WaitForSeconds(Random.Range(_watchDurationRange.x, _watchDurationRange.y));
                _isWatching = false;

                yield return _reader.TweenLowerAmount(0f, _raiseDuration).WaitForCompletion();
                _readDurationMultiplier *= _readSpeedUpPerCycle;
            }
        }

        private void _stopReader()
        {
            _isWatching = false;
            if (_readerRoutine != null)
            {
                StopCoroutine(_readerRoutine);
                _readerRoutine = null;
            }
        }

        #endregion

        #region Eating

        private void _onEatClicked()
        {
            if (!IsRunning || _isRoundOver)
                return;

            _player.TryBite(_onBite);
        }

        private void _onBite()
        {
            if (_isRoundOver)
                return;

            _biteCount++;
            _updateFood();

            if (_biteCount >= _bitesToFinish)
                _onPlateFinished();
        }

        private void _updateFood()
        {
            float remaining = 1f - (float)_biteCount / _bitesToFinish;
            int visibleCount = Mathf.CeilToInt(remaining * _foodPieces.Length);

            for (int i = 0; i < _foodPieces.Length; i++)
                _foodPieces[i].SetActive(i < visibleCount);
        }

        #endregion

        #region Round End

        private void _onPlateFinished()
        {
            _isRoundOver = true;
            _finishTimeLeft = TimeLeft;
            _stopReader();
            _eatButton.interactable = false;

            DOVirtual.DelayedCall(_endDelay, Finish).SetLink(gameObject);
        }

        private void _onCaught()
        {
            _isRoundOver = true;
            _stopReader();
            _eatButton.interactable = false;

            _reader.PlayShoot().OnComplete(() =>
            {
                _player.PlayDeath();
                DOVirtual.DelayedCall(_endDelay, Finish).SetLink(gameObject);
            });
        }

        #endregion

        protected override int CalculateScore()
        {
            float progress = Mathf.Clamp01((float)_biteCount / _bitesToFinish);
            int score = Mathf.RoundToInt(progress * _maxProgressScore);

            if (_biteCount >= _bitesToFinish && HasTimer)
                score += Mathf.RoundToInt(_finishTimeLeft / Duration * _maxTimeBonus);

            return Mathf.Max(_minScore, score);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _eatButton.onClick.RemoveListener(_onEatClicked);
        }
    }
}
