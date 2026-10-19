using UI.Minigame;
using UnityEngine;

namespace StateMachine.States
{
    public class BootstrapState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly FadeController _fadeController;

        public BootstrapState(GameStateMachine gameStateMachine, FadeController fadeController)
        {
            _gameStateMachine = gameStateMachine;
            _fadeController = fadeController;
        }

        public void Enter()
        {
            Application.targetFrameRate = 60;
            _fadeController.SetBlack();

            _gameStateMachine.Enter<MainMenuState>();
        }

        public void Exit()
        {
        }
    }
}
