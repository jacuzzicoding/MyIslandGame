using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// A notification UI element that displays a message for a specified duration.
    /// </summary>
    public class UINotification : UIPanel
    {
        private UILabel _messageLabel;
        private float _duration;
        private float _remainingTime;
        private float _fadeInTime;
        private float _fadeOutTime;
        private Color _startColor;
        private Color _endColor;
        private bool _isFadingIn;
        private Action _onComplete;
        
        /// <summary>
        /// Gets or sets the message to display.
        /// </summary>
        public string Message
        {
            get => _messageLabel.Text;
            set => _messageLabel.Text = value;
        }
        
        /// <summary>
        /// Gets the remaining time until the notification is removed.
        /// </summary>
        public float RemainingTime => _remainingTime;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UINotification"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="bounds">The bounds of the notification.</param>
        /// <param name="backgroundColor">The background color.</param>
        /// <param name="borderColor">The border color.</param>
        /// <param name="borderWidth">The border width.</param>
        /// <param name="font">The font to use for the message.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="textColor">The text color.</param>
        /// <param name="duration">The duration to display the notification in seconds.</param>
        /// <param name="fadeInTime">The fade-in time in seconds.</param>
        /// <param name="fadeOutTime">The fade-out time in seconds.</param>
        /// <param name="onComplete">An action to execute when the notification is removed.</param>
        public UINotification(
            GraphicsDevice graphicsDevice,
            Rectangle bounds,
            Color backgroundColor,
            Color borderColor,
            int borderWidth,
            SpriteFont font,
            string message,
            Color textColor,
            float duration = 3.0f,
            float fadeInTime = 0.25f,
            float fadeOutTime = 0.5f,
            Action onComplete = null)
            : base(graphicsDevice, bounds, backgroundColor, borderColor, borderWidth)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));
                
            // Create message label
            _messageLabel = new UILabel(
                font,
                message ?? string.Empty,
                new Vector2(10, 10),
                textColor);
                
            AddChild(_messageLabel);
            
            // Set up timing
            _duration = duration;
            _remainingTime = duration;
            _fadeInTime = fadeInTime;
            _fadeOutTime = fadeOutTime;
            _onComplete = onComplete;
            
            // Set up fading
            _startColor = new Color(
                (byte)backgroundColor.R,
                (byte)backgroundColor.G,
                (byte)backgroundColor.B,
                (byte)0);
                
            _endColor = backgroundColor;
            
            // Start fading in
            BackgroundColor = _startColor;
            _isFadingIn = true;
        }
        
        /// <summary>
        /// Updates the notification.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update remaining time
            _remainingTime -= deltaTime;
            
            // Handle fading
            if (_isFadingIn)
            {
                // Fade in
                float progress = Math.Min(1.0f, 1.0f - (_remainingTime - _duration + _fadeInTime) / _fadeInTime);
                BackgroundColor = LerpColor(_startColor, _endColor, progress);
                
                // Check if fade-in is complete
                if (progress >= 1.0f)
                {
                    _isFadingIn = false;
                }
            }
            else if (_remainingTime <= _fadeOutTime)
            {
                // Fade out
                float progress = Math.Min(1.0f, _remainingTime / _fadeOutTime);
                BackgroundColor = LerpColor(_endColor, _startColor, 1.0f - progress);
            }
            
            // Check if notification should be removed
            if (_remainingTime <= 0)
            {
                // Remove from parent
                if (Parent != null)
                {
                    Parent.RemoveChild(this);
                    
                    // Execute completion callback
                    _onComplete?.Invoke();
                }
            }
            
            base.Update(gameTime);
        }
        
        /// <summary>
        /// Linearly interpolates between two colors.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <param name="amount">The interpolation amount (0.0 to 1.0).</param>
        /// <returns>The interpolated color.</returns>
        private Color LerpColor(Color color1, Color color2, float amount)
        {
            return new Color(
                (byte)MathHelper.Lerp(color1.R, color2.R, amount),
                (byte)MathHelper.Lerp(color1.G, color2.G, amount),
                (byte)MathHelper.Lerp(color1.B, color2.B, amount),
                (byte)MathHelper.Lerp(color1.A, color2.A, amount));
        }
    }
    
    /// <summary>
    /// A manager for displaying notifications in the game.
    /// </summary>
    public class NotificationManager
    {
        private readonly UIContainer _container;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteFont _font;
        private int _maxNotifications;
        private int _notificationCount;
        private Vector2 _position;
        private int _spacing;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationManager"/> class.
        /// </summary>
        /// <param name="uiManager">The UI manager.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="font">The font to use for notifications.</param>
        /// <param name="position">The position of the notification container.</param>
        /// <param name="maxNotifications">The maximum number of notifications to display at once.</param>
        /// <param name="spacing">The spacing between notifications.</param>
        public NotificationManager(
            UIManager uiManager,
            GraphicsDevice graphicsDevice,
            SpriteFont font,
            Vector2 position,
            int maxNotifications = 5,
            int spacing = 5)
        {
            if (uiManager == null)
                throw new ArgumentNullException(nameof(uiManager));
                
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));
                
            if (font == null)
                throw new ArgumentNullException(nameof(font));
                
            _graphicsDevice = graphicsDevice;
            _font = font;
            _maxNotifications = maxNotifications;
            _position = position;
            _spacing = spacing;
            
            // Create container for notifications
            _container = new UIVerticalLayout(
                new Rectangle((int)position.X, (int)position.Y, 300, 500),
                spacing);
                
            // Register and add to UI
            uiManager.RegisterElement("notificationContainer", _container);
            uiManager.AddToCanvas(_container, UILayer.HUD);
        }
        
        /// <summary>
        /// Shows a notification message.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="duration">The duration to display the message in seconds.</param>
        /// <param name="backgroundColor">The background color of the notification.</param>
        /// <param name="textColor">The text color of the message.</param>
        /// <param name="borderColor">The border color of the notification.</param>
        public void ShowNotification(
            string message,
            float duration = 3.0f,
            Color? backgroundColor = null,
            Color? textColor = null,
            Color? borderColor = null)
        {
            // Use default colors if not specified
            backgroundColor ??= new Color(0, 0, 0, 200);
            textColor ??= Color.White;
            borderColor ??= Color.Yellow;
            
            // Measure text to determine notification size
            Vector2 textSize = _font.MeasureString(message);
            
            // Create notification
            var notification = new UINotification(
                _graphicsDevice,
                new Rectangle(0, 0, (int)textSize.X + 20, (int)textSize.Y + 20),
                backgroundColor.Value,
                borderColor.Value,
                1,
                _font,
                message,
                textColor.Value,
                duration,
                0.25f,
                0.5f,
                OnNotificationRemoved);
                
            // Check if we need to remove an older notification
            if (_notificationCount >= _maxNotifications && _container.Children.Count > 0)
            {
                // Remove the oldest notification
                _container.RemoveChild(_container.Children[0]);
                _notificationCount--;
            }
            
            // Add the new notification
            _container.AddChild(notification);
            _notificationCount++;
        }
        
        /// <summary>
        /// Called when a notification is removed.
        /// </summary>
        private void OnNotificationRemoved()
        {
            _notificationCount--;
        }
        
        /// <summary>
        /// Clears all notifications.
        /// </summary>
        public void ClearNotifications()
        {
            _container.ClearChildren();
            _notificationCount = 0;
        }
    }
}