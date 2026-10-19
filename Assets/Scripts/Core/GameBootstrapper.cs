using Core.Camera;
using Core.Minigame;
using Core.SceneManagement;
using Data;
using StateMachine;
using StateMachine.States;
using UI.Minigame;
using UnityEngine;

namespace Core
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private FadeController _fadeController;

        [Header("Camera")]
        [SerializeField] private CameraStateOrchestrator _cameraOrchestrator;

        [Header("Scriptable Objects")]
        [SerializeField] private MinigamesDefinitionsSO _minigamesDefinitions;

        private GameStateMachine _stateMachine;
        private GameFlowManager _gameFlowManager;

        private void Awake()
        {
            _stateMachine = new GameStateMachine();

            var sceneController = new SceneController();
            var sessionRandomize = new MinigameSessionRandomize(_minigamesDefinitions);

            _gameFlowManager = new GameFlowManager(_fadeController, 
                _uiManager, 
                sceneController, 
                sessionRandomize);

            if (_cameraOrchestrator != null)
                _cameraOrchestrator.Initialize(_stateMachine);

            _stateMachine.Register(new BootstrapState(_stateMachine, _fadeController));
            _stateMachine.Register(new MainMenuState(_stateMachine, _uiManager, _fadeController));
            _stateMachine.Register(new GameplayState(_stateMachine, _gameFlowManager, this));
            _stateMachine.Register(new FinishState(_stateMachine, _uiManager, _fadeController, _gameFlowManager));
        }

        private void Start()
        {
            _stateMachine.Enter<BootstrapState>();
        }

        private void OnDestroy()
        {
            _gameFlowManager?.Dispose();
        }
    }
}
