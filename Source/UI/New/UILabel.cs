using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Input;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// A label UI element for displaying text.
    /// </summary>
    public class UILabel : UIElement
    {
        private SpriteFont _font;
        private string _text;
        private Color _textColor;
        private bool _centered;
        private Vector2? _shadowOffset;
        private Color? _shadowColor;
        
        /// <summary>
        /// Gets or sets the text displayed by the label.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                UpdateSize();
            }
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
        /// Gets or sets a value indicating whether the text is centered.
        /// </summary>
        public bool Centered
        {
            get => _centered;
            set => _centered = value;
        }
        
        /// <summary>
        /// Gets or sets the shadow offset. Set to null to disable shadows.
        /// </summary>
        public Vector2? ShadowOffset
        {
            get => _shadowOffset;
            set => _shadowOffset = value;
        }
        
        /// <summary>
        /// Gets or sets the shadow color. Set to null to disable shadows.
        /// </summary>
        public Color? ShadowColor
        {
            get => _shadowColor;
            set => _shadowColor = value;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UILabel"/> class.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="text">The text to display.</param>
        /// <param name="position">The position of the label.</param>
        /// <param name="textColor">The text color.</param>
        /// <param name="centered">Whether the text is centered.</param>
        public UILabel(SpriteFont font, string text, Vector2 position, Color textColor, bool centered = false)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
            _text = text ?? string.Empty;
            _textColor = textColor;
            _centered = centered;
            
            // Calculate size based on text
            Vector2 size = font.MeasureString(text);
            Bounds = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UILabel"/> class with shadow effect.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="text">The text to display.</param>
        /// <param name="position">The position of the label.</param>
        /// <param name="textColor">The text color.</param>
        /// <param name="shadowColor">The shadow color.</param>
        /// <param name="shadowOffset">The shadow offset.</param>
        /// <param name="centered">Whether the text is centered.</param>
        public UILabel(SpriteFont font, string text, Vector2 position, Color textColor, 
                      Color shadowColor, Vector2 shadowOffset, bool centered = false)
            : this(font, text, position, textColor, centered)
        {
            _shadowColor = shadowColor;
            _shadowOffset = shadowOffset;
        }
        
        /// <summary>
        /// Initializes the label.
        /// </summary>
        public override void Initialize()
        {
            // No additional initialization needed
        }
        
        /// <summary>
        /// Updates the label.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // No update logic needed
        }
        
        /// <summary>
        /// Draws the label.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (string.IsNullOrEmpty(_text) || _font == null)
                return;
                
            Vector2 position = GetAbsolutePosition();
            
            // Adjust position for centered text
            if (_centered)
            {
                position.X -= Bounds.Width / 2;
                position.Y -= Bounds.Height / 2;
            }
            
            // Draw shadow if enabled
            if (_shadowOffset.HasValue && _shadowColor.HasValue)
            {
                spriteBatch.DrawString(_font, _text, position + _shadowOffset.Value, _shadowColor.Value);
            }
            
            // Draw main text
            spriteBatch.DrawString(_font, _text, position, _textColor);
        }
        
        /// <summary>
        /// Handles input for the label.
        /// </summary>
        /// <param name="inputManager">The input manager.</param>
        /// <returns>Always returns false as labels don't handle input.</returns>
        public override bool HandleInput(InputManager inputManager)
        {
            // Labels don't handle input
            return false;
        }
        
        /// <summary>
        /// Updates the size of the label based on the current text and font.
        /// </summary>
        private void UpdateSize()
        {
            if (_font != null)
            {
                Vector2 size = _font.MeasureString(_text);
                base.SetSize(size);
            }
        }
    }
}