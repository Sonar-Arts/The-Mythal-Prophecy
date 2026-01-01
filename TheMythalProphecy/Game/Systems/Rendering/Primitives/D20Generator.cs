using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering.Primitives
{
    /// <summary>
    /// Custom vertex structure with position, normal, and texture coordinates.
    /// Used for textured D20 rendering.
    /// </summary>
    public struct VertexPositionNormalTexture : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
        }
    }

    /// <summary>
    /// Generates a D20 (20-sided die) mesh based on the icosahedron.
    /// Each face displays a number from 1-20 using texture mapping.
    /// </summary>
    public static class D20Generator
    {
        /// <summary>
        /// Golden ratio φ = (1 + √5) / 2 ≈ 1.618034
        /// </summary>
        private static readonly float Phi = (1f + MathF.Sqrt(5f)) / 2f;

        /// <summary>
        /// Standard D20 face arrangement - opposite faces sum to 21.
        /// Index corresponds to face index, value is the number on that face.
        /// </summary>
        public static readonly int[] FaceNumbers = new int[]
        {
            20, 2, 8, 14, 10,   // Faces 0-4 (around vertex 0)
            4, 16, 6, 12, 18,   // Faces 5-9 (adjacent ring)
            1, 19, 13, 7, 3,    // Faces 10-14 (around vertex 3)
            17, 11, 15, 9, 5    // Faces 15-19 (adjacent ring)
        };

        /// <summary>
        /// Which vertex (0, 1, or 2) each face's number points toward.
        /// Cap faces point away from the pole vertex, middle ring faces alternate.
        /// </summary>
        public static readonly int[] FaceApexVertices = new int[]
        {
            2, 2, 2, 2, 2,      // Faces 0-4 (around vertex 0): point away from pole
            0, 0, 0, 0, 0,      // Faces 5-9 (adjacent ring): opposite orientation
            2, 2, 2, 2, 2,      // Faces 10-14 (around vertex 3): point away from pole
            0, 0, 0, 0, 0       // Faces 15-19 (adjacent ring): opposite orientation
        };

        /// <summary>
        /// 20 triangular faces defined by vertex indices.
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
        /// D20 result for the mesh generation.
        /// </summary>
        public class D20Mesh : IDisposable
        {
            public VertexBuffer VertexBuffer { get; init; }
            public IndexBuffer IndexBuffer { get; init; }
            public Vector3[] FaceCenters { get; init; }
            public Vector3[] FaceNormals { get; init; }

            public void Dispose()
            {
                VertexBuffer?.Dispose();
                IndexBuffer?.Dispose();
            }
        }

        /// <summary>
        /// Generates a D20 mesh with texture-mapped faces.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to create buffers on.</param>
        /// <param name="radius">The radius of the circumscribed sphere.</param>
        /// <returns>A D20Mesh containing vertex/index buffers and face data.</returns>
        public static D20Mesh Generate(GraphicsDevice graphicsDevice, float radius = 1f)
        {
            // Generate the 12 base vertex positions using golden ratio
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

            // Create vertices with texture coordinates
            var vertices = new VertexPositionNormalTexture[60];
            var indices = new short[60];
            var faceCenters = new Vector3[20];
            var faceNormals = new Vector3[20];

            for (int face = 0; face < 20; face++)
            {
                // Get the 3 vertex indices for this face
                int i0 = FaceIndices[face * 3];
                int i1 = FaceIndices[face * 3 + 1];
                int i2 = FaceIndices[face * 3 + 2];

                Vector3 p0 = basePositions[i0];
                Vector3 p1 = basePositions[i1];
                Vector3 p2 = basePositions[i2];

                // Calculate face center
                faceCenters[face] = (p0 + p1 + p2) / 3f;

                // Calculate face normal (flat shading)
                Vector3 edge1 = p1 - p0;
                Vector3 edge2 = p2 - p0;
                Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
                faceNormals[face] = faceNormal;

                // Get UV coordinates for this face's number, properly oriented
                int faceNumber = FaceNumbers[face];
                int apexVertex = FaceApexVertices[face];
                var (uv0, uv1, uv2) = D20TextureGenerator.GetFaceUVs(faceNumber, p0, p1, p2, faceNormal, apexVertex);

                // Create 3 vertices for this face with texture coordinates
                int vertexIndex = face * 3;
                vertices[vertexIndex] = new VertexPositionNormalTexture(p0, faceNormal, uv0);
                vertices[vertexIndex + 1] = new VertexPositionNormalTexture(p1, faceNormal, uv1);
                vertices[vertexIndex + 2] = new VertexPositionNormalTexture(p2, faceNormal, uv2);

                // Sequential indices
                indices[vertexIndex] = (short)vertexIndex;
                indices[vertexIndex + 1] = (short)(vertexIndex + 1);
                indices[vertexIndex + 2] = (short)(vertexIndex + 2);
            }

            // Create vertex buffer
            var vertexBuffer = new VertexBuffer(
                graphicsDevice,
                VertexPositionNormalTexture.VertexDeclaration,
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

            return new D20Mesh
            {
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
                FaceCenters = faceCenters,
                FaceNormals = faceNormals
            };
        }

        /// <summary>
        /// Gets the number on a specific face.
        /// </summary>
        public static int GetFaceNumber(int faceIndex)
        {
            if (faceIndex < 0 || faceIndex >= 20)
                return 0;
            return FaceNumbers[faceIndex];
        }
    }
}
