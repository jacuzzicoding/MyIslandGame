using System.Collections.Generic;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// Interface for UI elements that can contain other elements.
    /// </summary>
    public interface IUIContainer : IUIElement
    {
        /// <summary>
        /// Gets the children of this container.
        /// </summary>
        IReadOnlyList<IUIElement> Children { get; }
        
        /// <summary>
        /// Adds a child element to this container.
        /// </summary>
        /// <param name="element">The element to add.</param>
        void AddChild(IUIElement element);
        
        /// <summary>
        /// Removes a child element from this container.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        void RemoveChild(IUIElement element);
        
        /// <summary>
        /// Clears all children from this container.
        /// </summary>
        void ClearChildren();
    }
}