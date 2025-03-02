using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.States
{
    /// <summary>
    /// Manages game states and transitions between them.
    /// </summary>
    public class GameStateManager
    {
        private readonly Game _game;
        private readonly Dictionary<Type, GameState> _states = new();
        private GameState _currentState;
        private GameState _nextState;
        
        /// <summary>
        /// Gets the current active state.
        /// </summary>
        public GameState CurrentState => _currentState;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GameStateManager"/> class.
        /// </summary>
        /// <param name="game">The game instance.</param>
        public GameStateManager(Game game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
        }
        
        /// <summary>
        /// Adds a state to the manager.
        /// </summary>
        /// <typeparam name="T">The type of state to add.</typeparam>
        /// <param name="state">The state instance.</param>
        public void AddState<T>(T state) where T : GameState
        {
            var stateType = typeof(T);
            
            if (_states.ContainsKey(stateType))
            {
                _states[stateType] = state;
            }
            else
            {
                _states.Add(stateType, state);
                state.Initialize();
                state.LoadContent();
            }
        }
        
        /// <summary>
        /// Changes to a different state.
        /// </summary>
        /// <typeparam name="T">The type of state to change to.</typeparam>
        public void ChangeState<T>() where T : GameState
        {
            var stateType = typeof(T);
            
            if (!_states.TryGetValue(stateType, out var state))
            {
                throw new InvalidOperationException($"State of type {stateType.Name} not found.");
            }
            
            _nextState = state;
        }
        
        /// <summary>
        /// Updates the current state.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            // Handle state transition if needed
            if (_nextState != null && _nextState != _currentState)
            {
                _currentState?.OnExit();
                _currentState = _nextState;
                _currentState.OnEnter();
                _nextState = null;
            }
            
            _currentState?.Update(gameTime);
        }
        
        /// <summary>
        /// Draws the current state.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _currentState?.Draw(gameTime, spriteBatch);
        }
    }
}
