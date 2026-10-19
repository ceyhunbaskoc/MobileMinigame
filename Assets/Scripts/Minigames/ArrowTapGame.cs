using Core.Minigame;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class ArrowTapGame : MinigameBase
    {
        [Header("UI")]
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;
        [Space]
        [SerializeField] private Image _currentArrowImage;
        [SerializeField] private Sprite _baseArrowSprite;

        [Header("Running Band")]
        [SerializeField] private Renderer _runningBandRenderer;
        [SerializeField] private float _bandScrollMultiplier = 1f;

        [Header("Character")]
        [SerializeField] private Transform _character;
        [SerializeField] private Animator _characterAnimator;
        [SerializeField] private Transform _backLimit;
        [SerializeField] private Transform _frontLimit;
        [SerializeField, Range(0f, 1f)] private float _startProgress = 0.6f;

        [Header("Fall")]
        [SerializeField] private float _fallOverhang = 0.3f;
        [SerializeField] private float _fallDistance = 4f;
        [SerializeField] private float _fallDuration = 0.6f;
        [SerializeField] private float _fallTumbleAngle = 70f;
        [SerializeField] private float _fallDelay = 0.4f;

        [Header("Balance (progress per second, 0 = back edge, 1 = front)")]
        [SerializeField] private float _startDriftSpeed = 0.08f;
        [SerializeField] private float _maxDriftSpeed = 0.2f;
        [SerializeField] private float _driftRampDuration = 30f;
        [SerializeField] private float _pushPerCorrect = 0.07f;
        [SerializeField] private float _knockbackPerWrong = 0.05f;
        [SerializeField] private float _pushReleaseSpeed = 0.35f;

        private static readonly int StartHash = Animator.StringToHash("Run");
        private static readonly int FallHash = Animator.StringToHash("Fall");

        private const int REWARD_PER_CORRECT_ARROW = 10;
        private const int PENALTY_PER_WRONG_ARROW = 5;

        private ArrowDirection _currentCorrectArrow;
        private int _correctArrowCount;
        private int _wrongArrowCount;

        private Material _bandMaterial;
        private float _bandOffset;

        private float _progress;
        private float _pendingPush;
        private float _elapsed;
        private float _currentDriftSpeed;
        private bool _hasFallen;

        private enum ArrowDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        protected override void Awake()
        {
            base.Awake();

            _upButton.onClick.AddListener(() => _onArrowClicked(ArrowDirection.Up));
            _downButton.onClick.AddListener(() => _onArrowClicked(ArrowDirection.Down));
            _leftButton.onClick.AddListener(() => _onArrowClicked(ArrowDirection.Left));
            _rightButton.onClick.AddListener(() => _onArrowClicked(ArrowDirection.Right));

            if (_runningBandRenderer != null)
                _bandMaterial = _runningBandRenderer.material;

            _currentArrowImage.sprite = _baseArrowSprite;
            _currentArrowImage.enabled = false;

            _progress = _startProgress;
            _updateCharacterPosition();
        }

        protected override void OnBegin()
        {
            _progress = _startProgress;
            _pendingPush = 0f;
            _elapsed = 0f;
            _hasFallen = false;
            _correctArrowCount = 0;
            _wrongArrowCount = 0;

            _currentCorrectArrow = (ArrowDirection)Random.Range(0, 4);
            _selectRandomArrow();
            _characterAnimator.SetBool(StartHash, true);
        }

        protected override void OnTick()
        {
            float dt = Time.deltaTime;
            _elapsed += dt;

            float driftSpeed = Mathf.Lerp(_startDriftSpeed, _maxDriftSpeed, _elapsed / _driftRampDuration);
            _currentDriftSpeed = driftSpeed;
            _scrollBand(driftSpeed * dt);

            if (_hasFallen)
                return;

            float maxStep = _pushReleaseSpeed * dt;
            float pushStep = Mathf.Clamp(_pendingPush, -maxStep, maxStep);
            _pendingPush -= pushStep;

            _progress = Mathf.Min(1f, _progress + pushStep - driftSpeed * dt);
            if (_progress >= 1f && _pendingPush > 0f)
                _pendingPush = 0f;

            _updateCharacterPosition();

            if (_progress <= 0f)
                _fall();
        }

        private void _updateCharacterPosition()
        {
            _character.position = Vector3.Lerp(_backLimit.position, _frontLimit.position, _progress);
        }

        private void _scrollBand(float amount)
        {
            if (_bandMaterial == null)
                return;

            _bandOffset = Mathf.Repeat(_bandOffset + amount * _bandScrollMultiplier, 1f);
            _bandMaterial.mainTextureOffset = new Vector2(0f, -_bandOffset);
        }
        
        protected override void OnEnd()
        {
            _characterAnimator.SetBool(StartHash, false);
        }

        private void _fall()
        {
            _progress = 0f;
            _hasFallen = true;
            _currentArrowImage.enabled = false;
            _characterAnimator.SetTrigger(FallHash);

            _playFallAnimation()
                .AppendInterval(_fallDelay)
                .OnComplete(Finish);
        }

        private Sequence _playFallAnimation()
        {
            Vector3 backDirection = (_backLimit.position - _frontLimit.position).normalized;
            Vector3 edgePosition = _backLimit.position + backDirection * _fallOverhang;
            Vector3 groundPosition = edgePosition + Vector3.down * _fallDistance;

            Vector3 tumbleAxis = Vector3.Cross(Vector3.up, backDirection);
            Quaternion tumbleRotation = Quaternion.AngleAxis(_fallTumbleAngle, tumbleAxis) * _character.rotation;

            float bandLength = Vector3.Distance(_backLimit.position, _frontLimit.position);
            float slideSpeed = Mathf.Max(_currentDriftSpeed * bandLength, 0.01f);
            float slideDuration = Mathf.Min(_fallOverhang / slideSpeed, 0.4f);

            _character.DOKill();
            return DOTween.Sequence()
                .Append(_character.DOMove(edgePosition, slideDuration).SetEase(Ease.Linear))
                .Append(_character.DOMove(groundPosition, _fallDuration).SetEase(Ease.InQuad))
                .Join(_character.DORotateQuaternion(tumbleRotation, _fallDuration).SetEase(Ease.OutQuad))
                .SetLink(gameObject);
        }

        private void _selectRandomArrow()
        {
            int next = Random.Range(0, 3);
            if (next >= (int)_currentCorrectArrow)
                next++;

            _currentCorrectArrow = (ArrowDirection)next;

            var arrowTransform = _currentArrowImage.rectTransform;
            _currentArrowImage.enabled = true;
            arrowTransform.rotation = _getArrowRotation(_currentCorrectArrow);
            arrowTransform.DOKill(true);
            arrowTransform.DOPunchScale(Vector3.one * 0.2f, 0.15f, 1, 0f);
        }

        private Quaternion _getArrowRotation(ArrowDirection direction)
        {
            switch (direction)
            {
                case ArrowDirection.Up:
                    return Quaternion.Euler(0f, 0f, 90f);
                case ArrowDirection.Down:
                    return Quaternion.Euler(0f, 0f, -90f);
                case ArrowDirection.Left:
                    return Quaternion.Euler(0f, 0f, 180f);
                case ArrowDirection.Right:
                    return Quaternion.Euler(0f, 0f, 0f);
                default:
                    return Quaternion.identity;
            }
        }

        private void _onArrowClicked(ArrowDirection direction)
        {
            if (!IsRunning || _hasFallen)
                return;

            if (direction == _currentCorrectArrow)
            {
                _correctArrowCount++;
                _pendingPush += _pushPerCorrect;
                _selectRandomArrow();
            }
            else
            {
                _wrongArrowCount++;
                _pendingPush -= _knockbackPerWrong;

                var arrowTransform = _currentArrowImage.rectTransform;
                arrowTransform.DOKill(true);
                arrowTransform.DOShakeAnchorPos(0.2f, 15f, 20);
            }
        }

        protected override int CalculateScore()
        {
            int score = (_correctArrowCount * REWARD_PER_CORRECT_ARROW) - (_wrongArrowCount * PENALTY_PER_WRONG_ARROW);
            return Mathf.Max(_minScore, score);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _upButton.onClick.RemoveAllListeners();
            _downButton.onClick.RemoveAllListeners();
            _leftButton.onClick.RemoveAllListeners();
            _rightButton.onClick.RemoveAllListeners();

            if (_bandMaterial != null)
                Destroy(_bandMaterial);
        }
    }
}
