using UnityEngine;

namespace Minigames.Common
{
    public class CameraFollowZ : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothTime = 0.15f;

        [Header("Limits")]
        [SerializeField] private bool _useLimits;
        [SerializeField] private float _minZ;
        [SerializeField] private float _maxZ = 50f;

        private float _offsetZ;
        private float _velocityZ;

        private void Start()
        {
            _offsetZ = transform.position.z - _target.position.z;
        }

        private void LateUpdate()
        {
            float targetZ = _target.position.z + _offsetZ;
            if (_useLimits)
                targetZ = Mathf.Clamp(targetZ, _minZ, _maxZ);

            Vector3 position = transform.position;
            position.z = Mathf.SmoothDamp(position.z, targetZ, ref _velocityZ, _smoothTime);
            transform.position = position;
        }
    }
}
