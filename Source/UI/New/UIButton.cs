using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// A button UI element that can be clicked.
    /// </summary>
    public class UIButton : InteractableElement
    {
        private Texture2D _normalTexture;
        private Texture2D _hoverTexture;
        private Texture2D _pressedTexture;
        private Texture2D _disabledTexture;
        private SpriteFont _font;
        private string _text;
        private Color _textColor;
        
        /// <summary>
        /// Gets or sets the text displayed on the button.
        /// </summary>
        public string Text
        {
            get => _text;
            set => _text = value ?? string.Empty;
        }
        
        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIButton"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="bounds">The bounds of the button.</param>
        /// <param name="normalColor">The normal state color.</param>
        /// <param name="hoverColor">The hover state color.</param>
        /// <param name="pressedColor">The pressed state color.</param>
        /// <param name="font">The font to use for text.</param>
        /// <param name="text">The text to display.</param>
        /// <param name="textColor">The text color.</param>
        public UIButton(
            GraphicsDevice graphicsDevice,
            Rectangle bounds,
            Color normalColor,
            Color hoverColor,
            Color pressedColor,
            SpriteFont font,
            string text,
            Color textColor)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));
                
            Bounds = bounds;
            _font = font;
            _text = text ?? string.Empty;
            _textColor = textColor;
            
            // Create textures for different button states
            _normalTexture = CreateColorTexture(graphicsDevice, bounds.Width, bounds.Height, normalColor);
            _hoverTexture = CreateColorTexture(graphicsDevice, bounds.Width, bounds.Height, hoverColor);
            _pressedTexture = CreateColorTexture(graphicsDevice, bounds.Width, bounds.Height, pressedColor);
            
            // Create a semi-transparent version for disabled state
            Color disabledColor = new Color(normalColor.R, normalColor.G, normalColor.B, normalColor.A / 2);
            _disabledTexture = CreateColorTexture(graphicsDevice, bounds.Width, bounds.Height, disabledColor);
        }
        
        /// <summary>
        /// Initializes the button.
        /// </summary>
        public override void Initialize()
        {
            // No additional initialization needed
        }
        
        /// <summary>
        /// Updates the button.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // No additional update logic needed
        }
        
        /// <summary>
        /// Draws the button.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Choose the appropriate texture based on button state
            Texture2D texture;
            
            if (!IsActive)
            {
                texture = _disabledTexture;
            }
            else if (IsPressed)
            {
                texture = _pressedTexture;
            }
            else if (IsHovered)
            {
                texture = _hoverTexture;
            }
            else
            {
                texture = _normalTexture;
            }
            
            // Draw button background
            spriteBatch.Draw(texture, GetAbsolutePosition(), Color.White);
            
            // Draw text if provided
            if (!string.IsNullOrEmpty(_text) && _font != null)
            {
                Vector2 textSize = _font.MeasureString(_text);
                Vector2 textPos = GetAbsolutePosition() + new Vector2(
                    (Bounds.Width - textSize.X) / 2,
                    (Bounds.Height - textSize.Y) / 2);
                    
                spriteBatch.DrawString(_font, _text, textPos, _textColor);
            }
        }
        
        /// <summary>
        /// Sets the size of the button and updates the textures.
        /// </summary>
        /// <param name="size">The new size.</param>
        public override void SetSize(Vector2 size)
        {
            // Store the current state colors before disposing textures
            Color normalColor = new Color();
            Color hoverColor = new Color();
            Color pressedColor = new Color();
            Color disabledColor = new Color();
            
            if (_normalTexture != null)
            {
                // Sample the center pixel to get the color
                Color[] normalData = new Color[1];
                _normalTexture.GetData(0, new Rectangle(_normalTexture.Width / 2, _normalTexture.Height / 2, 1, 1), normalData, 0, 1);
                normalColor = normalData[0];
                
                Color[] hoverData = new Color[1];
                _hoverTexture.GetData(0, new Rectangle(_hoverTexture.Width / 2, _hoverTexture.Height / 2, 1, 1), hoverData, 0, 1);
                hoverColor = hoverData[0];
                
                Color[] pressedData = new Color[1];
                _pressedTexture.GetData(0, new Rectangle(_pressedTexture.Width / 2, _pressedTexture.Height / 2, 1, 1), pressedData, 0, 1);
                pressedColor = pressedData[0];
                
                Color[] disabledData = new Color[1];
                _disabledTexture.GetData(0, new Rectangle(_disabledTexture.Width / 2, _disabledTexture.Height / 2, 1, 1), disabledData, 0, 1);
                disabledColor = disabledData[0];
                
                // Dispose old textures
                _normalTexture.Dispose();
                _hoverTexture.Dispose();
                _pressedTexture.Dispose();
                _disabledTexture.Dispose();
                
                // Update bounds
                base.SetSize(size);
                
                // Recreate textures with new size
                GraphicsDevice graphicsDevice = _normalTexture.GraphicsDevice;
                _normalTexture = CreateColorTexture(graphicsDevice, Bounds.Width, Bounds.Height, normalColor);
                _hoverTexture = CreateColorTexture(graphicsDevice, Bounds.Width, Bounds.Height, hoverColor);
                _pressedTexture = CreateColorTexture(graphicsDevice, Bounds.Width, Bounds.Height, pressedColor);
                _disabledTexture = CreateColorTexture(graphicsDevice, Bounds.Width, Bounds.Height, disabledColor);
            }
            else
            {
                // Just update bounds if textures are not initialized
                base.SetSize(size);
            }
        }
    }
}