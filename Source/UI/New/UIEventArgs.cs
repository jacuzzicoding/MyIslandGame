using System;
using Microsoft.Xna.Framework;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// Event arguments for UI events.
    /// </summary>
    public class UIEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the source element that generated the event.
        /// </summary>
        public IUIElement Source { get; }
        
        /// <summary>
        /// Gets the position of the mouse when the event occurred.
        /// </summary>
        public Point MousePosition { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIEventArgs"/> class.
        /// </summary>
        /// <param name="source">The source element.</param>
        /// <param name="mousePosition">The mouse position.</param>
        public UIEventArgs(IUIElement source, Point mousePosition)
        {
            Source = source;
            MousePosition = mousePosition;
        }
    }
}