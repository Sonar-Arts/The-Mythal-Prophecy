using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Systems.Rendering.Primitives;
using System;

namespace TheMythalProphecy.Game.Tests
{
    /// <summary>
    /// Renders a D2 (coin) with textured number faces.
    /// Click to flip the coin with animation.
    /// </summary>
    public class D2TestRenderer : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _effect;
        private readonly Texture2D _dieTexture;
        private D2Generator.D2Mesh _mesh;

        // Camera orbit parameters
        private float _orbitYaw;
        private float _orbitPitch;
        private float _orbitDistance = 4f;
        private const float OrbitSensitivity = 0.005f;
        private const float MinPitch = -MathHelper.PiOver2 + 0.1f;
        private const float MaxPitch = MathHelper.PiOver2 - 0.1f;

        // Die rotation
        private Matrix _dieRotation = Matrix.Identity;
        private Vector3 _currentRotation = Vector3.Zero;

        // Roll animation
        private bool _isRolling;
        private float _rollTime;
        private float _rollDuration;
        private Vector3 _rollAxis;
        private float _rollSpeed;
        private Vector3 _targetRotation;
        private readonly Random _random = new Random();

        // Mouse state tracking
        private MouseState _previousMouseState;
        private bool _isDragging;

        /// <summary>
        /// Gets the current flip result (top-facing number).
        /// </summary>
        public int CurrentResult { get; private set; } = 1;

        /// <summary>
        /// Gets whether the coin is currently flipping.
        /// </summary>
        public bool IsRolling => _isRolling;

        public D2TestRenderer(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _previousMouseState = Mouse.GetState();

            // Generate D2 texture atlas
            Color dieColor = new Color(180, 180, 190);  // Silver
            Color numberColor = Color.Black;            // Black numbers
            _dieTexture = D2TextureGenerator.Generate(graphicsDevice, font, dieColor, numberColor);

            // Create BasicEffect for textured rendering
            _effect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                Texture = _dieTexture,
                LightingEnabled = true,
                PreferPerPixelLighting = true,
                // Ambient light so numbers are visible from all angles
                AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f)
            };
            _effect.EnableDefaultLighting();

            // Generate D2 mesh with UV coordinates
            _mesh = D2Generator.Generate(graphicsDevice, 1.5f);

            // Initial camera position
            _orbitYaw = MathHelper.ToRadians(30f);
            _orbitPitch = MathHelper.ToRadians(20f);
            UpdateCameraPosition();

            // Set up projection matrix
            float aspectRatio = graphicsDevice.Viewport.AspectRatio;
            _effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                aspectRatio,
                0.1f,
                100f);
        }

        /// <summary>
        /// Start a flip animation.
        /// </summary>
        public void Roll()
        {
            if (_isRolling) return;

            _isRolling = true;
            _rollTime = 0f;
            _rollDuration = 1.5f + (float)_random.NextDouble() * 0.5f;

            // Coin flips primarily around the X or Z axis (horizontal flip)
            float flipAxis = (float)_random.NextDouble() * MathHelper.TwoPi;
            _rollAxis = Vector3.Normalize(new Vector3(
                MathF.Cos(flipAxis),
                0.2f * ((float)_random.NextDouble() - 0.5f), // Small Y component
                MathF.Sin(flipAxis)));

            // Random flip speed (coins flip faster than dice roll)
            _rollSpeed = MathHelper.TwoPi * (4 + (float)_random.NextDouble() * 5);

            // Pre-calculate result
            CurrentResult = _random.Next(1, 3);

            // Calculate target rotation to land on the correct face
            _targetRotation = GetRotationForResult(CurrentResult);
        }

        /// <summary>
        /// Update the renderer (handles input and animation).
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var mouseState = Mouse.GetState();

            // Handle flip animation
            if (_isRolling)
            {
                _rollTime += deltaTime;
                float t = _rollTime / _rollDuration;

                if (t >= 1f)
                {
                    _isRolling = false;
                    _currentRotation = _targetRotation;
                }
                else
                {
                    float easeT = 1f - MathF.Pow(1f - t, 3f);
                    float spinAmount = _rollSpeed * (1f - easeT) * deltaTime;
                    _currentRotation += _rollAxis * spinAmount;

                    if (t > 0.7f)
                    {
                        float lerpT = (t - 0.7f) / 0.3f;
                        _currentRotation = Vector3.Lerp(_currentRotation, _targetRotation, lerpT * 0.1f);
                    }
                }

                _dieRotation = Matrix.CreateRotationX(_currentRotation.X) *
                               Matrix.CreateRotationY(_currentRotation.Y) *
                               Matrix.CreateRotationZ(_currentRotation.Z);
            }

            // Handle mouse input
            if (mouseState.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released &&
                !_isRolling)
            {
                _isDragging = false;
            }
            else if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_isDragging && !_isRolling)
                {
                    int deltaX = mouseState.X - _previousMouseState.X;
                    int deltaY = mouseState.Y - _previousMouseState.Y;

                    if (Math.Abs(deltaX) > 2 || Math.Abs(deltaY) > 2)
                    {
                        _orbitYaw -= deltaX * OrbitSensitivity;
                        _orbitPitch -= deltaY * OrbitSensitivity;
                        _orbitPitch = MathHelper.Clamp(_orbitPitch, MinPitch, MaxPitch);
                        UpdateCameraPosition();
                    }
                }
                else if (!_isDragging)
                {
                    int deltaX = mouseState.X - _previousMouseState.X;
                    int deltaY = mouseState.Y - _previousMouseState.Y;
                    if (Math.Abs(deltaX) > 3 || Math.Abs(deltaY) > 3)
                    {
                        _isDragging = true;
                    }
                }
            }
            else if (mouseState.LeftButton == ButtonState.Released &&
                     _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                if (!_isDragging && !_isRolling)
                {
                    Roll();
                }
                _isDragging = false;
            }

            // Handle scroll wheel for zoom
            int scrollDelta = mouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
            if (scrollDelta != 0)
            {
                _orbitDistance -= scrollDelta * 0.002f;
                _orbitDistance = MathHelper.Clamp(_orbitDistance, 2.5f, 8f);
                UpdateCameraPosition();
            }

            _previousMouseState = mouseState;
        }

        /// <summary>
        /// Draw the D2 coin with textured faces.
        /// </summary>
        public void Draw(GameTime gameTime)
        {
            // Set up graphics device for 3D rendering
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            // Set up effect matrices
            _effect.World = _dieRotation;
            _effect.View = Matrix.CreateLookAt(GetCameraPosition(), Vector3.Zero, Vector3.Up);

            // Set vertex and index buffers
            _graphicsDevice.SetVertexBuffer(_mesh.VertexBuffer);
            _graphicsDevice.Indices = _mesh.IndexBuffer;

            // Draw using each pass of the effect
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    _mesh.IndexBuffer.IndexCount / 3);
            }

            // Reset graphics state for 2D rendering
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        private Vector3 GetCameraPosition()
        {
            float x = _orbitDistance * MathF.Cos(_orbitPitch) * MathF.Sin(_orbitYaw);
            float y = _orbitDistance * MathF.Sin(_orbitPitch);
            float z = _orbitDistance * MathF.Cos(_orbitPitch) * MathF.Cos(_orbitYaw);
            return new Vector3(x, y, z);
        }

        private void UpdateCameraPosition()
        {
            _effect.View = Matrix.CreateLookAt(GetCameraPosition(), Vector3.Zero, Vector3.Up);
        }

        private Vector3 GetRotationForResult(int result)
        {
            // Find which face has this result
            int targetFace = -1;
            for (int i = 0; i < 2; i++)
            {
                if (D2Generator.FaceNumbers[i] == result)
                {
                    targetFace = i;
                    break;
                }
            }

            if (targetFace < 0) return Vector3.Zero;

            // Get the face normal and calculate rotation to point it up
            Vector3 faceNormal = _mesh.FaceNormals[targetFace];

            float randomYaw = (float)_random.NextDouble() * MathHelper.TwoPi;

            // For a coin, we want it to land flat
            // Face 0 (top, normal = Up) needs no X rotation
            // Face 1 (bottom, normal = Down) needs PI X rotation
            float pitch = targetFace == 0 ? 0f : MathHelper.Pi;
            float yaw = randomYaw;
            float roll = 0f;

            return new Vector3(pitch, yaw, roll);
        }

        public void UpdateViewport(Viewport viewport)
        {
            _effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                viewport.AspectRatio,
                0.1f,
                100f);
        }

        public void Dispose()
        {
            _mesh?.Dispose();
            _dieTexture?.Dispose();
            _effect?.Dispose();
        }
    }
}
