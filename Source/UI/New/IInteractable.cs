using System;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// Interface for UI elements that respond to input events.
    /// </summary>
    public interface IInteractable : IUIElement
    {
        /// <summary>
        /// Occurs when the element is clicked.
        /// </summary>
        event EventHandler<UIEventArgs> Click;
        
        /// <summary>
        /// Occurs when the mouse enters the element.
        /// </summary>
        event EventHandler<UIEventArgs> Hover;
        
        /// <summary>
        /// Occurs when the mouse leaves the element.
        /// </summary>
        event EventHandler<UIEventArgs> HoverExit;
        
        /// <summary>
        /// Gets a value indicating whether the mouse is hovering over this element.
        /// </summary>
        bool IsHovered { get; }
        
        /// <summary>
        /// Gets a value indicating whether this element is being pressed.
        /// </summary>
        bool IsPressed { get; }
        
        /// <summary>
        /// Called when the element is clicked.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        void OnClick(UIEventArgs args);
        
        /// <summary>
        /// Called when the mouse enters the element.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        void OnHover(UIEventArgs args);
        
        /// <summary>
        /// Called when the mouse leaves the element.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        void OnHoverExit(UIEventArgs args);
    }
}