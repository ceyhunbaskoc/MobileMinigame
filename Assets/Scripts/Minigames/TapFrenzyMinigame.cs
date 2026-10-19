using Core.Minigame;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Minigames
{
    public class TapFrenzyMinigame : MinigameBase
    {
        [Header("Rules")]
        [SerializeField] private int _targetTaps = 60;
        [SerializeField] private int _maxScore = 100;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _tapCountText;
        [SerializeField] private Transform _tapTarget;

        private int _tapCount;

        protected override void OnBegin()
        {
            _tapCount = 0;
            RefreshTapCount();
        }

        protected override void OnTick()
        {
            var pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame)
                HandleTap();
        }

        private void HandleTap()
        {
            _tapCount++;
            RefreshTapCount();

            if (_tapTarget != null)
            {
                _tapTarget.DOKill(true);
                _tapTarget.DOPunchScale(Vector3.one * 0.15f, 0.15f, 1, 0f);
            }
        }

        protected override int CalculateScore()
        {
            float ratio = Mathf.Clamp01((float)_tapCount / _targetTaps);
            return Mathf.RoundToInt(ratio * _maxScore);
        }

        protected override void OnEnd()
        {
            if (_tapTarget != null)
                _tapTarget.DOKill(true);
        }

        private void RefreshTapCount()
        {
            _tapCountText.text = _tapCount.ToString();
        }
    }
}
