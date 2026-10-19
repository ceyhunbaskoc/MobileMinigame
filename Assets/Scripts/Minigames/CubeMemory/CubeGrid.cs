using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minigames.CubeMemory
{
    public class CubeGrid : MonoBehaviour
    {
        public const int SIZE = 3;

        [SerializeField] private CubeCell _cellPrefab;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _gap = 0.05f;

        private readonly Dictionary<Vector3Int, CubeCell> _cells = new Dictionary<Vector3Int, CubeCell>();
        private readonly List<Vector3Int> _visibleCoords = new List<Vector3Int>();

        public event Action<CubeCell> OnCellClicked;

        public IReadOnlyList<Vector3Int> VisibleCoords => _visibleCoords;
        public bool IsInteractable { get; set; }

        private void Awake()
        {
            _build();
        }

        private void _build()
        {
            float step = _cellSize + _gap;
            float offset = (SIZE - 1) * 0.5f * step;

            for (int x = 0; x < SIZE; x++)
            for (int y = 0; y < SIZE; y++)
            for (int z = 0; z < SIZE; z++)
            {
                var coord = new Vector3Int(x, y, z);
                var cell = Instantiate(_cellPrefab, transform);
                cell.transform.localPosition = new Vector3(x * step - offset, y * step - offset, z * step - offset);
                cell.transform.localRotation = Quaternion.identity;
                cell.transform.localScale = Vector3.one * _cellSize;
                _setLayerRecursively(cell.gameObject, gameObject.layer);

                cell.Initialize(coord, this);
                _cells[coord] = cell;

                if (_isOnVisibleFace(coord))
                    _visibleCoords.Add(coord);
            }
        }

        public void HandleCellClicked(CubeCell cell)
        {
            if (!IsInteractable || cell.IsHollow)
                return;

            OnCellClicked?.Invoke(cell);
        }

        public void ApplyPattern(ICollection<Vector3Int> hollowCoords, bool animate)
        {
            foreach (var pair in _cells)
                pair.Value.SetHollow(hollowCoords.Contains(pair.Key), animate);
        }

        public void ResetCells(bool animate)
        {
            foreach (var cell in _cells.Values)
                cell.SetHollow(false, animate);
        }

        public HashSet<Vector3Int> GetHollowCoords()
        {
            var result = new HashSet<Vector3Int>();
            foreach (var pair in _cells)
            {
                if (pair.Value.IsHollow)
                    result.Add(pair.Key);
            }
            return result;
        }

        private static bool _isOnVisibleFace(Vector3Int coord)
        {
            return coord.x == SIZE - 1 || coord.y == SIZE - 1 || coord.z == 0;
        }

        private static void _setLayerRecursively(GameObject target, int layer)
        {
            target.layer = layer;
            foreach (Transform child in target.transform)
                _setLayerRecursively(child.gameObject, layer);
        }
    }
}
