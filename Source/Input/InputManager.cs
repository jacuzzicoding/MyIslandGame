using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MyIslandGame.Input
{
    /// <summary>
    /// Manages input from keyboard, mouse, and gamepad.
    /// </summary>
    public class InputManager
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        
        private GamePadState _currentGamePadState;
        private GamePadState _previousGamePadState;
        
        private readonly Dictionary<string, InputAction> _actions = new();
        
        /// <summary>
        /// Gets the current mouse position.
        /// </summary>
        public Point MousePosition => _currentMouseState.Position;
        
        /// <summary>
        /// Gets the mouse movement since the last update.
        /// </summary>
        public Point MouseMovement => new Point(
            _currentMouseState.Position.X - _previousMouseState.Position.X,
            _currentMouseState.Position.Y - _previousMouseState.Position.Y);
        
        /// <summary>
        /// Updates the input states.
        /// </summary>
        public void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _previousMouseState = _currentMouseState;
            _previousGamePadState = _currentGamePadState;
            
            _currentKeyboardState = Keyboard.GetState();
            _currentMouseState = Mouse.GetState();
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
            
            // Update all registered actions
            foreach (var action in _actions.Values)
            {
                action.Update(this);
            }
        }
        
        /// <summary>
        /// Registers an input action.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="action">The action configuration.</param>
        public void RegisterAction(string actionName, InputAction action)
        {
            _actions[actionName] = action;
        }
        
        /// <summary>
        /// Gets whether an action is active (pressed or held).
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <returns>True if the action is active, otherwise false.</returns>
        public bool IsActionActive(string actionName)
        {
            return _actions.TryGetValue(actionName, out var action) && action.IsActive;
        }
        
        /// <summary>
        /// Gets whether an action was just pressed this frame.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <returns>True if the action was just pressed, otherwise false.</returns>
        public bool WasActionPressed(string actionName)
        {
            return _actions.TryGetValue(actionName, out var action) && action.WasPressed;
        }
        
        /// <summary>
        /// Gets whether an action was just released this frame.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <returns>True if the action was just released, otherwise false.</returns>
        public bool WasActionReleased(string actionName)
        {
            return _actions.TryGetValue(actionName, out var action) && action.WasReleased;
        }
        
        /// <summary>
        /// Gets the value of an action's axis (typically between -1 and 1).
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <returns>The axis value.</returns>
        public float GetActionAxis(string actionName)
        {
            return _actions.TryGetValue(actionName, out var action) ? action.AxisValue : 0f;
        }
        
        /// <summary>
        /// Gets whether a key is currently pressed.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is pressed, otherwise false.</returns>
        public bool IsKeyDown(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key);
        }
        
        /// <summary>
        /// Gets whether a key was just pressed this frame.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key was just pressed, otherwise false.</returns>
        public bool WasKeyPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        }
        
        /// <summary>
        /// Gets whether a key was just released this frame.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key was just released, otherwise false.</returns>
        public bool WasKeyReleased(Keys key)
        {
            return _currentKeyboardState.IsKeyUp(key) && _previousKeyboardState.IsKeyDown(key);
        }
        
        /// <summary>
        /// Gets whether a mouse button is currently pressed.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button is pressed, otherwise false.</returns>
        public bool IsMouseButtonDown(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _currentMouseState.LeftButton == ButtonState.Pressed,
                MouseButton.Right => _currentMouseState.RightButton == ButtonState.Pressed,
                MouseButton.Middle => _currentMouseState.MiddleButton == ButtonState.Pressed,
                _ => false
            };
        }
        
        /// <summary>
        /// Gets whether a mouse button was just pressed this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button was just pressed, otherwise false.</returns>
        public bool WasMouseButtonPressed(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released,
                MouseButton.Right => _currentMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released,
                MouseButton.Middle => _currentMouseState.MiddleButton == ButtonState.Pressed && _previousMouseState.MiddleButton == ButtonState.Released,
                _ => false
            };
        }
        
        /// <summary>
        /// Gets whether a mouse button was just released this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button was just released, otherwise false.</returns>
        public bool WasMouseButtonReleased(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _currentMouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Pressed,
                MouseButton.Right => _currentMouseState.RightButton == ButtonState.Released && _previousMouseState.RightButton == ButtonState.Pressed,
                MouseButton.Middle => _currentMouseState.MiddleButton == ButtonState.Released && _previousMouseState.MiddleButton == ButtonState.Pressed,
                _ => false
            };
        }
    }
    
    /// <summary>
    /// Enum for mouse buttons.
    /// </summary>
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }
}
