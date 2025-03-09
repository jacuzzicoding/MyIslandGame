using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Input;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// Base implementation for interactive UI elements.
    /// </summary>
    public abstract class InteractableElement : UIElement, IInteractable
    {
        /// <summary>
        /// Gets a value indicating whether the mouse is hovering over this element.
        /// </summary>
        public bool IsHovered { get; protected set; }
        
        /// <summary>
        /// Gets a value indicating whether this element is being pressed.
        /// </summary>
        public bool IsPressed { get; protected set; }
        
        /// <summary>
        /// Occurs when the element is clicked.
        /// </summary>
        public event EventHandler<UIEventArgs> Click;
        
        /// <summary>
        /// Occurs when the mouse enters the element.
        /// </summary>
        public event EventHandler<UIEventArgs> Hover;
        
        /// <summary>
        /// Occurs when the mouse leaves the element.
        /// </summary>
        public event EventHandler<UIEventArgs> HoverExit;
        
        /// <summary>
        /// Handles input for this interactive element.
        /// </summary>
        /// <param name="inputManager">The input manager.</param>
        /// <returns>True if input was handled, otherwise false.</returns>
        public override bool HandleInput(InputManager inputManager)
        {
            if (!IsActive || !IsVisible)
                return false;
                
            // Check if mouse is over this element
            Point mousePos = inputManager.MousePosition;
            Vector2 absolutePos = GetAbsolutePosition();
            Rectangle absoluteBounds = new Rectangle(
                (int)absolutePos.X, 
                (int)absolutePos.Y, 
                Bounds.Width, 
                Bounds.Height);
                
            bool isMouseOver = absoluteBounds.Contains(mousePos);
            
            // Handle hover state
            if (isMouseOver && !IsHovered)
            {
                IsHovered = true;
                OnHover(new UIEventArgs(this, mousePos));
            }
            else if (!isMouseOver && IsHovered)
            {
                IsHovered = false;
                OnHoverExit(new UIEventArgs(this, mousePos));
            }
            
            // Handle click state
            if (isMouseOver)
            {
                if (inputManager.WasMouseButtonPressed(MouseButton.Left))
                {
                    IsPressed = true;
                }
                else if (inputManager.WasMouseButtonReleased(MouseButton.Left) && IsPressed)
                {
                    IsPressed = false;
                    OnClick(new UIEventArgs(this, mousePos));
                    return true;
                }
            }
            else if (IsPressed && inputManager.WasMouseButtonReleased(MouseButton.Left))
            {
                IsPressed = false;
            }
            
            return isMouseOver;
        }
        
        /// <summary>
        /// Called when the element is clicked.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public virtual void OnClick(UIEventArgs args)
        {
            Click?.Invoke(this, args);
        }
        
        /// <summary>
        /// Called when the mouse enters the element.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public virtual void OnHover(UIEventArgs args)
        {
            Hover?.Invoke(this, args);
        }
        
        /// <summary>
        /// Called when the mouse leaves the element.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public virtual void OnHoverExit(UIEventArgs args)
        {
            HoverExit?.Invoke(this, args);
        }
    }
}