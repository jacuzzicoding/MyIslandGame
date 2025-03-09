using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// Encapsulates SpriteBatch.Begin parameters for a consistent rendering configuration.
    /// </summary>
    public class SpriteBatchSettings
    {
        /// <summary>
        /// Gets the sprite sort mode.
        /// </summary>
        public SpriteSortMode SortMode { get; }
        
        /// <summary>
        /// Gets the blend state.
        /// </summary>
        public BlendState BlendState { get; }
        
        /// <summary>
        /// Gets the sampler state.
        /// </summary>
        public SamplerState SamplerState { get; }
        
        /// <summary>
        /// Gets the depth stencil state.
        /// </summary>
        public DepthStencilState DepthStencilState { get; }
        
        /// <summary>
        /// Gets the rasterizer state.
        /// </summary>
        public RasterizerState RasterizerState { get; }
        
        /// <summary>
        /// Gets the shader effect.
        /// </summary>
        public Effect Effect { get; }
        
        /// <summary>
        /// Gets the transform matrix.
        /// </summary>
        public Matrix? TransformMatrix { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteBatchSettings"/> class with default values.
        /// </summary>
        public SpriteBatchSettings()
            : this(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteBatchSettings"/> class with specified values.
        /// </summary>
        /// <param name="sortMode">The sprite sort mode.</param>
        /// <param name="blendState">The blend state.</param>
        /// <param name="samplerState">The sampler state.</param>
        /// <param name="depthStencilState">The depth stencil state.</param>
        /// <param name="rasterizerState">The rasterizer state.</param>
        /// <param name="effect">The shader effect.</param>
        /// <param name="transformMatrix">The transform matrix.</param>
        public SpriteBatchSettings(
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null,
            Matrix? transformMatrix = null)
        {
            SortMode = sortMode;
            BlendState = blendState ?? BlendState.AlphaBlend;
            SamplerState = samplerState ?? SamplerState.PointClamp;
            DepthStencilState = depthStencilState;
            RasterizerState = rasterizerState;
            Effect = effect;
            TransformMatrix = transformMatrix;
        }
    }
}