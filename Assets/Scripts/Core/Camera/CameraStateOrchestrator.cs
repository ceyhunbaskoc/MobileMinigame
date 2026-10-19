using System;
using StateMachine;
using StateMachine.States;
using UnityEngine;

namespace Core.Camera
{
    public class CameraStateOrchestrator : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera _mainMenuCamera;
        private GameStateMachine _stateMachine;

        public void Initialize(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _stateMachine.OnStateEntered += HandleStateChanged;
        }

        private void HandleStateChanged(IState stateType)
        {
            switch (stateType)
            {
                case MainMenuState:
                case FinishState:
                    _mainMenuCamera.gameObject.SetActive(true);
                    break;
                case GameplayState:
                    _mainMenuCamera.gameObject.SetActive(false);
                    break;
            }
        }
    }
}