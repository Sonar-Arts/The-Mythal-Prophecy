using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering
{
    /// <summary>
    /// Manages 3D rendering operations using MonoGame's built-in 3D support.
    /// Handles BasicEffect setup, depth testing, and mesh rendering.
    /// </summary>
    public class Renderer3D
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _effect;

        /// <summary>
        /// World transformation matrix (rotation, scale, translation of objects)
        /// </summary>
        public Matrix World { get; set; } = Matrix.Identity;

        /// <summary>
        /// View matrix (camera position and orientation)
        /// </summary>
        public Matrix View { get; set; } = Matrix.Identity;

        /// <summary>
        /// Projection matrix (perspective or orthographic)
        /// </summary>
        public Matrix Projection { get; set; } = Matrix.Identity;

        /// <summary>
        /// Whether to render in wireframe mode
        /// </summary>
        public bool Wireframe { get; set; }

        /// <summary>
        /// Whether lighting is enabled
        /// </summary>
        public bool LightingEnabled
        {
            get => _effect.LightingEnabled;
            set => _effect.LightingEnabled = value;
        }

        /// <summary>
        /// Gets the underlying BasicEffect for advanced configuration
        /// </summary>
        public BasicEffect Effect => _effect;

        public Renderer3D(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));

            // Initialize BasicEffect with vertex colors and lighting
            _effect = new BasicEffect(_graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = true,
                PreferPerPixelLighting = true
            };

            // Enable default lighting (3 directional lights)
            _effect.EnableDefaultLighting();

            // Set up default perspective projection
            float aspectRatio = _graphicsDevice.Viewport.AspectRatio;
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                aspectRatio,
                0.1f,
                100f);

            // Default camera looking at origin from Z=5
            View = Matrix.CreateLookAt(
                new Vector3(0, 0, 5f),
                Vector3.Zero,
                Vector3.Up);
        }

        /// <summary>
        /// Begin 3D rendering. Call before drawing 3D geometry.
        /// </summary>
        public void Begin()
        {
            // Enable depth testing
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Set up rasterizer state
            _graphicsDevice.RasterizerState = Wireframe
                ? new RasterizerState { FillMode = FillMode.WireFrame, CullMode = CullMode.None }
                : RasterizerState.CullCounterClockwise;

            // Enable alpha blending for transparency
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        /// <summary>
        /// End 3D rendering. Resets graphics device state for 2D rendering.
        /// </summary>
        public void End()
        {
            // Reset to defaults for 2D rendering
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        /// <summary>
        /// Draw a mesh using vertex and index buffers.
        /// </summary>
        public void DrawMesh(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, Matrix world)
        {
            // Set the vertex and index buffers
            _graphicsDevice.SetVertexBuffer(vertexBuffer);
            _graphicsDevice.Indices = indexBuffer;

            // Update effect matrices
            _effect.World = world;
            _effect.View = View;
            _effect.Projection = Projection;

            // Draw using each pass of the effect
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    indexBuffer.IndexCount / 3);
            }
        }

        /// <summary>
        /// Draw a mesh with the current World matrix.
        /// </summary>
        public void DrawMesh(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            DrawMesh(vertexBuffer, indexBuffer, World);
        }

        /// <summary>
        /// Update projection matrix for viewport changes.
        /// </summary>
        public void UpdateViewport(Viewport viewport)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                viewport.AspectRatio,
                0.1f,
                100f);
        }

        /// <summary>
        /// Set the camera position and target.
        /// </summary>
        public void SetCamera(Vector3 position, Vector3 target, Vector3 up)
        {
            View = Matrix.CreateLookAt(position, target, up);
        }

        /// <summary>
        /// Set the camera position looking at the origin.
        /// </summary>
        public void SetCamera(Vector3 position)
        {
            SetCamera(position, Vector3.Zero, Vector3.Up);
        }
    }
}
