using Core;
using UI.Minigame;

namespace StateMachine.States
{
    public class FinishState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly UIManager _uiManager;
        private readonly FadeController _fadeController;
        private readonly GameFlowManager _gameFlowManager;

        public FinishState(GameStateMachine gameStateMachine,
            UIManager uiManager,
            FadeController fadeController,
            GameFlowManager gameFlowManager)
        {
            _gameStateMachine = gameStateMachine;
            _uiManager = uiManager;
            _fadeController = fadeController;
            _gameFlowManager = gameFlowManager;
        }

        public void Enter()
        {
            _uiManager.ShowFinish(_gameFlowManager.TotalScore);
            _uiManager.OnMainMenuRequested += HandleMainMenuRequested;
            _fadeController.FadeIn();
        }

        public void Exit()
        {
            _uiManager.OnMainMenuRequested -= HandleMainMenuRequested;
        }

        private void HandleMainMenuRequested()
        {
            _fadeController.FadeOut(() => _gameStateMachine.Enter<MainMenuState>());
        }
    }
}
