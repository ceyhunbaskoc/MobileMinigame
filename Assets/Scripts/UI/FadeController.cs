using System;
using DG.Tweening;
using UnityEngine;

namespace UI.Minigame
{
    public class FadeController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _blackCanvasGroup;
        [SerializeField] private float _fadeDuration = 0.5f;

        public void SetBlack()
        {
            _blackCanvasGroup.DOKill();
            _blackCanvasGroup.alpha = 1f;
            _blackCanvasGroup.blocksRaycasts = true;
        }

        public Tween FadeIn(Action onComplete = null) => FadeTo(0f, onComplete);

        public Tween FadeOut(Action onComplete = null) => FadeTo(1f, onComplete);

        private Tween FadeTo(float target, Action onComplete)
        {
            _blackCanvasGroup.DOKill();
            _blackCanvasGroup.blocksRaycasts = true;

            return _blackCanvasGroup.DOFade(target, _fadeDuration)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _blackCanvasGroup.blocksRaycasts = target > 0.5f;
                    onComplete?.Invoke();
                });
        }
    }
}
