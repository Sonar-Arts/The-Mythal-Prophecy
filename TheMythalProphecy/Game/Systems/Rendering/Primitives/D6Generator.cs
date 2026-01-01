using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering.Primitives
{
    /// <summary>
    /// Generates a D6 (6-sided die) mesh based on the cube.
    /// Each face displays a number from 1-6 using texture mapping.
    /// </summary>
    public static class D6Generator
    {
        /// <summary>
        /// Standard D6 face arrangement - opposite faces sum to 7.
        /// Index corresponds to face index, value is the number on that face.
        /// </summary>
        public static readonly int[] FaceNumbers = new int[]
        {
            1,  // Face 0: Front (+Z)
            6,  // Face 1: Back (-Z) - opposite of 1
            2,  // Face 2: Right (+X)
            5,  // Face 3: Left (-X) - opposite of 2
            3,  // Face 4: Top (+Y)
            4   // Face 5: Bottom (-Y) - opposite of 3
        };

        /// <summary>
        /// 6 square faces defined by 4 vertex indices each.
        /// Vertex order for each face: first vertex determines "up" direction for UV mapping.
        /// Vertices are ordered to create counterclockwise winding when viewed from outside.
        /// </summary>
        private static readonly int[][] FaceIndices = new int[][]
        {
            // Face 0: Front (+Z) - top vertex first
            new[] { 3, 2, 1, 0 },  // top-left, top-right, bottom-right, bottom-left
            // Face 1: Back (-Z) - top vertex first
            new[] { 6, 7, 4, 5 },  // top-right, top-left, bottom-left, bottom-right
            // Face 2: Right (+X) - top vertex first
            new[] { 2, 6, 5, 1 },  // front-top, back-top, back-bottom, front-bottom
            // Face 3: Left (-X) - top vertex first
            new[] { 7, 3, 0, 4 },  // back-top, front-top, front-bottom, back-bottom
            // Face 4: Top (+Y) - back vertex first (so numbers read correctly)
            new[] { 7, 6, 2, 3 },  // back-left, back-right, front-right, front-left
            // Face 5: Bottom (-Y) - front vertex first
            new[] { 0, 1, 5, 4 }   // front-left, front-right, back-right, back-left
        };

        /// <summary>
        /// D6 mesh result for the mesh generation.
        /// </summary>
        public class D6Mesh : IDisposable
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
        /// Generates a D6 mesh with texture-mapped square faces.
        /// </summary>
        public static D6Mesh Generate(GraphicsDevice graphicsDevice, float radius = 1f)
        {
            // Half-size of the cube
            float h = radius * 0.6f;

            // 8 vertices of a cube
            Vector3[] basePositions = new Vector3[]
            {
                new Vector3(-h, -h,  h),  // 0: front-bottom-left
                new Vector3( h, -h,  h),  // 1: front-bottom-right
                new Vector3( h,  h,  h),  // 2: front-top-right
                new Vector3(-h,  h,  h),  // 3: front-top-left
                new Vector3(-h, -h, -h),  // 4: back-bottom-left
                new Vector3( h, -h, -h),  // 5: back-bottom-right
                new Vector3( h,  h, -h),  // 6: back-top-right
                new Vector3(-h,  h, -h)   // 7: back-top-left
            };

            // 6 faces × 6 vertices per face (2 triangles) = 36 vertices
            var vertices = new VertexPositionNormalTexture[36];
            var indices = new short[36];
            var faceCenters = new Vector3[6];
            var faceNormals = new Vector3[6];

            int vertexIndex = 0;

            for (int face = 0; face < 6; face++)
            {
                int[] faceVerts = FaceIndices[face];

                // Get the 4 vertex positions for this face
                Vector3 p0 = basePositions[faceVerts[0]];
                Vector3 p1 = basePositions[faceVerts[1]];
                Vector3 p2 = basePositions[faceVerts[2]];
                Vector3 p3 = basePositions[faceVerts[3]];

                // Calculate face center
                Vector3 center = (p0 + p1 + p2 + p3) / 4f;
                faceCenters[face] = center;

                // Calculate face normal using cross product
                Vector3 edge1 = p1 - p0;
                Vector3 edge2 = p3 - p0;
                Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

                // Ensure normal points outward (away from origin)
                if (Vector3.Dot(faceNormal, center) < 0)
                {
                    faceNormal = -faceNormal;
                }
                faceNormals[face] = faceNormal;

                // Get UV coordinates for this face's number
                int faceNumber = FaceNumbers[face];
                Vector3[] quadVerts = { p0, p1, p2, p3 };
                var uvs = D6TextureGenerator.GetFaceUVs(faceNumber, quadVerts, center, faceNormal);

                // Create 2 triangles for this quad face
                // Triangle 1: p0, p1, p2
                vertices[vertexIndex] = new VertexPositionNormalTexture(p0, faceNormal, uvs[0]);
                vertices[vertexIndex + 1] = new VertexPositionNormalTexture(p1, faceNormal, uvs[1]);
                vertices[vertexIndex + 2] = new VertexPositionNormalTexture(p2, faceNormal, uvs[2]);

                // Triangle 2: p0, p2, p3
                vertices[vertexIndex + 3] = new VertexPositionNormalTexture(p0, faceNormal, uvs[0]);
                vertices[vertexIndex + 4] = new VertexPositionNormalTexture(p2, faceNormal, uvs[2]);
                vertices[vertexIndex + 5] = new VertexPositionNormalTexture(p3, faceNormal, uvs[3]);

                for (int i = 0; i < 6; i++)
                {
                    indices[vertexIndex + i] = (short)(vertexIndex + i);
                }

                vertexIndex += 6;
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

            return new D6Mesh
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
            if (faceIndex < 0 || faceIndex >= 6)
                return 0;
            return FaceNumbers[faceIndex];
        }
    }
}
