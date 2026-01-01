using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering.Primitives
{
    /// <summary>
    /// Generates a texture atlas for D20 die faces with numbers 1-20.
    /// Layout is a 5x4 grid where each cell contains one number.
    /// </summary>
    public static class D20TextureGenerator
    {
        public const int CellSize = 256;
        public const int GridColumns = 5;
        public const int GridRows = 4;
        public const int TextureWidth = CellSize * GridColumns;
        public const int TextureHeight = CellSize * GridRows;

        /// <summary>
        /// Generates the D20 texture atlas with numbers 1-20.
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

            for (int i = 0; i < 20; i++)
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
            Vector2 maxSize = font.MeasureString("20");
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
        /// Gets UV coordinates for a face, properly oriented based on face geometry.
        /// </summary>
        /// <param name="faceNumber">The number (1-20) to display on this face</param>
        /// <param name="p0">First vertex position</param>
        /// <param name="p1">Second vertex position</param>
        /// <param name="p2">Third vertex position</param>
        /// <param name="faceNormal">The face normal (pointing outward)</param>
        /// <param name="apexVertexIndex">Which vertex (0, 1, or 2) the number points toward</param>
        /// <returns>UV coordinates for the three vertices</returns>
        public static (Vector2 UV0, Vector2 UV1, Vector2 UV2) GetFaceUVs(
            int faceNumber,
            Vector3 p0, Vector3 p1, Vector3 p2,
            Vector3 faceNormal,
            int apexVertexIndex)
        {
            if (faceNumber < 1 || faceNumber > 20)
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

            // Compute face center
            Vector3 faceCenter = (p0 + p1 + p2) / 3f;

            // Get the apex vertex - the number points toward this vertex
            Vector3 apexVertex = apexVertexIndex switch
            {
                0 => p0,
                1 => p1,
                _ => p2
            };

            // "Up" on the face = direction from center toward apex vertex
            Vector3 faceUp = Vector3.Normalize(apexVertex - faceCenter);

            // "Right" on the face = cross product (perpendicular to up, in the face plane)
            Vector3 faceRight = Vector3.Cross(faceNormal, faceUp);
            faceRight = Vector3.Normalize(faceRight);

            // Calculate the size of the face (for UV scaling)
            float edge1 = (p1 - p0).Length();
            float edge2 = (p2 - p0).Length();
            float edge3 = (p2 - p1).Length();
            float faceSize = (edge1 + edge2 + edge3) / 3f;

            // UV scale - how much of the cell to use (smaller = more padding)
            float uvScale = 0.7f / faceSize;
            float cellHalfWidth = (cellU1 - cellU0) / 2f;
            float cellHalfHeight = (cellV1 - cellV0) / 2f;

            // Project each vertex onto the face's local 2D coordinate system
            // and map to UV coordinates
            Vector2 uv0 = ProjectToUV(p0, faceCenter, faceRight, faceUp,
                cellCenterU, cellCenterV, cellHalfWidth, cellHalfHeight, uvScale);
            Vector2 uv1 = ProjectToUV(p1, faceCenter, faceRight, faceUp,
                cellCenterU, cellCenterV, cellHalfWidth, cellHalfHeight, uvScale);
            Vector2 uv2 = ProjectToUV(p2, faceCenter, faceRight, faceUp,
                cellCenterU, cellCenterV, cellHalfWidth, cellHalfHeight, uvScale);

            return (uv0, uv1, uv2);
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
            // Note: V is flipped (1 - v) because texture Y is inverted
            float u = cellCenterU + localX * uvScale * cellHalfWidth * 2f;
            float v = 1f - (cellCenterV + localY * uvScale * cellHalfHeight * 2f);

            return new Vector2(u, v);
        }
    }
}
