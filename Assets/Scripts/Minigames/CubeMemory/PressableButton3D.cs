using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Minigames.CubeMemory
{
    public class PressableButton3D : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Transform _cap;
        [SerializeField] private Vector3 _pressDirection = Vector3.down;
        [SerializeField] private float _pressDepth = 0.04f;
        [SerializeField] private float _pressDuration = 0.06f;
        [SerializeField] private float _releaseDuration = 0.3f;

        private Vector3 _capRestPosition;

        public event Action OnClicked;

        public bool Interactable { get; set; }

        private void Awake()
        {
            _capRestPosition = _cap.localPosition;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Interactable)
                return;

            _playPress();
            OnClicked?.Invoke();
        }

        private void _playPress()
        {
            Vector3 worldOffset = _pressDirection.normalized * _pressDepth;
            Vector3 localOffset = _cap.parent != null
                ? _cap.parent.InverseTransformVector(worldOffset)
                : worldOffset;
            Vector3 pressedPosition = _capRestPosition + localOffset;

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
