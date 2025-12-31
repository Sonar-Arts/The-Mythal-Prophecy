using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Systems.Rendering;
using TheMythalProphecy.Game.Systems.Rendering.Primitives;
using System;

namespace TheMythalProphecy.Game.Tests
{
    /// <summary>
    /// Renders an icosahedron with interactive orbit camera controls.
    /// Left-click and drag to orbit the camera around the shape.
    /// </summary>
    public class IcosahedronTestRenderer : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Renderer3D _renderer;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

        // Camera orbit parameters
        private float _orbitYaw;      // Horizontal rotation (around Y-axis)
        private float _orbitPitch;    // Vertical rotation
        private float _orbitDistance = 4f;
        private const float OrbitSensitivity = 0.005f;
        private const float MinPitch = -MathHelper.PiOver2 + 0.1f;
        private const float MaxPitch = MathHelper.PiOver2 - 0.1f;

        // Mouse state tracking
        private MouseState _previousMouseState;
        private bool _isDragging;

        /// <summary>
        /// Gets or sets whether wireframe mode is enabled.
        /// </summary>
        public bool Wireframe
        {
            get => _renderer.Wireframe;
            set => _renderer.Wireframe = value;
        }

        /// <summary>
        /// Gets the vertex count of the icosahedron.
        /// </summary>
        public int VertexCount => IcosahedronGenerator.VertexCount;

        /// <summary>
        /// Gets the face count of the icosahedron.
        /// </summary>
        public int FaceCount => IcosahedronGenerator.FaceCount;

        public IcosahedronTestRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _renderer = new Renderer3D(graphicsDevice);
            _previousMouseState = Mouse.GetState();

            // Generate icosahedron mesh
            (_vertexBuffer, _indexBuffer) = IcosahedronGenerator.Generate(graphicsDevice, 1.5f);

            // Initial camera position
            _orbitYaw = MathHelper.ToRadians(30f);
            _orbitPitch = MathHelper.ToRadians(20f);
            UpdateCameraPosition();
        }

        /// <summary>
        /// Update the renderer (handles camera orbit input).
        /// </summary>
        public void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();

            // Check for left mouse button drag
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_isDragging)
                {
                    // Calculate mouse delta
                    int deltaX = mouseState.X - _previousMouseState.X;
                    int deltaY = mouseState.Y - _previousMouseState.Y;

                    // Update orbit angles
                    _orbitYaw -= deltaX * OrbitSensitivity;
                    _orbitPitch -= deltaY * OrbitSensitivity;

                    // Clamp pitch to avoid gimbal lock
                    _orbitPitch = MathHelper.Clamp(_orbitPitch, MinPitch, MaxPitch);

                    UpdateCameraPosition();
                }
                else
                {
                    // Start dragging
                    _isDragging = true;
                }
            }
            else
            {
                _isDragging = false;
            }

            // Handle scroll wheel for zoom
            int scrollDelta = mouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
            if (scrollDelta != 0)
            {
                _orbitDistance -= scrollDelta * 0.002f;
                _orbitDistance = MathHelper.Clamp(_orbitDistance, 2f, 10f);
                UpdateCameraPosition();
            }

            _previousMouseState = mouseState;
        }

        /// <summary>
        /// Draw the icosahedron.
        /// </summary>
        public void Draw(GameTime gameTime)
        {
            _renderer.Begin();
            _renderer.DrawMesh(_vertexBuffer, _indexBuffer, Matrix.Identity);
            _renderer.End();
        }

        /// <summary>
        /// Update viewport when window is resized.
        /// </summary>
        public void UpdateViewport(Viewport viewport)
        {
            _renderer.UpdateViewport(viewport);
        }

        private void UpdateCameraPosition()
        {
            // Calculate camera position on sphere around origin
            float x = _orbitDistance * MathF.Cos(_orbitPitch) * MathF.Sin(_orbitYaw);
            float y = _orbitDistance * MathF.Sin(_orbitPitch);
            float z = _orbitDistance * MathF.Cos(_orbitPitch) * MathF.Cos(_orbitYaw);

            _renderer.SetCamera(new Vector3(x, y, z));
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }
    }
}
