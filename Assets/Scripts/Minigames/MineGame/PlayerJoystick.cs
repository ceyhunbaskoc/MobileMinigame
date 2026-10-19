using System;
using UnityEngine;

namespace Minigames.MineGame
{
    public class PlayerJoystick : MonoBehaviour
    {
        [SerializeField] private float _speed = 4f;
        [SerializeField] private float _turnSpeed = 12f;
        [SerializeField] private float _damagedSpeedMultiplier = 0.5f;
        [SerializeField] private VariableJoystick _variableJoystick;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private float _walkingDampTime = 0.1f;

        private static readonly int DeathHash = Animator.StringToHash("Death");
        private static readonly int DamagedHash = Animator.StringToHash("Damaged");
        private static readonly int FinishedHash = Animator.StringToHash("Finish");
        private static readonly int WalkingThresholdHash = Animator.StringToHash("WalkingThreshold");

        private const float INPUT_DEADZONE = 0.1f;

        private bool _canMove;
        private float _speedMultiplier = 1f;

        public event Action<Mine> OnTriggeredMine;
        public event Action OnReachedFinish;

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (canMove)
                return;

            _stop();
            _playerAnimator.SetFloat(WalkingThresholdHash, 0f);
        }

        private void Update()
        {
            if (!_canMove)
                return;

            Vector3 velocity = _rb.linearVelocity;
            velocity.y = 0f;

            float maxSpeed = _speed * _speedMultiplier;
            float normalizedSpeed = maxSpeed > 0f ? Mathf.Clamp01(velocity.magnitude / maxSpeed) : 0f;

            _playerAnimator.SetFloat(WalkingThresholdHash, normalizedSpeed, _walkingDampTime, Time.deltaTime);
        }

        public void Teleport(Vector3 position)
        {
            _rb.position = position;
            transform.position = position;
            _stop();
        }

        private void FixedUpdate()
        {
            if (!_canMove)
                return;

            Vector3 direction = Vector3.forward * _variableJoystick.Vertical + Vector3.right * _variableJoystick.Horizontal;
            direction = Vector3.ClampMagnitude(direction, 1f);

            Vector3 velocity = direction * (_speed * _speedMultiplier);
            velocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = velocity;

            if (direction.sqrMagnitude > INPUT_DEADZONE * INPUT_DEADZONE)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _visualRoot.rotation = Quaternion.Slerp(_visualRoot.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime);
            }
        }

        private void _stop()
        {
            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
        }

        public void PlayDeathAnimation()
        {
            SetCanMove(false);
            _playerAnimator.SetTrigger(DeathHash);
        }

        public void PlayDamagedAnimation()
        {
            _speedMultiplier = _damagedSpeedMultiplier;
            _playerAnimator.SetBool(DamagedHash, true);
        }

        public void PlayFinishedAnimation()
        {
            SetCanMove(false);
            _playerAnimator.SetTrigger(FinishedHash);
        }

        private void OnTriggerEnter(Collider other)
        {
            var mine = other.GetComponentInParent<Mine>();
            if (mine != null)
            {
                OnTriggeredMine?.Invoke(mine);
                return;
            }

            if (other.CompareTag("Finish"))
                OnReachedFinish?.Invoke();
        }
    }
}
