using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Input;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// Base implementation for container elements.
    /// </summary>
    public abstract class UIContainer : UIElement, IUIContainer
    {
        private readonly List<IUIElement> _children = new List<IUIElement>();
        
        /// <summary>
        /// Gets the children of this container.
        /// </summary>
        public IReadOnlyList<IUIElement> Children => _children.AsReadOnly();
        
        /// <summary>
        /// Adds a child element to this container.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public virtual void AddChild(IUIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
                
            if (element.Parent != null)
                element.Parent.RemoveChild(element);
                
            _children.Add(element);
            element.Parent = this;
        }
        
        /// <summary>
        /// Removes a child element from this container.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public virtual void RemoveChild(IUIElement element)
        {
            if (element != null && _children.Contains(element))
            {
                _children.Remove(element);
                element.Parent = null;
            }
        }
        
        /// <summary>
        /// Clears all children from this container.
        /// </summary>
        public virtual void ClearChildren()
        {
            foreach (var child in _children.ToList())
            {
                RemoveChild(child);
            }
        }
        
        /// <summary>
        /// Updates this container and its children.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // Update children
            foreach (var child in _children.ToList())
            {
                if (child.IsActive)
                {
                    child.Update(gameTime);
                }
            }
        }
        
        /// <summary>
        /// Draws this container and its children.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw children
            foreach (var child in _children)
            {
                if (child.IsVisible)
                {
                    child.Draw(spriteBatch);
                }
            }
        }
        
        /// <summary>
        /// Handles input for this container and its children.
        /// </summary>
        /// <param name="inputManager">The input manager.</param>
        /// <returns>True if input was handled, otherwise false.</returns>
        public override bool HandleInput(InputManager inputManager)
        {
            // Process children in reverse order (top-most first)
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                var child = _children[i];
                
                if (child.IsActive && child.IsVisible)
                {
                    if (child.HandleInput(inputManager))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}