using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering.Primitives
{
    /// <summary>
    /// Generates a texture atlas for D2 (coin) faces with numbers 1-2.
    /// Layout is a 2x1 grid where each cell contains one number.
    /// Left half (cell 0) = "1", Right half (cell 1) = "2"
    /// </summary>
    public static class D2TextureGenerator
    {
        public const int CellSize = 256;
        public const int GridColumns = 2;
        public const int GridRows = 1;
        public const int TextureWidth = CellSize * GridColumns;
        public const int TextureHeight = CellSize * GridRows;

        /// <summary>
        /// UV mapping result for a circular face.
        /// </summary>
        public struct FaceUVs
        {
            public Vector2 Center { get; init; }
            public float CellCenterU { get; init; }
            public float CellCenterV { get; init; }
            public float UVRadius { get; init; }

            public Vector2 GetRingUV(float angle)
            {
                // Map angle to UV coordinates within the cell
                float u = CellCenterU + MathF.Cos(angle) * UVRadius;
                float v = CellCenterV + MathF.Sin(angle) * UVRadius;
                return new Vector2(u, v);
            }
        }

        /// <summary>
        /// Generates the D2 texture atlas with numbers 1-2.
        /// </summary>
        public static Texture2D Generate(GraphicsDevice graphicsDevice, SpriteFont font, Color dieColor, Color numberColor)
        {
            var renderTarget = new RenderTarget2D(graphicsDevice, TextureWidth, TextureHeight);
            var spriteBatch = new SpriteBatch(graphicsDevice);
            var pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();

            for (int i = 0; i < 2; i++)
            {
                int col = i % GridColumns;
                int x = col * CellSize;
                int y = 0;
                int number = i + 1;

                // Fill cell with die color
                spriteBatch.Draw(pixel, new Rectangle(x, y, CellSize, CellSize), dieColor);

                // Draw number perfectly centered
                DrawCenteredNumber(spriteBatch, font, number, x, y, CellSize, numberColor);
            }

            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);

            var finalTexture = new Texture2D(graphicsDevice, TextureWidth, TextureHeight);
            Color[] data = new Color[TextureWidth * TextureHeight];
            renderTarget.GetData(data);
            finalTexture.SetData(data);

            renderTarget.Dispose();
            pixel.Dispose();
            spriteBatch.Dispose();

            return finalTexture;
        }

        private static void DrawCenteredNumber(SpriteBatch spriteBatch, SpriteFont font, int number, int cellX, int cellY, int cellSize, Color color)
        {
            string text = number.ToString();

            // Measure the widest possible number for consistent sizing
            Vector2 maxSize = font.MeasureString("2");
            Vector2 textSize = font.MeasureString(text);

            // Scale to fit within 35% of cell size (slightly larger for coin)
            float targetHeight = cellSize * 0.35f;
            float scale = targetHeight / maxSize.Y;

            // Calculate the exact center of the cell
            Vector2 cellCenter = new Vector2(
                cellX + cellSize / 2f,
                cellY + cellSize / 2f
            );

            // Origin is the center of the text
            Vector2 origin = new Vector2(textSize.X / 2f, textSize.Y / 2f);

            // Draw shadow
            spriteBatch.DrawString(font, text,
                cellCenter + new Vector2(2, 2),
                Color.White * 0.5f,
                0f, origin, scale, SpriteEffects.None, 0f);

            // Draw number
            spriteBatch.DrawString(font, text,
                cellCenter,
                color,
                0f, origin, scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Gets UV coordinates for a circular face.
        /// </summary>
        /// <param name="faceNumber">The number (1-2) to display on this face</param>
        /// <param name="radius">The radius of the coin (used for UV scaling)</param>
        /// <returns>FaceUVs struct for mapping vertices</returns>
        public static FaceUVs GetFaceUVs(int faceNumber, float radius)
        {
            if (faceNumber < 1 || faceNumber > 2)
                faceNumber = 1;

            // Get the texture cell bounds for this number
            int index = faceNumber - 1;
            int col = index % GridColumns;

            float cellU0 = (float)col / GridColumns;
            float cellU1 = (float)(col + 1) / GridColumns;
            float cellCenterU = (cellU0 + cellU1) / 2f;
            float cellCenterV = 0.5f; // Single row, so always centered vertically

            // UV radius - how much of the cell to use for the circular face
            // Use about 45% of the cell width for the circle
            float uvRadius = 0.45f / GridColumns;

            return new FaceUVs
            {
                Center = new Vector2(cellCenterU, cellCenterV),
                CellCenterU = cellCenterU,
                CellCenterV = cellCenterV,
                UVRadius = uvRadius
            };
        }
    }
}
