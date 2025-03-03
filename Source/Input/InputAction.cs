using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MyIslandGame.Input
{
    /// <summary>
    /// Represents an input action that can be mapped to various input devices and buttons.
    /// </summary>
    public class InputAction
    {
        private readonly List<Keys> _keys = new List<Keys>();
        private readonly List<MouseButton> _mouseButtons = new List<MouseButton>();
        private readonly List<Buttons> _gamepadButtons = new List<Buttons>();
        
        private GamePadTriggers _trigger = GamePadTriggers.None;
        private GamePadThumbSticks _thumbStick = GamePadThumbSticks.None;
        
        private bool _wasActive;
        
        /// <summary>
        /// Gets a value indicating whether this action is currently active.
        /// </summary>
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether this action was just pressed this frame.
        /// </summary>
        public bool WasPressed { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether this action was just released this frame.
        /// </summary>
        public bool WasReleased { get; private set; }
        
        /// <summary>
        /// Gets the analog value of this action (typically between -1 and 1).
        /// </summary>
        public float AxisValue { get; private set; }
        
        /// <summary>
        /// Gets or sets the sensitivity of this action.
        /// </summary>
        public float Sensitivity { get; set; } = 0.1f;
        
        /// <summary>
        /// Gets or sets the dead zone for analog inputs.
        /// </summary>
        public float DeadZone { get; set; } = 0.15f;
        
        /// <summary>
        /// Maps a keyboard key to this action.
        /// </summary>
        /// <param name="key">The key to map.</param>
        /// <returns>This InputAction instance for chaining.</returns>
        public InputAction MapKey(Keys key)
        {
            _keys.Add(key);
            return this;
        }
        
        /// <summary>
        /// Maps a mouse button to this action.
        /// </summary>
        /// <param name="button">The mouse button to map.</param>
        /// <returns>This InputAction instance for chaining.</returns>
        public InputAction MapMouseButton(MouseButton button)
        {
            _mouseButtons.Add(button);
            return this;
        }
        
        /// <summary>
        /// Maps a gamepad button to this action.
        /// </summary>
        /// <param name="button">The gamepad button to map.</param>
        /// <returns>This InputAction instance for chaining.</returns>
        public InputAction MapGamepadButton(Buttons button)
        {
            _gamepadButtons.Add(button);
            return this;
        }
        
        /// <summary>
        /// Maps a gamepad trigger to this action.
        /// </summary>
        /// <param name="trigger">The trigger to map.</param>
        /// <returns>This action for method chaining.</returns>
        public InputAction MapTrigger(GamePadTriggers trigger)
        {
            _trigger = trigger;
            return this;
        }
        
        /// <summary>
        /// Maps a gamepad thumbstick to this action.
        /// </summary>
        /// <param name="thumbStick">The thumbstick to map.</param>
        /// <returns>This action for method chaining.</returns>
        public InputAction MapThumbStick(GamePadThumbSticks thumbStick)
        {
            _thumbStick = thumbStick;
            return this;
        }
        
        /// <summary>
        /// Gets the list of mapped keyboard keys.
        /// </summary>
        public IReadOnlyList<Keys> Keys => _keys.AsReadOnly();
        
        /// <summary>
        /// Gets the list of mapped mouse buttons.
        /// </summary>
        public IReadOnlyList<MouseButton> MouseButtons => _mouseButtons.AsReadOnly();
        
        /// <summary>
        /// Gets the list of mapped gamepad buttons.
        /// </summary>
        public IReadOnlyList<Buttons> GamepadButtons => _gamepadButtons.AsReadOnly();
        
        /// <summary>
        /// Updates the state of this action.
        /// </summary>
        /// <param name="inputManager">The input manager.</param>
        public void Update(InputManager inputManager)
        {
            _wasActive = IsActive;
            IsActive = false;
            AxisValue = 0f;
            
            // Check keyboard keys
            foreach (var key in _keys)
            {
                if (inputManager.IsKeyDown(key))
                {
                    IsActive = true;
                    AxisValue = 1f;
                    break;
                }
            }
            
            // Check mouse buttons
            if (!IsActive)
            {
                foreach (var button in _mouseButtons)
                {
                    if (inputManager.IsMouseButtonDown(button))
                    {
                        IsActive = true;
                        AxisValue = 1f;
                        break;
                    }
                }
            }
            
            // Check gamepad buttons and analog inputs
            // Note: In a real implementation, you would check gamepad buttons and analog inputs
            // We're simplifying here for brevity
            
            // Set pressed and released states
            WasPressed = IsActive && !_wasActive;
            WasReleased = !IsActive && _wasActive;
        }
    }
    
    /// <summary>
    /// Enum for gamepad buttons.
    /// </summary>
    public enum GamePadButtons
    {
        A, B, X, Y,
        LeftShoulder, RightShoulder,
        LeftStick, RightStick,
        DPadUp, DPadDown, DPadLeft, DPadRight,
        Start, Back
    }
    
    /// <summary>
    /// Enum for gamepad triggers.
    /// </summary>
    public enum GamePadTriggers
    {
        None,
        Left,
        Right
    }
    
    /// <summary>
    /// Enum for gamepad thumbsticks.
    /// </summary>
    public enum GamePadThumbSticks
    {
        None,
        LeftX, LeftY,
        RightX, RightY
    }
}
