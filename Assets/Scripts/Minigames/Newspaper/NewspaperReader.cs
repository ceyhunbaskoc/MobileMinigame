using DG.Tweening;
using UnityEngine;

namespace Minigames.Newspaper
{
    [RequireComponent(typeof(Animator))]
    public class NewspaperReader : MonoBehaviour
    {
        [Header("Newspaper")]
        [SerializeField] private Transform _newspaper;
        [SerializeField] private Transform _newspaperUpPose;
        [SerializeField] private Transform _newspaperDownPose;
        [SerializeField] private Transform _leftHandle;
        [SerializeField] private Transform _rightHandle;
        [SerializeField, Range(0f, 1f)] private float _handRotationWeight = 1f;

        [Header("Look")]
        [SerializeField] private Transform _lookTarget;
        [SerializeField, Range(0f, 1f)] private float _bodyLookWeight = 0.3f;

        [Header("Gun")]
        [SerializeField] private GameObject _gun;
        [SerializeField] private Transform _aimPose;
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private float _aimDuration = 0.35f;

        private Animator _animator;
        private float _lowerAmount;
        private float _aimWeight;
        private Tween _lowerTween;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _gun.SetActive(false);
            _applyNewspaperPose();
        }

        public Tween TweenLowerAmount(float target, float duration)
        {
            _lowerTween?.Kill();
            _lowerTween = _createLowerTween(target, duration).SetLink(gameObject);
            return _lowerTween;
        }

        public Sequence PlayShoot()
        {
            _lowerTween?.Kill();
            _gun.SetActive(true);

            return DOTween.Sequence()
                .Append(_createLowerTween(1f, 0.15f))
                .Join(DOTween.To(() => _aimWeight, w => _aimWeight = w, 1f, _aimDuration).SetEase(Ease.OutQuad))
                .AppendCallback(() =>
                {
                    if (_muzzleFlash != null)
                        _muzzleFlash.Play();
                })
                .SetLink(gameObject);
        }

        private Tween _createLowerTween(float target, float duration)
        {
            return DOTween.To(() => _lowerAmount, value =>
                {
                    _lowerAmount = value;
                    _applyNewspaperPose();
                }, target, duration)
                .SetEase(Ease.InOutSine);
        }

        private void _applyNewspaperPose()
        {
            _newspaper.SetPositionAndRotation(
                Vector3.Lerp(_newspaperUpPose.position, _newspaperDownPose.position, _lowerAmount),
                Quaternion.Slerp(_newspaperUpPose.rotation, _newspaperDownPose.rotation, _lowerAmount));
        }

        private void OnAnimatorIK(int layerIndex)
        {
            _setHandIK(AvatarIKGoal.LeftHand, _leftHandle.position, _leftHandle.rotation);

            Vector3 rightPosition = Vector3.Lerp(_rightHandle.position, _aimPose.position, _aimWeight);
            Quaternion rightRotation = Quaternion.Slerp(_rightHandle.rotation, _aimPose.rotation, _aimWeight);
            _setHandIK(AvatarIKGoal.RightHand, rightPosition, rightRotation);

            float lookWeight = Mathf.Max(_lowerAmount, _aimWeight);
            _animator.SetLookAtWeight(lookWeight, _bodyLookWeight, 1f, 0f, 0.5f);
            _animator.SetLookAtPosition(_lookTarget.position);
        }

        private void _setHandIK(AvatarIKGoal goal, Vector3 position, Quaternion rotation)
        {
            _animator.SetIKPositionWeight(goal, 1f);
            _animator.SetIKRotationWeight(goal, _handRotationWeight);
            _animator.SetIKPosition(goal, position);
            _animator.SetIKRotation(goal, rotation);
        }
    }
}
