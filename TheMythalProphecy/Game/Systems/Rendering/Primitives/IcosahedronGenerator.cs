using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering.Primitives
{
    /// <summary>
    /// Generates icosahedron mesh geometry using the golden ratio formula.
    /// An icosahedron has 12 vertices and 20 triangular faces.
    /// </summary>
    public static class IcosahedronGenerator
    {
        /// <summary>
        /// Golden ratio φ = (1 + √5) / 2 ≈ 1.618034
        /// </summary>
        private static readonly float Phi = (1f + MathF.Sqrt(5f)) / 2f;

        /// <summary>
        /// Distinct colors for each of the 20 faces.
        /// </summary>
        private static readonly Color[] FaceColors = new Color[]
        {
            new Color(255, 0, 0),      // Red
            new Color(255, 80, 0),     // Red-Orange
            new Color(255, 160, 0),    // Orange
            new Color(255, 220, 0),    // Golden Yellow
            new Color(200, 255, 0),    // Yellow-Green
            new Color(100, 255, 0),    // Lime
            new Color(0, 255, 50),     // Green
            new Color(0, 255, 150),    // Spring Green
            new Color(0, 255, 255),    // Cyan
            new Color(0, 180, 255),    // Sky Blue
            new Color(0, 100, 255),    // Azure
            new Color(0, 0, 255),      // Blue
            new Color(80, 0, 255),     // Blue-Violet
            new Color(160, 0, 255),    // Violet
            new Color(220, 0, 255),    // Purple
            new Color(255, 0, 220),    // Magenta
            new Color(255, 0, 160),    // Pink
            new Color(255, 0, 100),    // Rose
            new Color(255, 100, 100),  // Coral
            new Color(255, 180, 100),  // Peach
        };

        /// <summary>
        /// 20 triangular faces defined by vertex indices.
        /// Each face is 3 consecutive indices forming a triangle.
        /// Winding order is counter-clockwise for front-facing.
        /// </summary>
        private static readonly int[] FaceIndices = new int[]
        {
            // 5 faces around vertex 0
            0, 11, 5,
            0, 5, 1,
            0, 1, 7,
            0, 7, 10,
            0, 10, 11,
            // 5 adjacent faces
            1, 5, 9,
            5, 11, 4,
            11, 10, 2,
            10, 7, 6,
            7, 1, 8,
            // 5 faces around vertex 3
            3, 9, 4,
            3, 4, 2,
            3, 2, 6,
            3, 6, 8,
            3, 8, 9,
            // 5 adjacent faces
            4, 9, 5,
            2, 4, 11,
            6, 2, 10,
            8, 6, 7,
            9, 8, 1
        };

        /// <summary>
        /// Generates an icosahedron mesh with the specified radius.
        /// Uses the golden ratio formula for vertex positions.
        /// Each face has its own distinct color.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to create buffers on.</param>
        /// <param name="radius">The radius of the circumscribed sphere.</param>
        /// <returns>A tuple containing the vertex buffer and index buffer.</returns>
        public static (VertexBuffer VertexBuffer, IndexBuffer IndexBuffer) Generate(
            GraphicsDevice graphicsDevice,
            float radius = 1f)
        {
            // Generate the 12 base vertex positions using golden ratio
            // Vertices lie on 3 mutually perpendicular golden rectangles
            Vector3[] basePositions = new Vector3[]
            {
                new Vector3(-1, Phi, 0),   // 0
                new Vector3(1, Phi, 0),    // 1
                new Vector3(-1, -Phi, 0),  // 2
                new Vector3(1, -Phi, 0),   // 3
                new Vector3(0, -1, Phi),   // 4
                new Vector3(0, 1, Phi),    // 5
                new Vector3(0, -1, -Phi),  // 6
                new Vector3(0, 1, -Phi),   // 7
                new Vector3(Phi, 0, -1),   // 8
                new Vector3(Phi, 0, 1),    // 9
                new Vector3(-Phi, 0, -1),  // 10
                new Vector3(-Phi, 0, 1)    // 11
            };

            // Normalize vertices to unit sphere, then scale by radius
            for (int i = 0; i < basePositions.Length; i++)
            {
                basePositions[i] = Vector3.Normalize(basePositions[i]) * radius;
            }

            // Create vertices with per-face colors
            // 20 faces × 3 vertices per face = 60 vertices total
            var vertices = new VertexPositionColorNormal[60];
            var indices = new short[60];

            for (int face = 0; face < 20; face++)
            {
                // Get the 3 vertex indices for this face
                int i0 = FaceIndices[face * 3];
                int i1 = FaceIndices[face * 3 + 1];
                int i2 = FaceIndices[face * 3 + 2];

                Vector3 p0 = basePositions[i0];
                Vector3 p1 = basePositions[i1];
                Vector3 p2 = basePositions[i2];

                // Calculate face normal (flat shading)
                Vector3 edge1 = p1 - p0;
                Vector3 edge2 = p2 - p0;
                Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

                // Get the color for this face
                Color faceColor = FaceColors[face];

                // Create 3 vertices for this face
                int vertexIndex = face * 3;
                vertices[vertexIndex] = new VertexPositionColorNormal(p0, faceColor, faceNormal);
                vertices[vertexIndex + 1] = new VertexPositionColorNormal(p1, faceColor, faceNormal);
                vertices[vertexIndex + 2] = new VertexPositionColorNormal(p2, faceColor, faceNormal);

                // Sequential indices
                indices[vertexIndex] = (short)vertexIndex;
                indices[vertexIndex + 1] = (short)(vertexIndex + 1);
                indices[vertexIndex + 2] = (short)(vertexIndex + 2);
            }

            // Create vertex buffer
            var vertexBuffer = new VertexBuffer(
                graphicsDevice,
                VertexPositionColorNormal.VertexDeclaration,
                vertices.Length,
                BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            // Create index buffer
            var indexBuffer = new IndexBuffer(
                graphicsDevice,
                IndexElementSize.SixteenBits,
                indices.Length,
                BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);

            return (vertexBuffer, indexBuffer);
        }

        /// <summary>
        /// Gets the number of vertices in an icosahedron.
        /// </summary>
        public static int VertexCount => 12;

        /// <summary>
        /// Gets the number of faces in an icosahedron.
        /// </summary>
        public static int FaceCount => 20;

        /// <summary>
        /// Gets the number of indices (3 per face).
        /// </summary>
        public static int IndexCount => 60;
    }

    /// <summary>
    /// Custom vertex structure with position, color, and normal.
    /// Required for lighting calculations with vertex colors.
    /// </summary>
    public struct VertexPositionColorNormal : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0));

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal)
        {
            Position = position;
            Color = color;
            Normal = normal;
        }
    }
}
