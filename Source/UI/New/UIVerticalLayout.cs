using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// A vertical layout container for UI elements.
    /// </summary>
    public class UIVerticalLayout : UIContainer
    {
        private int _spacing;
        private VerticalAlignment _alignment;
        
        /// <summary>
        /// Gets or sets the spacing between elements.
        /// </summary>
        public int Spacing
        {
            get => _spacing;
            set
            {
                _spacing = Math.Max(0, value);
                UpdateLayout();
            }
        }
        
        /// <summary>
        /// Gets or sets the vertical alignment of elements.
        /// </summary>
        public VerticalAlignment Alignment
        {
            get => _alignment;
            set
            {
                _alignment = value;
                UpdateLayout();
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIVerticalLayout"/> class.
        /// </summary>
        /// <param name="bounds">The bounds of the layout.</param>
        /// <param name="spacing">The spacing between elements.</param>
        /// <param name="alignment">The vertical alignment of elements.</param>
        public UIVerticalLayout(Rectangle bounds, int spacing = 0, VerticalAlignment alignment = VerticalAlignment.Top)
        {
            Bounds = bounds;
            _spacing = Math.Max(0, spacing);
            _alignment = alignment;
        }
        
        /// <summary>
        /// Initializes the layout and its children.
        /// </summary>
        public override void Initialize()
        {
            // Don't call abstract base.Initialize()
            
            // Initialize child elements
            foreach (var child in Children)
            {
                child.Initialize();
            }
        }
        
        /// <summary>
        /// Adds a child element to the layout.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public override void AddChild(IUIElement element)
        {
            base.AddChild(element);
            UpdateLayout();
        }
        
        /// <summary>
        /// Removes a child element from the layout.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public override void RemoveChild(IUIElement element)
        {
            base.RemoveChild(element);
            UpdateLayout();
        }
        
        /// <summary>
        /// Sets the size of the layout and updates the layout.
        /// </summary>
        /// <param name="size">The new size.</param>
        public override void SetSize(Vector2 size)
        {
            base.SetSize(size);
            UpdateLayout();
        }
        
        /// <summary>
        /// Updates the layout by positioning all children.
        /// </summary>
        private void UpdateLayout()
        {
            if (Children.Count == 0)
                return;
                
            // Calculate total height of all children plus spacing
            int totalHeight = 0;
            int maxWidth = 0;
            
            foreach (var child in Children)
            {
                totalHeight += child.Bounds.Height;
                maxWidth = Math.Max(maxWidth, child.Bounds.Width);
            }
            
            // Add spacing between elements
            totalHeight += _spacing * (Children.Count - 1);
            
            // Calculate starting Y position based on alignment
            int startY;
            
            switch (_alignment)
            {
                case VerticalAlignment.Top:
                    startY = Bounds.Y;
                    break;
                    
                case VerticalAlignment.Center:
                    startY = Bounds.Y + (Bounds.Height - totalHeight) / 2;
                    break;
                    
                case VerticalAlignment.Bottom:
                    startY = Bounds.Y + Bounds.Height - totalHeight;
                    break;
                    
                default:
                    startY = Bounds.Y;
                    break;
            }
            
            // Calculate horizontal position (centered within the layout bounds)
            int centerX = Bounds.X + Bounds.Width / 2;
            
            // Position each child
            int currentY = startY;
            
            foreach (var child in Children)
            {
                // Center horizontally
                int x = centerX - child.Bounds.Width / 2;
                
                // Set position
                child.SetPosition(new Vector2(x, currentY));
                
                // Move to next position
                currentY += child.Bounds.Height + _spacing;
            }
        }
    }
    
    /// <summary>
    /// Vertical alignment options for the <see cref="UIVerticalLayout"/>.
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// Align elements to the top.
        /// </summary>
        Top,
        
        /// <summary>
        /// Align elements to the center.
        /// </summary>
        Center,
        
        /// <summary>
        /// Align elements to the bottom.
        /// </summary>
        Bottom
    }
}