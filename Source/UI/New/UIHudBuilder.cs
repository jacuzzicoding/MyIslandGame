using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.ECS;
using MyIslandGame.ECS.Components;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// Utility class for building and updating HUD elements.
    /// </summary>
    public class UIHudBuilder
    {
        private readonly UIManager _uiManager;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteFont _font;
        private readonly EntityManager _entityManager;
        
        private UIPanel _healthPanel;
        private UIProgressBar _healthBar;
        private UILabel _healthLabel;
        
        private UIPanel _statsPanel;
        private UILabel[] _statLabels;
        
        private UIPanel _compassPanel;
        private UILabel _directionLabel;
        
        private UIPanel _timePanel;
        private UILabel _timeLabel;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIHudBuilder"/> class.
        /// </summary>
        /// <param name="uiManager">The UI manager.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="font">The font to use for HUD elements.</param>
        /// <param name="entityManager">The entity manager.</param>
        public UIHudBuilder(
            UIManager uiManager, 
            GraphicsDevice graphicsDevice, 
            SpriteFont font, 
            EntityManager entityManager)
        {
            _uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _font = font ?? throw new ArgumentNullException(nameof(font));
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
        }
        
        /// <summary>
        /// Creates a health bar display.
        /// </summary>
        /// <param name="position">The position of the health bar.</param>
        /// <param name="width">The width of the health bar.</param>
        /// <param name="height">The height of the health bar.</param>
        /// <returns>The health panel containing the health bar.</returns>
        public UIPanel CreateHealthBar(Vector2 position, int width, int height)
        {
            // Create health panel
            _healthPanel = new UIPanel(
                _graphicsDevice,
                new Rectangle((int)position.X, (int)position.Y, width, height),
                new Color(0, 0, 0, 150),
                Color.DarkGray,
                1);
                
            // Create health bar
            _healthBar = new UIProgressBar(
                _graphicsDevice,
                new Rectangle(5, 5, width - 10, height - 25),
                Color.DarkRed,
                Color.Red,
                Color.DarkGray,
                1,
                100,
                100);
                
            // Create health label
            _healthLabel = new UILabel(
                _font,
                "100 / 100",
                new Vector2(width / 2, height - 15),
                Color.White,
                true);
                
            // Add to panel
            _healthPanel.AddChild(_healthBar);
            _healthPanel.AddChild(_healthLabel);
            
            // Register and add to UI
            _uiManager.RegisterElement("healthPanel", _healthPanel);
            _uiManager.AddToCanvas(_healthPanel, UILayer.HUD);
            
            return _healthPanel;
        }
        
        /// <summary>
        /// Creates a stats panel.
        /// </summary>
        /// <param name="position">The position of the stats panel.</param>
        /// <param name="width">The width of the stats panel.</param>
        /// <param name="height">The height of the stats panel.</param>
        /// <param name="statNames">The names of the stats to display.</param>
        /// <returns>The stats panel.</returns>
        public UIPanel CreateStatsPanel(Vector2 position, int width, int height, string[] statNames)
        {
            // Create stats panel
            _statsPanel = new UIPanel(
                _graphicsDevice,
                new Rectangle((int)position.X, (int)position.Y, width, height),
                new Color(0, 0, 0, 150),
                Color.DarkGray,
                1);
                
            // Create stat labels
            _statLabels = new UILabel[statNames.Length];
            
            for (int i = 0; i < statNames.Length; i++)
            {
                _statLabels[i] = new UILabel(
                    _font,
                    $"{statNames[i]}: 0",
                    new Vector2(10, 10 + i * 20),
                    Color.White);
                    
                _statsPanel.AddChild(_statLabels[i]);
            }
            
            // Register and add to UI
            _uiManager.RegisterElement("statsPanel", _statsPanel);
            _uiManager.AddToCanvas(_statsPanel, UILayer.HUD);
            
            return _statsPanel;
        }
        
        /// <summary>
        /// Creates a compass display.
        /// </summary>
        /// <param name="position">The position of the compass.</param>
        /// <param name="width">The width of the compass.</param>
        /// <param name="height">The height of the compass.</param>
        /// <returns>The compass panel.</returns>
        public UIPanel CreateCompass(Vector2 position, int width, int height)
        {
            // Create compass panel
            _compassPanel = new UIPanel(
                _graphicsDevice,
                new Rectangle((int)position.X, (int)position.Y, width, height),
                new Color(0, 0, 0, 150),
                Color.DarkGray,
                1);
                
            // Create direction label
            _directionLabel = new UILabel(
                _font,
                "N",
                new Vector2(width / 2, height / 2),
                Color.White,
                true);
                
            // Add to panel
            _compassPanel.AddChild(_directionLabel);
            
            // Register and add to UI
            _uiManager.RegisterElement("compassPanel", _compassPanel);
            _uiManager.AddToCanvas(_compassPanel, UILayer.HUD);
            
            return _compassPanel;
        }
        
        /// <summary>
        /// Creates a time display.
        /// </summary>
        /// <param name="position">The position of the time display.</param>
        /// <param name="width">The width of the time display.</param>
        /// <param name="height">The height of the time display.</param>
        /// <returns>The time panel.</returns>
        public UIPanel CreateTimeDisplay(Vector2 position, int width, int height)
        {
            // Create time panel
            _timePanel = new UIPanel(
                _graphicsDevice,
                new Rectangle((int)position.X, (int)position.Y, width, height),
                new Color(0, 0, 0, 150),
                Color.DarkGray,
                1);
                
            // Create time label
            _timeLabel = new UILabel(
                _font,
                "00:00",
                new Vector2(width / 2, height / 2),
                Color.White,
                true);
                
            // Add to panel
            _timePanel.AddChild(_timeLabel);
            
            // Register and add to UI
            _uiManager.RegisterElement("timePanel", _timePanel);
            _uiManager.AddToCanvas(_timePanel, UILayer.HUD);
            
            return _timePanel;
        }
        
        /// <summary>
        /// Updates the HUD elements with the latest game data.
        /// </summary>
        public void UpdateHUD()
        {
            UpdateHealthBar();
            UpdateStatsPanel();
            UpdateCompass();
            UpdateTimeDisplay();
        }
        
        /// <summary>
        /// Updates the health bar with the player's current health.
        /// </summary>
        private void UpdateHealthBar()
        {
            if (_healthBar == null || _healthLabel == null)
                return;
                
            // Find player entity (assuming single player game)
            var playerEntities = _entityManager.GetEntitiesWithComponents(typeof(PlayerComponent));
            var playerEnumerator = playerEntities.GetEnumerator();
            
            if (playerEnumerator.MoveNext())
            {
                var player = playerEnumerator.Current;
                
                // In a real implementation, you would get the player's health component
                // and update the health bar with the current and maximum health
                int currentHealth = 75; // Example value
                int maxHealth = 100;    // Example value
                
                _healthBar.CurrentValue = currentHealth;
                _healthBar.MaxValue = maxHealth;
                _healthLabel.Text = $"{currentHealth} / {maxHealth}";
            }
        }
        
        /// <summary>
        /// Updates the stats panel with the player's current stats.
        /// </summary>
        private void UpdateStatsPanel()
        {
            if (_statLabels == null)
                return;
                
            // Find player entity (assuming single player game)
            var playerEntities = _entityManager.GetEntitiesWithComponents(typeof(PlayerComponent));
            var playerEnumerator = playerEntities.GetEnumerator();
            
            if (playerEnumerator.MoveNext())
            {
                var player = playerEnumerator.Current;
                
                // In a real implementation, you would get the player's stat components
                // and update the stat labels with the current values
                int[] statValues = { 10, 8, 12, 6 }; // Example values
                
                for (int i = 0; i < _statLabels.Length && i < statValues.Length; i++)
                {
                    // Extract the stat name from the current text
                    string statName = _statLabels[i].Text.Split(':')[0];
                    _statLabels[i].Text = $"{statName}: {statValues[i]}";
                }
            }
        }
        
        /// <summary>
        /// Updates the compass with the player's current direction.
        /// </summary>
        private void UpdateCompass()
        {
            if (_directionLabel == null)
                return;
                
            // Find player entity (assuming single player game)
            var playerEntities = _entityManager.GetEntitiesWithComponents(typeof(PlayerComponent));
            var playerEnumerator = playerEntities.GetEnumerator();
            
            if (playerEnumerator.MoveNext())
            {
                var player = playerEnumerator.Current;
                
                // In a real implementation, you would get the player's transform component
                // and calculate the direction based on the rotation
                float rotation = 0.0f; // Example value
                
                // Convert rotation to direction
                string direction = "N";
                
                if (rotation >= -MathHelper.PiOver4 && rotation < MathHelper.PiOver4)
                {
                    direction = "N";
                }
                else if (rotation >= MathHelper.PiOver4 && rotation < 3 * MathHelper.PiOver4)
                {
                    direction = "E";
                }
                else if (rotation >= 3 * MathHelper.PiOver4 || rotation < -3 * MathHelper.PiOver4)
                {
                    direction = "S";
                }
                else
                {
                    direction = "W";
                }
                
                _directionLabel.Text = direction;
            }
        }
        
        /// <summary>
        /// Updates the time display with the current game time.
        /// </summary>
        private void UpdateTimeDisplay()
        {
            if (_timeLabel == null)
                return;
                
            // In a real implementation, you would get the game's time manager
            // and update the time label with the current time
            int hours = 12;   // Example value
            int minutes = 30; // Example value
            
            _timeLabel.Text = $"{hours:00}:{minutes:00}";
        }
    }
    
    /// <summary>
    /// A progress bar UI element.
    /// </summary>
    public class UIProgressBar : UIElement
    {
        private Texture2D _backgroundTexture;
        private Texture2D _fillTexture;
        private Color _backgroundColor;
        private Color _fillColor;
        private Color _borderColor;
        private int _borderWidth;
        private float _currentValue;
        private float _maxValue;
        
        /// <summary>
        /// Gets or sets the current value of the progress bar.
        /// </summary>
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = MathHelper.Clamp(value, 0, _maxValue);
                UpdateFillTexture();
            }
        }
        
        /// <summary>
        /// Gets or sets the maximum value of the progress bar.
        /// </summary>
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = Math.Max(1, value);
                _currentValue = MathHelper.Clamp(_currentValue, 0, _maxValue);
                UpdateFillTexture();
            }
        }
        
        /// <summary>
        /// Gets the current progress as a value between 0 and 1.
        /// </summary>
        public float Progress => _currentValue / _maxValue;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIProgressBar"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="bounds">The bounds of the progress bar.</param>
        /// <param name="backgroundColor">The background color.</param>
        /// <param name="fillColor">The fill color.</param>
        /// <param name="borderColor">The border color.</param>
        /// <param name="borderWidth">The border width.</param>
        /// <param name="currentValue">The initial current value.</param>
        /// <param name="maxValue">The initial maximum value.</param>
        public UIProgressBar(
            GraphicsDevice graphicsDevice,
            Rectangle bounds,
            Color backgroundColor,
            Color fillColor,
            Color borderColor,
            int borderWidth,
            float currentValue,
            float maxValue)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));
                
            Bounds = bounds;
            _backgroundColor = backgroundColor;
            _fillColor = fillColor;
            _borderColor = borderColor;
            _borderWidth = Math.Max(0, borderWidth);
            _maxValue = Math.Max(1, maxValue);
            _currentValue = MathHelper.Clamp(currentValue, 0, _maxValue);
            
            CreateTextures(graphicsDevice);
            UpdateFillTexture();
        }
        
        /// <summary>
        /// Initializes the progress bar.
        /// </summary>
        public override void Initialize()
        {
            // No additional initialization needed
        }
        
        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // No update logic needed
        }
        
        /// <summary>
        /// Draws the progress bar.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 position = GetAbsolutePosition();
            
            // Draw background
            spriteBatch.Draw(_backgroundTexture, position, Color.White);
            
            // Calculate fill width
            int fillWidth = (int)(Bounds.Width * Progress);
            
            if (fillWidth > 0)
            {
                // Draw fill
                Rectangle fillRect = new Rectangle(0, 0, fillWidth, Bounds.Height);
                spriteBatch.Draw(_fillTexture, position, fillRect, Color.White);
            }
        }
        
        /// <summary>
        /// Handles input for the progress bar.
        /// </summary>
        /// <param name="inputManager">The input manager.</param>
        /// <returns>Always returns false as progress bars don't handle input.</returns>
        public override bool HandleInput(Input.InputManager inputManager)
        {
            // Progress bars don't handle input
            return false;
        }
        
        /// <summary>
        /// Creates the textures for the progress bar.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        private void CreateTextures(GraphicsDevice graphicsDevice)
        {
            // Create background texture
            _backgroundTexture = new Texture2D(graphicsDevice, Bounds.Width, Bounds.Height);
            Color[] bgData = new Color[Bounds.Width * Bounds.Height];
            
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
                        bgData[index] = _borderColor;
                    }
                    else
                    {
                        bgData[index] = _backgroundColor;
                    }
                }
            }
            
            _backgroundTexture.SetData(bgData);
            
            // Create fill texture (will be updated in UpdateFillTexture)
            _fillTexture = new Texture2D(graphicsDevice, Bounds.Width, Bounds.Height);
        }
        
        /// <summary>
        /// Updates the fill texture based on the current progress.
        /// </summary>
        private void UpdateFillTexture()
        {
            if (_fillTexture == null)
                return;
                
            Color[] fillData = new Color[Bounds.Width * Bounds.Height];
            
            for (int y = 0; y < Bounds.Height; y++)
            {
                for (int x = 0; x < Bounds.Width; x++)
                {
                    int index = y * Bounds.Width + x;
                    
                    // Only fill if within the border
                    if (_borderWidth > 0 && 
                        (x < _borderWidth || x >= Bounds.Width - _borderWidth || 
                         y < _borderWidth || y >= Bounds.Height - _borderWidth))
                    {
                        fillData[index] = Color.Transparent;
                    }
                    else
                    {
                        fillData[index] = _fillColor;
                    }
                }
            }
            
            _fillTexture.SetData(fillData);
        }
        
        /// <summary>
        /// Sets the size of the progress bar and updates the textures.
        /// </summary>
        /// <param name="size">The new size.</param>
        public override void SetSize(Vector2 size)
        {
            // Store current values
            Color backgroundColor = _backgroundColor;
            Color fillColor = _fillColor;
            Color borderColor = _borderColor;
            int borderWidth = _borderWidth;
            float currentValue = _currentValue;
            float maxValue = _maxValue;
            
            // Update bounds
            base.SetSize(size);
            
            // Recreate textures
            if (_backgroundTexture != null && _backgroundTexture.GraphicsDevice != null)
            {
                _backgroundTexture.Dispose();
                _fillTexture.Dispose();
                
                CreateTextures(_backgroundTexture.GraphicsDevice);
                UpdateFillTexture();
            }
        }
    }
}