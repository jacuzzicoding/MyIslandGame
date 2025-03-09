using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// A horizontal layout container for UI elements.
    /// </summary>
    public class UIHorizontalLayout : UIContainer
    {
        private int _spacing;
        private HorizontalAlignment _alignment;
        
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
        /// Gets or sets the horizontal alignment of elements.
        /// </summary>
        public HorizontalAlignment Alignment
        {
            get => _alignment;
            set
            {
                _alignment = value;
                UpdateLayout();
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIHorizontalLayout"/> class.
        /// </summary>
        /// <param name="bounds">The bounds of the layout.</param>
        /// <param name="spacing">The spacing between elements.</param>
        /// <param name="alignment">The horizontal alignment of elements.</param>
        public UIHorizontalLayout(Rectangle bounds, int spacing = 0, HorizontalAlignment alignment = HorizontalAlignment.Left)
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
                
            // Calculate total width of all children plus spacing
            int totalWidth = 0;
            int maxHeight = 0;
            
            foreach (var child in Children)
            {
                totalWidth += child.Bounds.Width;
                maxHeight = Math.Max(maxHeight, child.Bounds.Height);
            }
            
            // Add spacing between elements
            totalWidth += _spacing * (Children.Count - 1);
            
            // Calculate starting X position based on alignment
            int startX;
            
            switch (_alignment)
            {
                case HorizontalAlignment.Left:
                    startX = Bounds.X;
                    break;
                    
                case HorizontalAlignment.Center:
                    startX = Bounds.X + (Bounds.Width - totalWidth) / 2;
                    break;
                    
                case HorizontalAlignment.Right:
                    startX = Bounds.X + Bounds.Width - totalWidth;
                    break;
                    
                default:
                    startX = Bounds.X;
                    break;
            }
            
            // Calculate vertical position (centered within the layout bounds)
            int centerY = Bounds.Y + Bounds.Height / 2;
            
            // Position each child
            int currentX = startX;
            
            foreach (var child in Children)
            {
                // Center vertically
                int y = centerY - child.Bounds.Height / 2;
                
                // Set position
                child.SetPosition(new Vector2(currentX, y));
                
                // Move to next position
                currentX += child.Bounds.Width + _spacing;
            }
        }
    }
    
    /// <summary>
    /// Horizontal alignment options for the <see cref="UIHorizontalLayout"/>.
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// Align elements to the left.
        /// </summary>
        Left,
        
        /// <summary>
        /// Align elements to the center.
        /// </summary>
        Center,
        
        /// <summary>
        /// Align elements to the right.
        /// </summary>
        Right
    }
}