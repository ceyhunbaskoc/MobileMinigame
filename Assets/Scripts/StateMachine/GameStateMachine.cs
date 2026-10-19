using System;
using System.Collections.Generic;

namespace StateMachine
{
    public class GameStateMachine
    {
        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();
        private IState _currentState;
        private IState _previousState;
        
        public event Action<IState> OnStateEntered;

        public IState CurrentState => _currentState;
        public IState PreviousState => _previousState;
        
        public void Register<TState>(TState state) where TState : IState
        {
            _states[typeof(TState)] = state;
        }

        public void Enter<TState>() where TState : IState
        {
            if (!_states.TryGetValue(typeof(TState), out var next))
                throw new InvalidOperationException($"State not registered: {typeof(TState).Name}");

            _currentState?.Exit();
            _previousState = _currentState;
            _currentState = next;
            OnStateEntered?.Invoke(_currentState);
            _currentState.Enter();
        }
    }
}