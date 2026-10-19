using Core.Minigame;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames.MineGame
{
    public class MineMinigame : MinigameBase
    {
        [SerializeField] private PlayerJoystick _playerJoystick;
        [SerializeField] private Transform _enemy;
        [SerializeField] private Transform _enemyFrontPoint;
        [SerializeField] private MineFieldSpawner _mineField;

        [Header("Points")]
        [SerializeField] private Transform _playerStartPoint;
        [SerializeField] private Transform _finishPoint;
        [SerializeField] private Transform _enemyStartPoint;
        [SerializeField] private Transform _enemyEndPoint;

        [Header("Properties")]
        [SerializeField] private float _deathDelay = 1.5f;
        [SerializeField] private int _maxMineHits = 2;

        [Header("Enemy")]
        [SerializeField] private float _enemySpeed = 1f;
        [SerializeField] private float _enemySpeedMultiplier = 1.2f;
        [SerializeField] private float _catchDistance = 0.5f;

        [Header("Scan")]
        [SerializeField] private float _searchRadius = 3f;
        [SerializeField] private float _scanCooldown = 2f;
        [SerializeField] private LayerMask _mineLayer = ~0;
        [SerializeField] private CanvasGroup _scanEffectCanvasGroup;

        [Header("Scan Ring (World Space Canvas)")]
        [SerializeField] private Image _scanRingImage;
        [SerializeField] private float _scanRingGrowDuration = 0.35f;
        [SerializeField] private float _scanRingFadeDuration = 0.4f;
        [SerializeField, Range(0f, 1f)] private float _scanRingMaxAlpha = 0.8f;
        [SerializeField] private float _scanRingHeightOffset = 0.02f;

        [Header("Score")]
        [SerializeField] private int _finishScore = 60;
        [SerializeField] private int _maxTimeBonus = 40;
        [SerializeField] private int _maxProgressScore = 40;
        [SerializeField] private int _penaltyPerMineHit = 10;

        [Header("UI")]
        [SerializeField] private Button _scanButton;
        [SerializeField] private Image _scanCooldownFill;

        private int _mineHitCount;
        private float _currentEnemySpeed;
        private float _scanCooldownLeft;
        private bool _isRoundOver;
        private bool _hasReachedFinish;
        private float _finishTimeLeft;

        protected override void Awake()
        {
            base.Awake();

            _playerJoystick.OnTriggeredMine += _handleTriggeredMine;
            _playerJoystick.OnReachedFinish += _handleReachedFinish;
            _scanButton.onClick.AddListener(_scanAround);

            _mineField.Spawn();

            _playerJoystick.SetCanMove(false);
            _playerJoystick.Teleport(_playerStartPoint.position);
            _enemy.position = _enemyStartPoint.position;

            if (_scanEffectCanvasGroup != null)
                _scanEffectCanvasGroup.alpha = 0f;
            if (_scanRingImage != null)
                _scanRingImage.gameObject.SetActive(false);
        }

        protected override void OnBegin()
        {
            _mineHitCount = 0;
            _currentEnemySpeed = _enemySpeed;
            _scanCooldownLeft = 0f;
            _isRoundOver = false;
            _hasReachedFinish = false;

            _scanButton.interactable = true;
            if (_scanCooldownFill != null)
                _scanCooldownFill.fillAmount = 1f;

            _playerJoystick.SetCanMove(true);
        }

        protected override void OnTick()
        {
            if (_isRoundOver)
                return;

            _updateScanCooldown();
            _moveEnemy();

            if (_isPlayerCaught())
                _handleEnemyReached();
        }

        protected override void OnEnd()
        {
            _isRoundOver = true;
            _playerJoystick.SetCanMove(false);
            _scanButton.interactable = false;
        }

        #region Enemy

        private void _moveEnemy()
        {
            _enemy.position = Vector3.MoveTowards(_enemy.position, _enemyEndPoint.position, _currentEnemySpeed * Time.deltaTime);
        }

        private bool _isPlayerCaught()
        {
            Vector3 enemyDirection = (_enemyEndPoint.position - _enemyStartPoint.position).normalized;
            float distanceAhead = Vector3.Dot(_playerJoystick.transform.position - _enemyFrontPoint.position, enemyDirection);
            return distanceAhead <= _catchDistance;
        }

        #endregion

        #region Scan

        private void _scanAround()
        {
            if (!IsRunning || _isRoundOver || _scanCooldownLeft > 0f)
                return;

            _scanCooldownLeft = _scanCooldown;
            _currentEnemySpeed *= _enemySpeedMultiplier;
            _scanButton.interactable = false;

            Vector3 center = _playerJoystick.transform.position;
            _playScanEffect(center);

            var hits = Physics.OverlapSphere(center, _searchRadius, _mineLayer, QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                var mine = hit.GetComponentInParent<Mine>();
                if (mine != null)
                    mine.ShowMine();
            }
        }

        private void _updateScanCooldown()
        {
            if (_scanCooldownLeft <= 0f)
                return;

            _scanCooldownLeft = Mathf.Max(0f, _scanCooldownLeft - Time.deltaTime);

            if (_scanCooldownFill != null)
                _scanCooldownFill.fillAmount = 1f - _scanCooldownLeft / _scanCooldown;

            if (_scanCooldownLeft <= 0f)
                _scanButton.interactable = true;
        }

        private void _playScanEffect(Vector3 center)
        {
            if (_scanEffectCanvasGroup != null)
            {
                _scanEffectCanvasGroup.DOKill();
                _scanEffectCanvasGroup.alpha = 0f;
                DOTween.Sequence()
                    .Append(_scanEffectCanvasGroup.DOFade(0.1f, 0.08f))
                    .Append(_scanEffectCanvasGroup.DOFade(0f, 0.4f).SetEase(Ease.OutQuad))
                    .SetTarget(_scanEffectCanvasGroup)
                    .SetLink(gameObject);
            }

            if (_scanRingImage != null)
                _playScanRing(center);
        }

        private void _playScanRing(Vector3 center)
        {
            var ring = _scanRingImage.rectTransform;
            ring.DOKill();
            _scanRingImage.DOKill();

            ring.position = center + Vector3.up * _scanRingHeightOffset;
            ring.localScale = Vector3.zero;
            _scanRingImage.color = _withAlpha(_scanRingImage.color, 0f);
            _scanRingImage.gameObject.SetActive(true);

            float baseWorldSize = ring.rect.width * ring.parent.lossyScale.x;
            float targetScale = _searchRadius * 2f / baseWorldSize;

            DOTween.Sequence()
                .Append(ring.DOScale(targetScale, _scanRingGrowDuration).SetEase(Ease.OutCubic))
                .Join(_scanRingImage.DOFade(_scanRingMaxAlpha, _scanRingGrowDuration).SetEase(Ease.OutQuad))
                .Append(_scanRingImage.DOFade(0f, _scanRingFadeDuration).SetEase(Ease.InQuad))
                .AppendCallback(() => _scanRingImage.gameObject.SetActive(false))
                .SetTarget(ring)
                .SetLink(gameObject);
        }

        private static Color _withAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        #endregion

        #region Round End

        private void _handleTriggeredMine(Mine mine)
        {
            if (!IsRunning || _isRoundOver || !mine.Explode())
                return;

            _mineHitCount++;
            if (_mineHitCount >= _maxMineHits)
            {
                _playerJoystick.PlayDeathAnimation();
                _endRound();
            }
            else
            {
                _playerJoystick.PlayDamagedAnimation();
            }
        }

        private void _handleReachedFinish()
        {
            if (!IsRunning || _isRoundOver)
                return;

            _hasReachedFinish = true;
            _finishTimeLeft = TimeLeft;
            _playerJoystick.PlayFinishedAnimation();
            _endRound();
        }

        private void _handleEnemyReached()
        {
            _playerJoystick.PlayDeathAnimation();
            _endRound();
        }

        private void _endRound()
        {
            _isRoundOver = true;
            _playerJoystick.SetCanMove(false);
            _scanButton.interactable = false;

            DOVirtual.DelayedCall(_deathDelay, Finish).SetLink(gameObject);
        }

        #endregion

        protected override int CalculateScore()
        {
            int score;

            if (_hasReachedFinish)
            {
                float timeRatio = HasTimer ? _finishTimeLeft / Duration : 0f;
                score = _finishScore + Mathf.RoundToInt(timeRatio * _maxTimeBonus);
            }
            else
            {
                score = Mathf.RoundToInt(_getProgress01() * _maxProgressScore);
            }

            score -= _mineHitCount * _penaltyPerMineHit;
            return Mathf.Max(_minScore, score);
        }

        private float _getProgress01()
        {
            Vector3 path = _finishPoint.position - _playerStartPoint.position;
            Vector3 traveled = _playerJoystick.transform.position - _playerStartPoint.position;
            return Mathf.Clamp01(Vector3.Dot(traveled, path) / path.sqrMagnitude);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _playerJoystick.OnTriggeredMine -= _handleTriggeredMine;
            _playerJoystick.OnReachedFinish -= _handleReachedFinish;
            _scanButton.onClick.RemoveListener(_scanAround);
        }

        private void OnDrawGizmos()
        {
            if (_playerJoystick == null)
                return;

            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
            Gizmos.DrawWireSphere(_playerJoystick.transform.position, _searchRadius);
        }
    }
}
