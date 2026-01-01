using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering.Primitives
{
    /// <summary>
    /// Generates a texture atlas for D6 die faces with numbers 1-6.
    /// Layout is a 3x2 grid where each cell contains one number.
    /// </summary>
    public static class D6TextureGenerator
    {
        public const int CellSize = 256;
        public const int GridColumns = 3;
        public const int GridRows = 2;
        public const int TextureWidth = CellSize * GridColumns;
        public const int TextureHeight = CellSize * GridRows;

        /// <summary>
        /// Generates the D6 texture atlas with numbers 1-6.
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

            for (int i = 0; i < 6; i++)
            {
                int col = i % GridColumns;
                int row = i / GridColumns;
                int x = col * CellSize;
                int y = row * CellSize;
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
            Vector2 maxSize = font.MeasureString("6");
            Vector2 textSize = font.MeasureString(text);

            // Scale to fit within 30% of cell size, based on largest number
            float targetHeight = cellSize * 0.30f;
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
                Color.Black * 0.7f,
                0f, origin, scale, SpriteEffects.None, 0f);

            // Draw number
            spriteBatch.DrawString(font, text,
                cellCenter,
                color,
                0f, origin, scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Gets UV coordinates for a square face, properly oriented based on face geometry.
        /// </summary>
        /// <param name="faceNumber">The number (1-6) to display on this face</param>
        /// <param name="vertices">The 4 vertices of the quad</param>
        /// <param name="faceCenter">The center of the face</param>
        /// <param name="faceNormal">The face normal (pointing outward)</param>
        /// <returns>UV coordinates for the 4 vertices</returns>
        public static Vector2[] GetFaceUVs(
            int faceNumber,
            Vector3[] vertices,
            Vector3 faceCenter,
            Vector3 faceNormal)
        {
            if (faceNumber < 1 || faceNumber > 6)
                faceNumber = 1;

            // Get the texture cell bounds for this number
            int index = faceNumber - 1;
            int col = index % GridColumns;
            int row = index / GridColumns;

            float cellU0 = (float)col / GridColumns;
            float cellV0 = (float)row / GridRows;
            float cellU1 = (float)(col + 1) / GridColumns;
            float cellV1 = (float)(row + 1) / GridRows;
            float cellCenterU = (cellU0 + cellU1) / 2f;
            float cellCenterV = (cellV0 + cellV1) / 2f;

            // Determine "up" direction on the face (toward first vertex)
            Vector3 faceUp = Vector3.Normalize(vertices[0] - faceCenter);

            // "Right" on the face = cross product (perpendicular to up, in the face plane)
            Vector3 faceRight = Vector3.Cross(faceNormal, faceUp);
            faceRight = Vector3.Normalize(faceRight);

            // Calculate the size of the face for UV scaling
            float maxDist = 0f;
            for (int i = 0; i < 4; i++)
            {
                float dist = (vertices[i] - faceCenter).Length();
                if (dist > maxDist) maxDist = dist;
            }

            // UV scale - how much of the cell to use
            float uvScale = 0.40f / maxDist;
            float cellHalfWidth = (cellU1 - cellU0) / 2f;
            float cellHalfHeight = (cellV1 - cellV0) / 2f;

            var uvs = new Vector2[4];

            // Project each vertex onto the face's local 2D coordinate system
            for (int i = 0; i < 4; i++)
            {
                uvs[i] = ProjectToUV(vertices[i], faceCenter, faceRight, faceUp,
                    cellCenterU, cellCenterV, cellHalfWidth, cellHalfHeight, uvScale);
            }

            return uvs;
        }

        private static Vector2 ProjectToUV(
            Vector3 vertex, Vector3 faceCenter,
            Vector3 faceRight, Vector3 faceUp,
            float cellCenterU, float cellCenterV,
            float cellHalfWidth, float cellHalfHeight,
            float uvScale)
        {
            // Vector from face center to vertex
            Vector3 offset = vertex - faceCenter;

            // Project onto face's local axes
            float localX = Vector3.Dot(offset, faceRight);
            float localY = Vector3.Dot(offset, faceUp);

            // Map to UV space (centered in the cell)
            // Negate localX to correct horizontal flip
            float u = cellCenterU - localX * uvScale * cellHalfWidth * 2f;
            float v = 1f - (cellCenterV + localY * uvScale * cellHalfHeight * 2f);

            return new Vector2(u, v);
        }
    }
}
