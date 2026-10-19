using UI.Minigame;

namespace StateMachine.States
{
    public class MainMenuState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly UIManager _uiManager;
        private readonly FadeController _fadeController;

        public MainMenuState(GameStateMachine gameStateMachine, UIManager uiManager, FadeController fadeController)
        {
            _gameStateMachine = gameStateMachine;
            _uiManager = uiManager;
            _fadeController = fadeController;
        }

        public void Enter()
        {
            _uiManager.ShowMainMenu();
            _uiManager.OnPlayRequested += HandlePlayRequested;
            _fadeController.FadeIn();
        }

        public void Exit()
        {
            _uiManager.OnPlayRequested -= HandlePlayRequested;
        }

        private void HandlePlayRequested()
        {
            _gameStateMachine.Enter<GameplayState>();
        }
    }
}
