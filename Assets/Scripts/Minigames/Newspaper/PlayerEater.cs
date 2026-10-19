using System;
using DG.Tweening;
using UnityEngine;

namespace Minigames.Newspaper
{
    [RequireComponent(typeof(Animator))]
    public class PlayerEater : MonoBehaviour
    {
        [SerializeField] private Transform _platePoint;
        [SerializeField] private Transform _mouthPoint;
        [SerializeField] private float _reachDuration = 0.12f;
        [SerializeField] private float _toMouthDuration = 0.15f;
        [SerializeField] private float _returnDuration = 0.12f;

        private static readonly int DeathHash = Animator.StringToHash("Death");

        private Animator _animator;
        private Sequence _biteSequence;
        private Vector3 _handPosition;
        private float _handWeight;
        private bool _isDead;

        public bool IsEating { get; private set; }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public bool TryBite(Action onBite)
        {
            if (IsEating || _isDead)
                return false;

            IsEating = true;
            _handPosition = _platePoint.position;

            _biteSequence = DOTween.Sequence()
                .Append(DOTween.To(() => _handWeight, w => _handWeight = w, 1f, _reachDuration).SetEase(Ease.OutQuad))
                .Append(DOTween.To(() => _handPosition, p => _handPosition = p, _mouthPoint.position, _toMouthDuration).SetEase(Ease.OutQuad))
                .AppendCallback(() => onBite?.Invoke())
                .Append(DOTween.To(() => _handWeight, w => _handWeight = w, 0f, _returnDuration).SetEase(Ease.InQuad))
                .OnComplete(() => IsEating = false)
                .SetLink(gameObject);

            return true;
        }

        public void PlayDeath()
        {
            _isDead = true;
            IsEating = false;
            _biteSequence?.Kill();
            _handWeight = 0f;
            _animator.SetTrigger(DeathHash);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _handWeight);
            if (_handWeight > 0f)
                _animator.SetIKPosition(AvatarIKGoal.RightHand, _handPosition);
        }
    }
}
