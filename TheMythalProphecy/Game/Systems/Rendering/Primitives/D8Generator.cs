using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering.Primitives
{
    /// <summary>
    /// Generates a D8 (8-sided die) mesh based on the octahedron.
    /// Each face displays a number from 1-8 using texture mapping.
    /// </summary>
    public static class D8Generator
    {
        /// <summary>
        /// Standard D8 face arrangement - opposite faces sum to 9.
        /// Index corresponds to face index, value is the number on that face.
        /// </summary>
        public static readonly int[] FaceNumbers = new int[]
        {
            1, 2, 3, 4,  // Top faces (around top vertex)
            8, 7, 6, 5   // Bottom faces (opposites)
        };

        /// <summary>
        /// 8 triangular faces defined by 3 vertex indices each.
        /// Vertices: 0=top, 1=front, 2=right, 3=back, 4=left, 5=bottom
        /// First vertex determines "up" direction for UV mapping.
        /// Top faces: top vertex first (numbers point up toward apex)
        /// Bottom faces: equatorial vertex first (numbers point up toward equator)
        /// </summary>
        private static readonly int[][] FaceIndices = new int[][]
        {
            // Top 4 faces (connected to top vertex) - apex first
            new[] { 0, 2, 1 },  // Face 0: top-right-front
            new[] { 0, 3, 2 },  // Face 1: top-back-right
            new[] { 0, 4, 3 },  // Face 2: top-left-back
            new[] { 0, 1, 4 },  // Face 3: top-front-left
            // Bottom 4 faces (connected to bottom vertex) - equatorial vertex first
            new[] { 1, 2, 5 },  // Face 4: front-right-bottom (opposite of face 0)
            new[] { 2, 3, 5 },  // Face 5: right-back-bottom (opposite of face 1)
            new[] { 3, 4, 5 },  // Face 6: back-left-bottom (opposite of face 2)
            new[] { 4, 1, 5 }   // Face 7: left-front-bottom (opposite of face 3)
        };

        /// <summary>
        /// D8 mesh result for the mesh generation.
        /// </summary>
        public class D8Mesh : IDisposable
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
        /// Generates a D8 mesh with texture-mapped triangular faces.
        /// </summary>
        public static D8Mesh Generate(GraphicsDevice graphicsDevice, float radius = 1f)
        {
            // Generate the 6 vertices for an octahedron
            Vector3[] basePositions = new Vector3[]
            {
                new Vector3(0, 1, 0) * radius,   // 0: Top
                new Vector3(0, 0, 1) * radius,   // 1: Front
                new Vector3(1, 0, 0) * radius,   // 2: Right
                new Vector3(0, 0, -1) * radius,  // 3: Back
                new Vector3(-1, 0, 0) * radius,  // 4: Left
                new Vector3(0, -1, 0) * radius   // 5: Bottom
            };

            // 8 faces × 3 vertices = 24 vertices
            var vertices = new VertexPositionNormalTexture[24];
            var indices = new short[24];
            var faceCenters = new Vector3[8];
            var faceNormals = new Vector3[8];

            int vertexIndex = 0;

            for (int face = 0; face < 8; face++)
            {
                int[] faceVerts = FaceIndices[face];

                // Get the 3 vertex positions for this face
                Vector3 p0 = basePositions[faceVerts[0]];
                Vector3 p1 = basePositions[faceVerts[1]];
                Vector3 p2 = basePositions[faceVerts[2]];

                // Calculate face center
                Vector3 center = (p0 + p1 + p2) / 3f;
                faceCenters[face] = center;

                // Calculate face normal using cross product
                Vector3 edge1 = p1 - p0;
                Vector3 edge2 = p2 - p0;
                Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

                // Ensure normal points outward (away from origin)
                if (Vector3.Dot(faceNormal, center) < 0)
                {
                    faceNormal = -faceNormal;
                }
                faceNormals[face] = faceNormal;

                // Get UV coordinates for this face's number
                int faceNumber = FaceNumbers[face];
                Vector3[] triangleVerts = { p0, p1, p2 };
                var uvs = D8TextureGenerator.GetFaceUVs(faceNumber, triangleVerts, center, faceNormal);

                // Create vertices for this triangle
                vertices[vertexIndex] = new VertexPositionNormalTexture(p0, faceNormal, uvs[0]);
                vertices[vertexIndex + 1] = new VertexPositionNormalTexture(p1, faceNormal, uvs[1]);
                vertices[vertexIndex + 2] = new VertexPositionNormalTexture(p2, faceNormal, uvs[2]);

                indices[vertexIndex] = (short)vertexIndex;
                indices[vertexIndex + 1] = (short)(vertexIndex + 1);
                indices[vertexIndex + 2] = (short)(vertexIndex + 2);

                vertexIndex += 3;
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

            return new D8Mesh
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
            if (faceIndex < 0 || faceIndex >= 8)
                return 0;
            return FaceNumbers[faceIndex];
        }
    }
}
