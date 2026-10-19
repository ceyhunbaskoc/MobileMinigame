using System.Collections.Generic;
using UnityEngine;

namespace Minigames.MineGame
{
    public class MineFieldSpawner : MonoBehaviour
    {
        [SerializeField] private Mine _minePrefab;
        [SerializeField] private Vector2 _areaSize = new Vector2(10f, 30f);

        [Header("Density")]
        [SerializeField, Tooltip("Mines per 10 square units")] private float _density = 1f;
        [SerializeField] private int _maxMineCount = 60;
        [SerializeField] private float _minDistance = 1.2f;
        [SerializeField] private int _maxAttemptsPerMine = 30;

        [Header("Safe Zones")]
        [SerializeField] private Transform[] _safePoints;
        [SerializeField] private float _safeRadius = 2f;

        private readonly List<Mine> _spawnedMines = new List<Mine>();

        public IReadOnlyList<Mine> Mines => _spawnedMines;

        [ContextMenu("Spawn")]
        public void Spawn()
        {
            Clear();

            float area = _areaSize.x * _areaSize.y;
            int targetCount = Mathf.Min(_maxMineCount, Mathf.RoundToInt(area * _density / 10f));
            var positions = new List<Vector3>(targetCount);

            int maxAttempts = targetCount * _maxAttemptsPerMine;
            for (int attempt = 0; attempt < maxAttempts && positions.Count < targetCount; attempt++)
            {
                Vector3 candidate = _getRandomPoint();
                if (_isValidPoint(candidate, positions))
                    positions.Add(candidate);
            }

            if (positions.Count < targetCount)
                Debug.LogWarning($"[MineFieldSpawner] Placed {positions.Count}/{targetCount} mines. Lower density or min distance.", this);

            foreach (var position in positions)
            {
                var rotation = Quaternion.Euler(-90f, 0f, Random.Range(0f, 360f));
                _spawnedMines.Add(Instantiate(_minePrefab, position, rotation, transform));
            }
        }

        [ContextMenu("Clear")]
        public void Clear()
        {
            _spawnedMines.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.GetComponent<Mine>() == null)
                    continue;

                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        private Vector3 _getRandomPoint()
        {
            var localPoint = new Vector3(
                Random.Range(-_areaSize.x * 0.5f, _areaSize.x * 0.5f),
                0f,
                Random.Range(-_areaSize.y * 0.5f, _areaSize.y * 0.5f));

            return transform.position + transform.rotation * localPoint;
        }

        private bool _isValidPoint(Vector3 candidate, List<Vector3> placed)
        {
            float minDistanceSqr = _minDistance * _minDistance;
            foreach (var position in placed)
            {
                if (_flatDistanceSqr(candidate, position) < minDistanceSqr)
                    return false;
            }

            if (_safePoints != null)
            {
                float safeRadiusSqr = _safeRadius * _safeRadius;
                foreach (var safePoint in _safePoints)
                {
                    if (safePoint != null && _flatDistanceSqr(candidate, safePoint.position) < safeRadiusSqr)
                        return false;
                }
            }

            return true;
        }

        private static float _flatDistanceSqr(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.8f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(_areaSize.x, 0.05f, _areaSize.y));
            Gizmos.matrix = Matrix4x4.identity;

            if (_safePoints == null)
                return;

            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.8f);
            foreach (var safePoint in _safePoints)
            {
                if (safePoint != null)
                    Gizmos.DrawWireSphere(safePoint.position, _safeRadius);
            }
        }
    }
}
