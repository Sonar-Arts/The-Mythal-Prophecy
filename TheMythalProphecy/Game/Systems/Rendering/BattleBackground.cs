using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TheMythalProphecy.Game.Systems.Rendering
{
    /// <summary>
    /// Manages a multi-layer parallax background for battle scenes
    /// </summary>
    public class BattleBackground : IDisposable
    {
        private readonly List<ParallaxLayer> _layers;
        private Vector2 _cameraOffset;
        private float _elapsedTime;
        private bool _disposed;

        /// <summary>
        /// Name of this background
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Whether this background is visible
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Creates a new battle background with the specified name
        /// </summary>
        public BattleBackground(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _layers = new List<ParallaxLayer>();
            _cameraOffset = Vector2.Zero;
            _elapsedTime = 0f;
        }

        /// <summary>
        /// Adds a parallax layer to this background
        /// </summary>
        public void AddLayer(ParallaxLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            _layers.Add(layer);
        }

        /// <summary>
        /// Removes a parallax layer from this background
        /// </summary>
        public bool RemoveLayer(ParallaxLayer layer)
        {
            return _layers.Remove(layer);
        }

        /// <summary>
        /// Clears all layers from this background
        /// </summary>
        public void ClearLayers()
        {
            _layers.Clear();
        }

        /// <summary>
        /// Gets the number of layers in this background
        /// </summary>
        public int LayerCount => _layers.Count;

        /// <summary>
        /// Updates the background, calculating parallax offsets based on camera position
        /// </summary>
        public void Update(GameTime gameTime, Camera2D camera)
        {
            if (gameTime == null || camera == null)
                return;

            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Store camera offset for parallax calculation
            _cameraOffset = camera.Position;

            // Update auto-scrolling layers
            foreach (var layer in _layers)
            {
                if (layer.AutoScrollSpeed != Vector2.Zero)
                {
                    layer.Offset += layer.AutoScrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }

        /// <summary>
        /// Draws all layers of this background
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Viewport viewport)
        {
            if (!Visible || spriteBatch == null)
                return;

            // Draw layers in order of depth (lowest depth first = back to front)
            foreach (var layer in _layers.OrderBy(l => (int)l.RenderLayer).ThenBy(l => l.Depth))
            {
                if (!layer.Visible || layer.Texture == null)
                    continue;

                // Calculate parallax offset
                Vector2 parallaxOffset = _cameraOffset * layer.ScrollFactor;
                Vector2 position = layer.Offset - parallaxOffset;

                // Calculate depth for SpriteBatch
                float depth = RenderLayerHelper.ToDepth(layer.RenderLayer) + layer.Depth;

                // Wrap position for seamless tiling (optional, for repeating backgrounds)
                // This can be enabled per-layer if needed
                // position.X = position.X % layer.Texture.Width;
                // position.Y = position.Y % layer.Texture.Height;

                // Draw the layer
                spriteBatch.Draw(
                    layer.Texture,
                    position,
                    null,
                    layer.Tint,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    depth
                );
            }
        }

        /// <summary>
        /// Disposes all textures in this background
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            foreach (var layer in _layers)
            {
                layer.Texture?.Dispose();
            }

            _layers.Clear();
            _disposed = true;
        }
    }
}
