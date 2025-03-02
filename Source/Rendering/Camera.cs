using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.Rendering
{
    /// <summary>
    /// Represents a 2D camera that handles view transformations.
    /// </summary>
    public class Camera
    {
        private Vector2 _position;
        private float _zoom;
        private float _rotation;
        private Vector2 _origin;
        private Matrix _transformMatrix;
        private bool _transformDirty;
        private Viewport _viewport;
        
        /// <summary>
        /// Gets or sets the camera position.
        /// </summary>
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    _transformDirty = true;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the camera zoom. Values greater than 1 zoom in, less than 1 zoom out.
        /// </summary>
        public float Zoom
        {
            get => _zoom;
            set
            {
                if (_zoom != value)
                {
                    _zoom = MathHelper.Clamp(value, 0.1f, 10f);
                    _transformDirty = true;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the camera rotation in radians.
        /// </summary>
        public float Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    _transformDirty = true;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the camera origin (center point).
        /// </summary>
        public Vector2 Origin
        {
            get => _origin;
            set
            {
                if (_origin != value)
                {
                    _origin = value;
                    _transformDirty = true;
                }
            }
        }
        
        /// <summary>
        /// Gets the viewport dimensions.
        /// </summary>
        public Viewport Viewport => _viewport;
        
        /// <summary>
        /// Gets the width of the viewport.
        /// </summary>
        public int ViewportWidth => _viewport.Width;
        
        /// <summary>
        /// Gets the height of the viewport.
        /// </summary>
        public int ViewportHeight => _viewport.Height;
        
        /// <summary>
        /// Gets the bounds of the camera view.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                Vector2 topLeft = ScreenToWorld(Vector2.Zero);
                Vector2 bottomRight = ScreenToWorld(new Vector2(_viewport.Width, _viewport.Height));
                
                int width = (int)(bottomRight.X - topLeft.X);
                int height = (int)(bottomRight.Y - topLeft.Y);
                
                return new Rectangle((int)topLeft.X, (int)topLeft.Y, width, height);
            }
        }
        
        /// <summary>
        /// Gets the transformation matrix for rendering.
        /// </summary>
        public Matrix TransformMatrix
        {
            get
            {
                if (_transformDirty)
                {
                    // Calculate the transform matrix
                    _transformMatrix = 
                        Matrix.CreateTranslation(new Vector3(-_position, 0)) *
                        Matrix.CreateRotationZ(_rotation) *
                        Matrix.CreateScale(new Vector3(_zoom, _zoom, 1)) *
                        Matrix.CreateTranslation(new Vector3(_origin, 0));
                    
                    _transformDirty = false;
                }
                
                return _transformMatrix;
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        public Camera(Viewport viewport)
        {
            _viewport = viewport;
            _position = Vector2.Zero;
            _zoom = 1.0f;
            _rotation = 0.0f;
            _origin = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
            _transformDirty = true;
        }
        
        /// <summary>
        /// Updates the camera to follow a target position.
        /// </summary>
        /// <param name="targetPosition">The target position to follow.</param>
        /// <param name="smoothness">The smoothness factor (0 = instant, higher values = smoother).</param>
        public void FollowTarget(Vector2 targetPosition, float smoothness = 0.1f)
        {
            if (smoothness <= 0)
            {
                Position = targetPosition;
            }
            else
            {
                Position = Vector2.Lerp(Position, targetPosition, 1 / smoothness);
            }
        }
        
        /// <summary>
        /// Restricts the camera position to stay within the specified boundaries.
        /// </summary>
        /// <param name="worldBounds">The boundaries in world coordinates.</param>
        public void ClampToBounds(Rectangle worldBounds)
        {
            Rectangle bounds = Bounds;
            Vector2 newPosition = Position;
            
            // Calculate camera movement limits
            float minX = worldBounds.Left + bounds.Width / 2f;
            float maxX = worldBounds.Right - bounds.Width / 2f;
            float minY = worldBounds.Top + bounds.Height / 2f;
            float maxY = worldBounds.Bottom - bounds.Height / 2f;
            
            // Adjust limits based on zoom
            if (bounds.Width > worldBounds.Width)
            {
                minX = maxX = worldBounds.Center.X;
            }
            
            if (bounds.Height > worldBounds.Height)
            {
                minY = maxY = worldBounds.Center.Y;
            }
            
            // Clamp position
            newPosition.X = MathHelper.Clamp(newPosition.X, minX, maxX);
            newPosition.Y = MathHelper.Clamp(newPosition.Y, minY, maxY);
            
            Position = newPosition;
        }
        
        /// <summary>
        /// Converts a screen position to a world position.
        /// </summary>
        /// <param name="screenPosition">The screen position.</param>
        /// <returns>The corresponding world position.</returns>
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            Matrix inverseViewMatrix = Matrix.Invert(TransformMatrix);
            return Vector2.Transform(screenPosition, inverseViewMatrix);
        }
        
        /// <summary>
        /// Converts a world position to a screen position.
        /// </summary>
        /// <param name="worldPosition">The world position.</param>
        /// <returns>The corresponding screen position.</returns>
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, TransformMatrix);
        }
        
        /// <summary>
        /// Moves the camera by the specified amount.
        /// </summary>
        /// <param name="direction">The direction to move.</param>
        /// <param name="amount">The amount to move.</param>
        public void Move(Vector2 direction, float amount)
        {
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                Position += direction * amount;
            }
        }
        
        /// <summary>
        /// Zooms the camera by the specified amount.
        /// </summary>
        /// <param name="zoomAmount">The amount to zoom (positive = zoom in, negative = zoom out).</param>
        public void ZoomBy(float zoomAmount)
        {
            Zoom += zoomAmount;
        }
        
        /// <summary>
        /// Centers the camera on the specified position.
        /// </summary>
        /// <param name="position">The position to center on.</param>
        public void CenterOn(Vector2 position)
        {
            Position = position;
        }
        
        /// <summary>
        /// Updates the viewport when window size changes.
        /// </summary>
        /// <param name="viewport">The new viewport.</param>
        public void UpdateViewport(Viewport viewport)
        {
            _viewport = viewport;
            _origin = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
            _transformDirty = true;
        }
    }
}
