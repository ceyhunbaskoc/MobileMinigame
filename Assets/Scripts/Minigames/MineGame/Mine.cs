using DG.Tweening;
using UnityEngine;

namespace Minigames.MineGame
{
    public class Mine : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private Collider _collider;
        [SerializeField] private ParticleSystem _explosionEffect;
        [Space]
        [SerializeField] private float _revealDuration = 0.25f;
        [SerializeField] private float _visibleDuration = 0.5f;
        [SerializeField] private float _hideDuration = 0.25f;

        private Vector3 _visualScale;

        public bool IsExploded { get; private set; }

        private void Awake()
        {
            _visualScale = _visual.localScale;
            _visual.localScale = Vector3.zero;
            _visual.gameObject.SetActive(false);
        }

        public void ShowMine()
        {
            if (IsExploded)
                return;

            _playRevealSequence();
        }

        public bool Explode()
        {
            if (IsExploded)
                return false;

            IsExploded = true;
            _collider.enabled = false;

            _playRevealSequence();

            if (_explosionEffect != null)
                _explosionEffect.Play();

            return true;
        }

        private void _playRevealSequence()
        {
            _visual.DOKill();
            _visual.gameObject.SetActive(true);

            DOTween.Sequence()
                .Append(_visual.DOScale(_visualScale, _revealDuration).SetEase(Ease.OutBack))
                .AppendInterval(_visibleDuration)
                .Append(_visual.DOScale(0f, _hideDuration).SetEase(Ease.InBack))
                .AppendCallback(() => _visual.gameObject.SetActive(false))
                .SetTarget(_visual)
                .SetLink(gameObject);
        }
    }
}
