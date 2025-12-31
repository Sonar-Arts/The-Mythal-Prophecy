using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Tests;

namespace TheMythalProphecy.Game.States.Tests
{
    /// <summary>
    /// Test state for rendering a 3D icosahedron with interactive orbit controls.
    /// Demonstrates MonoGame's 3D rendering capabilities using the golden ratio formula.
    /// </summary>
    public class IcosahedronTestState : IGameState
    {
        private readonly ContentManager _content;
        private readonly GameStateManager _stateManager;

        private IcosahedronTestRenderer _testRenderer;
        private SpriteFont _font;
        private KeyboardState _previousKeyState;

        public IcosahedronTestState(ContentManager content, GameStateManager stateManager)
        {
            _content = content;
            _stateManager = stateManager;
        }

        public void Enter()
        {
            _font = _content.Load<SpriteFont>("Fonts/Default");
            _testRenderer = new IcosahedronTestRenderer(GameServices.GraphicsDevice);
            _previousKeyState = Keyboard.GetState();
        }

        public void Exit()
        {
            _testRenderer?.Dispose();
        }

        public void Pause()
        {
            // Nothing to do
        }

        public void Resume()
        {
            _previousKeyState = Keyboard.GetState();
        }

        public void Update(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();

            // ESC to return to options menu
            if (keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape))
            {
                _stateManager.PopState();
                _previousKeyState = keyState;
                return;
            }

            // W to toggle wireframe
            if (keyState.IsKeyDown(Keys.W) && !_previousKeyState.IsKeyDown(Keys.W))
            {
                _testRenderer.Wireframe = !_testRenderer.Wireframe;
            }

            _previousKeyState = keyState;

            _testRenderer.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var graphicsDevice = GameServices.GraphicsDevice;

            // End the sprite batch from game loop
            spriteBatch.End();

            // Clear to dark background
            graphicsDevice.Clear(new Color(15, 10, 25));

            // Draw 3D icosahedron
            _testRenderer.Draw(gameTime);

            // Draw UI overlay with instructions
            spriteBatch.Begin();

            string[] instructions = new string[]
            {
                "ICOSAHEDRON TEST",
                "",
                $"Vertices: {_testRenderer.VertexCount}",
                $"Faces: {_testRenderer.FaceCount}",
                "",
                "Controls:",
                "  Left-click + drag: Orbit camera",
                "  Scroll wheel: Zoom",
                $"  W: Toggle wireframe ({(_testRenderer.Wireframe ? "ON" : "OFF")})",
                "  ESC: Return to menu"
            };

            float y = 20;
            foreach (var line in instructions)
            {
                var color = line == "ICOSAHEDRON TEST" ? new Color(255, 215, 0) : Color.White;
                spriteBatch.DrawString(_font, line, new Vector2(22, y + 2), Color.Black * 0.5f);
                spriteBatch.DrawString(_font, line, new Vector2(20, y), color);
                y += 24;
            }

            spriteBatch.End();

            // Resume sprite batch for game loop
            spriteBatch.Begin();
        }
    }
}
