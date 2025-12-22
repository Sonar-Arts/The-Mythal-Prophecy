using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.Systems.Rendering
{
    /// <summary>
    /// Represents a single parallax layer in a battle background
    /// </summary>
    public class ParallaxLayer
    {
        /// <summary>
        /// The texture for this layer
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Base offset for positioning this layer
        /// </summary>
        public Vector2 Offset { get; set; }

        /// <summary>
        /// Parallax scroll factor (0.0 = static, 1.0 = full camera speed)
        /// </summary>
        public float ScrollFactor { get; set; }

        /// <summary>
        /// Render layer this parallax layer belongs to
        /// </summary>
        public RenderLayer RenderLayer { get; set; }

        /// <summary>
        /// Fine-tuned depth within the render layer (0.0 to 0.09)
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// Tint color applied to this layer
        /// </summary>
        public Color Tint { get; set; } = Color.White;

        /// <summary>
        /// Whether this layer is visible
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Optional automatic scroll speed for animated layers (e.g., clouds)
        /// </summary>
        public Vector2 AutoScrollSpeed { get; set; } = Vector2.Zero;

        /// <summary>
        /// Creates a new parallax layer
        /// </summary>
        public ParallaxLayer()
        {
        }

        /// <summary>
        /// Creates a new parallax layer with specified parameters
        /// </summary>
        public ParallaxLayer(Texture2D texture, float scrollFactor, RenderLayer renderLayer, float depth)
        {
            Texture = texture;
            ScrollFactor = scrollFactor;
            RenderLayer = renderLayer;
            Depth = depth;
            Offset = Vector2.Zero;
        }
    }
}
