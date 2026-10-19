using DG.Tweening;
using UnityEngine;

namespace Minigames.FuckTech
{
    public class PhysicalButton : MonoBehaviour
    {
        [SerializeField] private Transform _cap;
        [SerializeField] private Transform _pressPoint;
        [SerializeField] private float _pressDepth = 0.04f;
        [SerializeField] private float _pressDuration = 0.06f;
        [SerializeField] private float _releaseDuration = 0.3f;

        private Vector3 _capRestPosition;

        public Transform PressPoint => _pressPoint != null ? _pressPoint : _cap;

        private void Awake()
        {
            _capRestPosition = _cap.localPosition;
        }

        public void PlayPress()
        {
            Vector3 pressDirection = _cap.parent != null
                ? _cap.parent.InverseTransformDirection(-_cap.up)
                : -_cap.up;
            Vector3 pressedPosition = _capRestPosition + pressDirection * _pressDepth;

            _cap.DOKill();
            _cap.localPosition = _capRestPosition;

            DOTween.Sequence()
                .Append(_cap.DOLocalMove(pressedPosition, _pressDuration).SetEase(Ease.OutQuad))
                .Append(_cap.DOLocalMove(_capRestPosition, _releaseDuration).SetEase(Ease.OutBack))
                .SetTarget(_cap)
                .SetLink(gameObject);
        }
    }
}
