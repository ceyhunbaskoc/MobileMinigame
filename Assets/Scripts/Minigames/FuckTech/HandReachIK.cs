using System;
using DG.Tweening;
using UnityEngine;

namespace Minigames.FuckTech
{
    [RequireComponent(typeof(Animator))]
    public class HandReachIK : MonoBehaviour
    {
        [SerializeField] private float _reachDuration = 0.18f;
        [SerializeField] private float _holdDuration = 0.08f;
        [SerializeField] private float _returnDuration = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _lookAtWeight = 0.6f;

        private Animator _animator;

        private readonly Transform[] _targets = new Transform[2];
        private readonly float[] _weights = new float[2];
        private readonly Sequence[] _sequences = new Sequence[2];

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Press(Transform target, AvatarIKGoal hand, Action onPressed = null)
        {
            int index = _getHandIndex(hand);
            _targets[index] = target;

            _sequences[index]?.Kill();
            _sequences[index] = DOTween.Sequence()
                .Append(_tweenWeight(index, 1f, _reachDuration).SetEase(Ease.OutQuad))
                .AppendCallback(() => onPressed?.Invoke())
                .AppendInterval(_holdDuration)
                .Append(_tweenWeight(index, 0f, _returnDuration).SetEase(Ease.InOutQuad))
                .SetLink(gameObject);
        }

        private Tween _tweenWeight(int index, float target, float duration)
        {
            return DOTween.To(() => _weights[index], w => _weights[index] = w, target, duration);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            float lookWeight = 0f;
            Vector3 lookPosition = Vector3.zero;

            for (int i = 0; i < 2; i++)
            {
                var goal = i == 0 ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;
                float weight = _targets[i] != null ? _weights[i] : 0f;

                _animator.SetIKPositionWeight(goal, weight);
                if (weight <= 0f)
                    continue;

                _animator.SetIKPosition(goal, _targets[i].position);

                if (weight > lookWeight)
                {
                    lookWeight = weight;
                    lookPosition = _targets[i].position;
                }
            }

            _animator.SetLookAtWeight(lookWeight * _lookAtWeight);
            if (lookWeight > 0f)
                _animator.SetLookAtPosition(lookPosition);
        }

        private static int _getHandIndex(AvatarIKGoal hand)
        {
            return hand == AvatarIKGoal.LeftHand ? 0 : 1;
        }
    }
}
