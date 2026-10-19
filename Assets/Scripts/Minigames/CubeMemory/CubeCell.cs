using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Minigames.CubeMemory
{
    public class CubeCell : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private Transform _visual;
        [SerializeField] private float _animationDuration = 0.15f;

        private CubeGrid _grid;
        private Vector3 _visualScale;

        public Vector3Int Coord { get; private set; }
        public bool IsHollow { get; private set; }

        public void Initialize(Vector3Int coord, CubeGrid grid)
        {
            Coord = coord;
            _grid = grid;
            _visualScale = _visual.localScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _grid.HandleCellClicked(this);
        }

        public void SetHollow(bool isHollow, bool animate)
        {
            if (IsHollow == isHollow)
                return;

            IsHollow = isHollow;
            _collider.enabled = !isHollow;

            Vector3 targetScale = isHollow ? Vector3.zero : _visualScale;
            _visual.DOKill();

            if (!animate)
            {
                _visual.localScale = targetScale;
                return;
            }

            _visual.DOScale(targetScale, _animationDuration)
                .SetEase(isHollow ? Ease.InBack : Ease.OutBack)
                .SetLink(gameObject);
        }
    }
}
