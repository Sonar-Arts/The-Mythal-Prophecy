using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Tests;

namespace TheMythalProphecy.Game.States.Tests
{
    /// <summary>
    /// Test state for rendering a 3D D10 die with roll animation.
    /// Click to roll the die, drag to orbit camera.
    /// </summary>
    public class D10TestState : IGameState
    {
        private readonly ContentManager _content;
        private readonly GameStateManager _stateManager;

        private D10TestRenderer _testRenderer;
        private SpriteFont _font;
        private KeyboardState _previousKeyState;

        public D10TestState(ContentManager content, GameStateManager stateManager)
        {
            _content = content;
            _stateManager = stateManager;
        }

        public void Enter()
        {
            _font = _content.Load<SpriteFont>("Fonts/Default");
            _testRenderer = new D10TestRenderer(GameServices.GraphicsDevice, _font);
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

            // Space to roll
            if (keyState.IsKeyDown(Keys.Space) && !_previousKeyState.IsKeyDown(Keys.Space))
            {
                _testRenderer.Roll();
            }

            _previousKeyState = keyState;

            _testRenderer.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var graphicsDevice = GameServices.GraphicsDevice;
            int screenWidth = graphicsDevice.Viewport.Width;

            // End the sprite batch from game loop
            spriteBatch.End();

            // Clear to dark background
            graphicsDevice.Clear(new Color(20, 15, 30));

            // Draw 3D D10 (textured)
            _testRenderer.Draw(gameTime);

            // Draw UI overlay
            spriteBatch.Begin();

            // Title
            string title = "D10 DICE TEST";
            Vector2 titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title, new Vector2(22, 22), Color.Black * 0.5f);
            spriteBatch.DrawString(_font, title, new Vector2(20, 20), new Color(255, 215, 0));

            // Roll result display
            string resultText = _testRenderer.IsRolling ? "Rolling..." : $"Result: {_testRenderer.CurrentResult}";
            Color resultColor = _testRenderer.IsRolling ? Color.White :
                (_testRenderer.CurrentResult == 9 ? new Color(0, 255, 100) :  // Max = green
                 _testRenderer.CurrentResult == 0 ? new Color(255, 80, 80) :   // Min = red
                 Color.White);

            // Center the result at top
            Vector2 resultSize = _font.MeasureString(resultText);
            float resultX = (screenWidth - resultSize.X * 1.5f) / 2f;
            spriteBatch.DrawString(_font, resultText, new Vector2(resultX + 2, 22), Color.Black * 0.5f,
                0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, resultText, new Vector2(resultX, 20), resultColor,
                0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);

            // Instructions
            string[] instructions = new string[]
            {
                "Controls:",
                "  Click / Space: Roll the die",
                "  Drag: Orbit camera",
                "  Scroll: Zoom",
                "  ESC: Return to menu"
            };

            float y = 70;
            foreach (var line in instructions)
            {
                spriteBatch.DrawString(_font, line, new Vector2(22, y + 2), Color.Black * 0.5f);
                spriteBatch.DrawString(_font, line, new Vector2(20, y), Color.White * 0.8f);
                y += 22;
            }

            spriteBatch.End();

            // Resume sprite batch for game loop
            spriteBatch.Begin();
        }
    }
}
