using System.Collections;
using Core;
using UnityEngine;

namespace StateMachine.States
{
    public class GameplayState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly GameFlowManager _gameFlowManager;
        private readonly MonoBehaviour _coroutineRunner;

        private Coroutine _sessionRoutine;

        public GameplayState(GameStateMachine gameStateMachine, GameFlowManager gameFlowManager, MonoBehaviour coroutineRunner)
        {
            _gameStateMachine = gameStateMachine;
            _gameFlowManager = gameFlowManager;
            _coroutineRunner = coroutineRunner;
        }

        public void Enter()
        {
            _sessionRoutine = _coroutineRunner.StartCoroutine(RunSession());
        }

        public void Exit()
        {
            if (_sessionRoutine != null)
            {
                _coroutineRunner.StopCoroutine(_sessionRoutine);
                _sessionRoutine = null;
            }
        }

        private IEnumerator RunSession()
        {
            yield return _gameFlowManager.RunSession();

            _sessionRoutine = null;
            _gameStateMachine.Enter<FinishState>();
        }
    }
}
