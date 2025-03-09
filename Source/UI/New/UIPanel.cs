using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// A panel container that can hold other UI elements.
    /// </summary>
    public class UIPanel : UIContainer
    {
        private Texture2D _backgroundTexture;
        private Color _backgroundColor;
        private int _borderWidth;
        private Color _borderColor;
        
        /// <summary>
        /// Gets or sets the background color of the panel.
        /// </summary>
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                UpdateBackgroundTexture();
            }
        }
        
        /// <summary>
        /// Gets or sets the border color of the panel.
        /// </summary>
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                UpdateBackgroundTexture();
            }
        }
        
        /// <summary>
        /// Gets or sets the border width of the panel.
        /// </summary>
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = Math.Max(0, value);
                UpdateBackgroundTexture();
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIPanel"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="bounds">The bounds of the panel.</param>
        /// <param name="backgroundColor">The background color.</param>
        /// <param name="borderColor">The border color.</param>
        /// <param name="borderWidth">The border width.</param>
        public UIPanel(GraphicsDevice graphicsDevice, Rectangle bounds, Color backgroundColor, 
                       Color borderColor = default, int borderWidth = 0)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));
                
            Bounds = bounds;
            _backgroundColor = backgroundColor;
            _borderColor = borderColor == default ? Color.Black : borderColor;
            _borderWidth = Math.Max(0, borderWidth);
            
            CreateBackgroundTexture(graphicsDevice);
        }
        
        /// <summary>
        /// Initializes the panel and its children.
        /// </summary>
        public override void Initialize()
        {
            // Initialize children
            foreach (var child in Children)
            {
                child.Initialize();
            }
        }
        
        /// <summary>
        /// Updates the panel and its children.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        
        /// <summary>
        /// Draws the panel and its children.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw panel background
            spriteBatch.Draw(_backgroundTexture, GetAbsolutePosition(), Color.White);
            
            // Draw children
            base.Draw(spriteBatch);
        }
        
        /// <summary>
        /// Creates the background texture for the panel.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        private void CreateBackgroundTexture(GraphicsDevice graphicsDevice)
        {
            if (Bounds.Width <= 0 || Bounds.Height <= 0)
                return;
                
            _backgroundTexture = new Texture2D(graphicsDevice, Bounds.Width, Bounds.Height);
            UpdateBackgroundTexture();
        }
        
        /// <summary>
        /// Updates the background texture based on current color and border settings.
        /// </summary>
        private void UpdateBackgroundTexture()
        {
            if (_backgroundTexture == null || Bounds.Width <= 0 || Bounds.Height <= 0)
                return;
                
            Color[] data = new Color[Bounds.Width * Bounds.Height];
            
            for (int y = 0; y < Bounds.Height; y++)
            {
                for (int x = 0; x < Bounds.Width; x++)
                {
                    int index = y * Bounds.Width + x;
                    
                    // Draw border if needed
                    if (_borderWidth > 0 && 
                        (x < _borderWidth || x >= Bounds.Width - _borderWidth || 
                         y < _borderWidth || y >= Bounds.Height - _borderWidth))
                    {
                        data[index] = _borderColor;
                    }
                    else
                    {
                        data[index] = _backgroundColor;
                    }
                }
            }
            
            _backgroundTexture.SetData(data);
        }
        
        /// <summary>
        /// Sets the size of the panel and updates the background texture.
        /// </summary>
        /// <param name="size">The new size.</param>
        public override void SetSize(Vector2 size)
        {
            // Update bounds
            base.SetSize(size);
            
            // Recreate texture with new size
            if (_backgroundTexture != null && _backgroundTexture.GraphicsDevice != null)
            {
                _backgroundTexture.Dispose();
                CreateBackgroundTexture(_backgroundTexture.GraphicsDevice);
            }
        }
    }
}